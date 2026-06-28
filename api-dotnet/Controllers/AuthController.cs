using System.Text.Json;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly JwtService _jwt;
    private readonly IHttpClientFactory _http;

    public AuthController(DatabaseService db, JwtService jwt, IHttpClientFactory http)
    {
        _db = db;
        _jwt = jwt;
        _http = http;
    }

    // GET /api/AccountLogin?code=...&role=...
    [AllowAnonymous]
    [HttpPost("AccountLogin")]
    [HttpGet("AccountLogin")]
    public async Task<IActionResult> AccountLogin(
        [FromQuery] string? code,
        [FromQuery] string? role,
        [FromBody] LoginRequest? body)
    {
        var nationalId = code ?? body?.code;
        var roleParam  = role ?? body?.role;

        if (string.IsNullOrWhiteSpace(nationalId))
            return BadRequest(new { status = false, message = "خطای دریافت کد!" });

        await using var conn = _db.CreateConnection();

        if (!string.IsNullOrWhiteSpace(roleParam))
            await conn.ExecuteAsync(
                "UPDATE users SET roles=@role WHERE national_id=@nid",
                new { role = roleParam, nid = nationalId });

        var user = await conn.QueryRowDict(
            "SELECT * FROM users WHERE national_id=@nid",
            new { nid = nationalId });

        if (user == null)
            return BadRequest(new { status = false, message = "خطای دریافت کاربر!" });

        int    userId     = Convert.ToInt32(user.GetValueOrDefault("id") ?? 0);
        string userRoles  = !string.IsNullOrWhiteSpace(roleParam)
                                ? roleParam
                                : (user.GetValueOrDefault("roles") as string ?? "VOTER");
        string regionName = user.GetValueOrDefault("regionName") as string ?? "";
        if (string.IsNullOrEmpty(regionName))
        {
            int rid = Convert.ToInt32(user.GetValueOrDefault("region_id") ?? 0);
            if (rid > 0)
            {
                var reg = await conn.QueryFirstOrDefaultAsync<string>(
                    "SELECT `name` FROM region WHERE id=@rid", new { rid });
                regionName = reg ?? "";
            }
        }

        var token = _jwt.GenerateToken(nationalId, userRoles, regionName);

        string firstName = user.GetValueOrDefault("first_name") as string ?? "";
        string lastName  = user.GetValueOrDefault("last_name")  as string ?? "";

        return Ok(new
        {
            status = true,
            action = "updated",
            user = new
            {
                id             = nationalId,
                national_id    = nationalId,
                personnel_code = user.GetValueOrDefault("personnel_code"),
                orgPositionDesc= user.GetValueOrDefault("org_position_desc"),
                full_name      = $"{firstName} {lastName}".Trim(),
                roles          = new[] { userRoles },
                userType       = new[] { user.GetValueOrDefault("user_type") },
                regionName,
                regionId       = user.GetValueOrDefault("region_id")
            },
            token
        });
    }

    // POST /api/SsoAccountLogin?code=...
    [AllowAnonymous]
    [HttpPost("SsoAccountLogin")]
    [HttpGet("SsoAccountLogin")]
    public async Task<IActionResult> SsoAccountLogin(
        [FromQuery] string? code,
        [FromBody] SsoLoginRequest? body)
    {
        var ssoCode = code ?? body?.Code;
        if (string.IsNullOrWhiteSpace(ssoCode))
            return BadRequest(new { status = false, message = "خطای دریافت کد!" });

        try
        {
        // ── 1. Call SSO ──────────────────────────────────────────────────────
        string ssoBody;
        try
        {
            using var client = _http.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("code", ssoCode);
            client.DefaultRequestHeaders.Add("Authorization", "Basic MTExOjIyMg==");
            var ssoResp = await client.PostAsync(
                "https://my1.medu.ir/api/sso/UserInfo",
                new StringContent("", System.Text.Encoding.UTF8));
            ssoBody = await ssoResp.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = false, step = "sso_call", message = ex.Message });
        }

        // ── 2. Parse SSO response ────────────────────────────────────────────
        using var doc = JsonDocument.Parse(ssoBody);
        var root = doc.RootElement;

        if (!root.TryGetProperty("resultCode", out var rc) || rc.GetInt32() != 200)
        {
            var msg = root.TryGetProperty("data", out var errData) ? errData.ToString() : "خطای SSO";
            return BadRequest(new { status = false, message = msg });
        }

        var d = root.GetProperty("data");

        static string Str(JsonElement el) =>
            el.ValueKind == JsonValueKind.Number ? el.GetRawText() :
            el.ValueKind == JsonValueKind.String ? el.GetString() ?? "" : el.ToString();

        static string GetStr(JsonElement parent, string key) =>
            parent.TryGetProperty(key, out var v) ? Str(v) : "";

        string nationalId          = GetStr(d, "nationalID");
        string firstName           = GetStr(d, "firstName");
        string lastName            = GetStr(d, "lastName");
        string fatherName          = GetStr(d, "father");
        int    gender              = d.TryGetProperty("gender",               out var p) ? p.GetInt32()  : 0;
        string birthDateRaw        = GetStr(d, "birthDate");
        string birthDate           = birthDateRaw.Length >= 10 ? birthDateRaw[..10] : birthDateRaw;
        string persianBirthDate    = GetStr(d, "persianBirthDate");
        string mobile              = GetStr(d, "mobile");
        int    userType            = d.TryGetProperty("userType",             out p) ? p.GetInt32()      : 0;
        int    verified            = d.TryGetProperty("verified",             out p) && p.GetBoolean() ? 1 : 0;
        string employeeKey         = GetStr(d, "employeeKey");
        long   personnelCode       = d.TryGetProperty("personnelCode",        out p) ? p.GetInt64()      : 0;
        int    orgPositionCode     = d.TryGetProperty("orgPositionCode",      out p) ? p.GetInt32()      : 0;
        string orgPositionDesc     = GetStr(d, "orgPositionDesc");
        int    orgPositionTypeCode = d.TryGetProperty("orgPositionTypeCode",  out p) ? p.GetInt32()      : 0;
        string orgPositionTypeDesc = GetStr(d, "orgPositionTypeDesc");
        int    regionId          = d.TryGetProperty("regionId",             out p) ? p.GetInt32()    : 0;
        int    isForeigner       = d.TryGetProperty("isForeigner",          out p) && p.GetBoolean() ? 1 : 0;
        string ip                = GetStr(d, "ip");

        // ── 3. Insert or Update user ─────────────────────────────────────────
        await using var conn = _db.CreateConnection();

        var existing = await conn.QueryRowDict(
            "SELECT u.id, u.roles, u.regionName, u.region_id FROM users u WHERE u.national_id=@nid",
            new { nid = nationalId });

        string userRoles  = "VOTER";
        string regionName = "";
        int    userId;
        string action;

        if (existing != null)
        {
            userId     = Convert.ToInt32(existing.GetValueOrDefault("id") ?? 0);
            userRoles  = existing.GetValueOrDefault("roles") as string ?? "VOTER";
            regionName = existing.GetValueOrDefault("regionName") as string ?? "";

            // اگه regionName خالیه از جدول region بخون
            if (string.IsNullOrEmpty(regionName))
            {
                int existingRegionId = Convert.ToInt32(existing.GetValueOrDefault("region_id") ?? regionId);
                var reg = await conn.QueryRowDict(
                    "SELECT `name` FROM region WHERE id=@rid", new { rid = existingRegionId > 0 ? existingRegionId : regionId });
                regionName = reg?.GetValueOrDefault("name") as string ?? "";
            }

            await conn.ExecuteAsync(@"
                UPDATE users SET
                    first_name=@fn, last_name=@ln, father_name=@fa, gender=@gen,
                    birth_date=@bd, persian_birth_date=@pbd, mobile=@mob,
                    user_type=@ut, verified=@ver, employee_key=@ek,
                    personnel_code=@pc, org_position_code=@opc, org_position_desc=@opd,
                    org_position_type_code=@optc, org_position_type_desc=@optd,
                    region_id=@rid, regionName=@rn, is_foreigner=@isf, ip_address=@ip, updated_at=NOW()
                WHERE id=@id",
                new { fn=firstName, ln=lastName, fa=fatherName, gen=gender,
                      bd=birthDate, pbd=persianBirthDate, mob=mobile, ut=userType,
                      ver=verified, ek=employeeKey, pc=personnelCode,
                      opc=orgPositionCode, opd=orgPositionDesc,
                      optc=orgPositionTypeCode, optd=orgPositionTypeDesc,
                      rid=regionId, rn=regionName, isf=isForeigner, ip, id=userId });
            action = "updated";
        }
        else
        {
            var reg = await conn.QueryRowDict(
                "SELECT `name` FROM region WHERE id=@rid", new { rid = regionId });
            regionName = reg?.GetValueOrDefault("name") as string ?? "";

            userId = await conn.QuerySingleAsync<int>(@"
                INSERT INTO users (
                    national_id, first_name, last_name, father_name, gender,
                    birth_date, persian_birth_date, mobile, user_type, verified,
                    employee_key, personnel_code, org_position_code, org_position_desc,
                    org_position_type_code, org_position_type_desc, region_id, regionName,
                    is_foreigner, ip_address, created_at, roles
                ) VALUES (
                    @nid, @fn, @ln, @fa, @gen, @bd, @pbd, @mob, @ut, @ver,
                    @ek, @pc, @opc, @opd, @optc, @optd, @rid, @rn,
                    @isf, @ip, NOW(), 'VOTER'
                ); SELECT LAST_INSERT_ID();",
                new { nid=nationalId, fn=firstName, ln=lastName, fa=fatherName, gen=gender,
                      bd=birthDate, pbd=persianBirthDate, mob=mobile, ut=userType,
                      ver=verified, ek=employeeKey, pc=personnelCode,
                      opc=orgPositionCode, opd=orgPositionDesc,
                      optc=orgPositionTypeCode, optd=orgPositionTypeDesc,
                      rid=regionId, rn=regionName, isf=isForeigner, ip });
            action = "inserted";
        }

        // ── 4. Address upsert ────────────────────────────────────────────────
        if (d.TryGetProperty("homeAddress", out var homeAddr))
        {
            string postCode = GetStr(homeAddr, "postCode");
            string address  = GetStr(homeAddr, "address");

            if (!string.IsNullOrEmpty(address) || !string.IsNullOrEmpty(postCode))
            {
                var addrExists = await conn.QueryFirstOrDefaultAsync<int?>(
                    "SELECT id FROM user_addresses WHERE user_id=@uid", new { uid = userId });

                if (addrExists.HasValue)
                    await conn.ExecuteAsync(
                        "UPDATE user_addresses SET post_code=@pc, address=@addr WHERE user_id=@uid",
                        new { pc = postCode, addr = address, uid = userId });
                else
                    await conn.ExecuteAsync(
                        "INSERT INTO user_addresses (user_id, post_code, address) VALUES (@uid, @pc, @addr)",
                        new { uid = userId, pc = postCode, addr = address });
            }
        }

        // ── 5. JWT ───────────────────────────────────────────────────────────
        var token = _jwt.GenerateToken(nationalId, userRoles, regionName);

        return Ok(new
        {
            status = true,
            action,
            user = new
            {
                id              = nationalId,
                national_id     = nationalId,
                personnel_code  = personnelCode,
                orgPositionDesc,
                full_name       = $"{firstName} {lastName}".Trim(),
                roles           = new[] { userRoles },
                userType        = new[] { userType },
                regionName,
                regionId
            },
            token
        });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = false, message = ex.Message, trace = ex.StackTrace });
        }
    }
}

public record LoginRequest(string? code, string? role);
public record SsoLoginRequest(string? Code);

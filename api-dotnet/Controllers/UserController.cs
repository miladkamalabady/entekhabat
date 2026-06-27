using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly JalaliService _jalali;
    private readonly BaleService _bale;

    public UserController(DatabaseService db, JalaliService jalali, BaleService bale)
    {
        _db = db;
        _jalali = jalali;
        _bale = bale;
    }

    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";
    private string UserRole => User.Claims.FirstOrDefault(c => c.Type == "roles")?.Value ?? "";

    private static readonly string[] ValidDegrees = {
        "لیسانس", "کارشناسی", "فوق لیسانس", "کارشناسی ارشد",
        "دکتری", "دکترا", "فوق دکتری", "فوق دکترا", "پست دکترا"
    };

    // GET /api/user-status
    [HttpGet("user-status")]
    public async Task<IActionResult> UserStatus()
    {
        await using var conn = _db.CreateConnection();

        bool inFund = false;
        float yearsOfService = 0f;
        string education = "";
        try
        {
            var check = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT national_id, yearsOfService, education FROM userscheck WHERE national_id=@nid LIMIT 1",
                new { nid = NationalId });
            inFund = check != null;
            yearsOfService = check != null ? Convert.ToSingle(check.yearsOfService ?? 0) : 0f;
            education = ((string?)check?.education)?.Trim() ?? "";
        }
        catch { }

        bool hasMinYears = yearsOfService >= 1f;
        bool hasDegree   = ValidDegrees.Contains(education);

        string? reg = null;
        try
        {
            reg = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT nationalId FROM final_submissions WHERE nationalId=@nid LIMIT 1",
                new { nid = NationalId });
        }
        catch { }

        return Ok(new
        {
            status = true,
            data = new
            {
                membershipActive  = inFund,
                membershipYears   = hasMinYears,
                yearsOfService    = yearsOfService,
                education         = education,
                degree            = hasDegree,
                alreadyRegistered = reg != null
            }
        });
    }

    // GET /api/getUsers?page=1&limit=50&search=
    [HttpGet("getUsers")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int limit = 50, [FromQuery] string? search = null)
    {
        await using var conn = _db.CreateConnection();

        var currentUser = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT u.roles, u.region_id, r.ProvinceCode
              FROM users u LEFT JOIN region r ON r.id=u.region_id
              WHERE u.national_id=@nid LIMIT 1", new { nid = NationalId });

        if (currentUser == null) return Forbid();

        bool isAdmin = (string)currentUser.roles == "ADMIN";
        bool isProvinceSupervisor = (string)currentUser.roles == "SUPERVISOR"
            && ((string)currentUser.region_id?.ToString()).EndsWith("00")
            && currentUser.ProvinceCode != null;

        if (!isAdmin && !isProvinceSupervisor)
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });

        page  = Math.Max(1, page);
        limit = Math.Clamp(limit, 10, 200);
        int offset = (page - 1) * limit;

        var conditions = new List<string>();
        if (isProvinceSupervisor)
            conditions.Add($"r.ProvinceCode = {(int)currentUser.ProvinceCode}");
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Replace("'", "''");
            conditions.Add($@"(u.national_id LIKE '%{s}%' OR u.first_name LIKE '%{s}%'
                               OR u.last_name LIKE '%{s}%' OR u.personnel_code LIKE '%{s}%'
                               OR r.name LIKE '%{s}%')");
        }
        string where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

        string passColumns = isAdmin
            ? @", CASE WHEN u.roles='EXECUTIVE' AND (u.region_id % 100) != 0 THEN fra.EXECUTIVEPass ELSE NULL END AS executivePass,
                 CASE WHEN u.roles='SUPERVISOR' AND (u.region_id % 100) != 0 THEN fra.SUPERVISORPass ELSE NULL END AS supervisorPass"
            : "";
        string passJoin = isAdmin
            ? "LEFT JOIN final_results_approvals fra ON fra.region_id = u.region_id"
            : "";

        var total = await conn.QueryFirstOrDefaultAsync<int>(
            $"SELECT COUNT(*) FROM users u JOIN region r ON r.id=u.region_id {where}");
        int pages = limit > 0 ? (int)Math.Ceiling(total / (double)limit) : 1;

        var sql = $@"SELECT u.id, u.national_id, u.first_name, u.last_name,
                    u.personnel_code, u.region_id, u.roles, u.created_at,
                    r.name AS regionName, r.ProvinceCode AS provinceCode,
                    p.Name AS provinceName,
                    uc.education, uc.yearsOfService{passColumns}
                FROM users u
                JOIN region r ON r.id=u.region_id
                LEFT JOIN region p ON p.id=(r.ProvinceCode * 100)
                LEFT JOIN userscheck uc ON uc.national_id COLLATE utf8mb4_persian_ci=u.national_id
                {passJoin}
                {where}
                ORDER BY u.id DESC LIMIT {limit} OFFSET {offset}";

        var rows = (await conn.QueryAsync<dynamic>(sql)).AsList();
        var list = rows.Select(r =>
        {
            var d = (IDictionary<string, object>)r;
            if (d["created_at"] is DateTime dt)
                d["created_at"] = _jalali.FormatShort(dt);
            return d;
        });

        return Ok(new
        {
            status = true,
            data = list,
            meta = new { total, page, limit, pages }
        });
    }

    // POST /api/updateUser  {national_id, region_id, roles}
    [HttpPost("updateUser")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.national_id) || req.region_id <= 0
            || !new[] { "ADMIN", "SUPERVISOR", "EXECUTIVE", "CANDIDATE", "VOTER" }.Contains(req.roles))
            return BadRequest(new { status = false, message = "اطلاعات ارسالی معتبر نیست." });

        await using var conn = _db.CreateConnection();

        var currentUser = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT u.roles, u.region_id, r.ProvinceCode
              FROM users u LEFT JOIN region r ON r.id=u.region_id
              WHERE u.national_id=@nid LIMIT 1", new { nid = NationalId });

        bool isAdmin = currentUser != null && (string)currentUser.roles == "ADMIN";
        bool isProvinceSupervisor = currentUser != null
            && (string)currentUser.roles == "SUPERVISOR"
            && ((string)currentUser.region_id?.ToString()).EndsWith("00");

        if (!isAdmin && !isProvinceSupervisor)
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });

        var newRegion = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT id, ProvinceCode, Name FROM region WHERE id=@id LIMIT 1", new { id = req.region_id });
        if (newRegion == null)
            return BadRequest(new { status = false, message = "منطقه انتخاب شده معتبر نیست." });

        if (isProvinceSupervisor)
        {
            int provincecode = (int)currentUser.ProvinceCode;
            var target = await conn.QueryFirstOrDefaultAsync<dynamic>(
                @"SELECT u.national_id, r.ProvinceCode FROM users u
                  JOIN region r ON r.id=u.region_id WHERE u.national_id=@nid LIMIT 1",
                new { nid = req.national_id });

            if (target == null
                || (int)target.ProvinceCode != provincecode
                || (int)newRegion.ProvinceCode != provincecode)
                return StatusCode(403, new { status = false, message = "امکان ویرایش کاربران خارج از استان شما وجود ندارد." });
        }

        await conn.ExecuteAsync(
            "UPDATE users SET region_id=@rid, regionName=@rname, roles=@roles WHERE national_id=@nid",
            new { rid = req.region_id, rname = (string)newRegion.Name, roles = req.roles, nid = req.national_id });

        return Ok(new { status = true, message = "ویرایش با موفقیت انجام شد.", data = true });
    }

    // GET /api/getEXECUTIVEList
    [HttpGet("getEXECUTIVEList")]
    public async Task<IActionResult> GetExecutiveList()
    {
        await using var conn = _db.CreateConnection();

        var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT roles, region_id FROM users WHERE national_id=@nid", new { nid = NationalId });

        if (me == null || ((string)me.roles != "EXECUTIVE" && (string)me.roles != "SUPERVISOR"))
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });

        int regionId = (int)me.region_id;

        var rows = (await conn.QueryAsync<dynamic>(
            @"SELECT f.id AS codeentekhabati, tracking_code, requestStatus, f.create_date,
                     u.id, u.national_Id, u.first_name, u.last_name, u.persian_birth_date,
                     u.personnel_code, u.gender, u.father_name, u.org_position_desc,
                     uc.yearsOfService, uc.education, u.user_type, u.region_id,
                     re.name AS regname, u.roles,
                     ud.user_photo, ud.education_doc, ud.employment_cert,
                     ud.soPishine_cert, ud.ravan_cert, ud.document_reviews,
                     ud.updated_at AS datepic,
                     ua.post_code, ua.address, f.reson
              FROM final_submissions f
              JOIN users u ON u.national_id=f.nationalId
              LEFT JOIN region re ON re.id=u.region_id
              JOIN user_documents ud ON ud.nationalId=f.nationalId
              LEFT JOIN user_addresses ua ON ua.user_id=u.id
              LEFT JOIN userscheck uc ON uc.national_id=u.national_id
              WHERE u.region_id=@rid ORDER BY create_date DESC",
            new { rid = regionId })).AsList();

        var list = rows.Select(r =>
        {
            var d = (IDictionary<string, object>)r;
            if (d["create_date"] is DateTime cd)
                d["create_datesh"] = _jalali.FormatShort(cd);
            if (d["datepic"] is DateTime dp)
                d["datepic"] = _jalali.FormatShort(dp);
            return d;
        });

        return Ok(new { status = true, data = list });
    }

    // POST /api/ChangeState  {national_Id, requestStatus, reason}
    [HttpPost("ChangeState")]
    public async Task<IActionResult> ChangeState([FromBody] ChangeStateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.national_Id) || string.IsNullOrWhiteSpace(req.requestStatus))
            return BadRequest(new { status = false, message = "پارامتر کد و وضعیت الزامی است." });

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT roles FROM users WHERE national_id=@nid", new { nid = NationalId });
        string myRole = (string?)me?.roles ?? "";

        if (myRole != "EXECUTIVE" && myRole != "SUPERVISOR")
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });

        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            await conn.ExecuteAsync(
                @"UPDATE final_submissions SET requestStatus=@s, reson=@r, edited_at=NOW()
                  WHERE nationalId=@nid",
                new { s = req.requestStatus, r = req.reason ?? "", nid = req.national_Id }, tx);

            if (myRole == "SUPERVISOR" && (req.requestStatus == "SUPERVISION_APPROVED" || req.requestStatus == "SUPERVISION_REJECTED"))
            {
                await conn.ExecuteAsync(
                    @"UPDATE user_documents
                      SET supervision_status=@s, supervision_reason=@r,
                          supervision_reviewed_by=@by, supervision_reviewed_at=NOW()
                      WHERE nationalId=@nid",
                    new { s = req.requestStatus, r = req.reason ?? "", by = NationalId, nid = req.national_Id }, tx);
            }

            await conn.ExecuteAsync(
                "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'تغییر وضعیت',@desc)",
                new { nid = NationalId, desc = $"تغییر کدملی {req.national_Id} به {req.requestStatus}" }, tx);

            await tx.CommitAsync();

            // پیام بله به کاندید
            var statusMessages = new Dictionary<string, string>
            {
                ["SUPERVISION_APPROVED"]  = "صلاحیت شما توسط ناظر تأیید شد.",
                ["SUPERVISION_REJECTED"]  = "صلاحیت شما توسط ناظر رد شد.",
                ["EXECUTIVE_APPROVED"]    = "مدارک شما توسط اجرایی تأیید شد.",
                ["EXECUTIVE_REJECTED"]    = "مدارک شما توسط اجرایی رد شد.",
            };
            if (statusMessages.TryGetValue(req.requestStatus, out var smsText))
            {
                var mobile = await conn.QueryFirstOrDefaultAsync<string>(
                    "SELECT mobile FROM users WHERE national_id=@nid LIMIT 1", new { nid = req.national_Id });
                if (!string.IsNullOrWhiteSpace(mobile))
                    _bale.SendAsync(mobile, $"سامانه انتخابات: {smsText}");
            }

            return Ok(new
            {
                status = true,
                message = "با موفقیت انجام شد.",
                data = new { nationalId = req.national_Id, requestStatus = req.requestStatus }
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}

public record UpdateUserRequest(string national_id, int region_id, string roles);
public record ChangeStateRequest(string national_Id, string requestStatus, string? reason);

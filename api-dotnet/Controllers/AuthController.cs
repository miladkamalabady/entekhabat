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

    public AuthController(DatabaseService db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
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

        // اگر role ارسال شده، نقش را بروز کن
        if (!string.IsNullOrWhiteSpace(roleParam))
            await conn.ExecuteAsync(
                "UPDATE users SET roles=@role WHERE national_id=@nid",
                new { role = roleParam, nid = nationalId });

        // دریافت کاربر به صورت Dictionary برای دسترسی امن
        var user = await conn.QueryRowDict(
            "SELECT * FROM users WHERE national_id=@nid",
            new { nid = nationalId });

        if (user == null)
            return BadRequest(new { status = false, message = "خطای دریافت کاربر!" });

        // دسترسی امن با GetValueOrDefault
        int    userId     = Convert.ToInt32(user.GetValueOrDefault("id") ?? 0);
        string userRoles  = !string.IsNullOrWhiteSpace(roleParam)
                                ? roleParam
                                : (user.GetValueOrDefault("roles") as string ?? "VOTER");
        string regionName = user.GetValueOrDefault("regionName") as string ?? "";

        // upsert userstatus
        var statusExists = await conn.QueryFirstOrDefaultAsync<int?>(
            "SELECT user_id FROM userstatus WHERE user_id=@uid", new { uid = userId });

        if (statusExists.HasValue)
            await conn.ExecuteAsync(
                "UPDATE userstatus SET nationalId=@nid, ozvsandogh=1, sabegheO=1, madrak=1 WHERE user_id=@uid",
                new { nid = nationalId, uid = userId });
        else
            await conn.ExecuteAsync(
                "INSERT INTO userstatus (user_Id, nationalId, ozvsandogh, sabegheO, madrak) VALUES (@uid,@nid,1,1,1)",
                new { uid = userId, nid = nationalId });

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
}

public record LoginRequest(string? code, string? role);

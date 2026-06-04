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

    // POST /api/AccountLogin  (code, role optional)
    [AllowAnonymous]
    [HttpPost("AccountLogin")]
    [HttpGet("AccountLogin")]
    public async Task<IActionResult> AccountLogin([FromQuery] string? code, [FromQuery] string? role,
        [FromBody] LoginRequest? body)
    {
        // Accept from query or body
        var nationalId = code ?? body?.code;
        var roleParam = role ?? body?.role;

        if (string.IsNullOrWhiteSpace(nationalId))
            return BadRequest(new { status = false, message = "خطای دریافت کد!" });

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            // Optionally update role
            if (!string.IsNullOrWhiteSpace(roleParam))
                await conn.ExecuteAsync(
                    "UPDATE users SET roles=@role WHERE national_id=@nid",
                    new { role = roleParam, nid = nationalId }, tx);

            var user = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM users WHERE national_id=@nid",
                new { nid = nationalId }, tx);

            if (user == null)
                return BadRequest(new { status = false, message = "خطای دریافت کاربر!" });

            int userId = (int)user.id;
            string userRoles = !string.IsNullOrWhiteSpace(roleParam) ? roleParam : (string)user.roles;
            string regionName = (string)(user.regionName ?? "");

            // Upsert userstatus
            var existsStatus = await conn.QueryFirstOrDefaultAsync<int?>(
                "SELECT user_id FROM userstatus WHERE user_id=@uid", new { uid = userId }, tx);

            if (existsStatus.HasValue)
                await conn.ExecuteAsync(
                    "UPDATE userstatus SET nationalId=@nid, ozvsandogh=1, sabegheO=1, madrak=1 WHERE user_id=@uid",
                    new { nid = nationalId, uid = userId }, tx);
            else
                await conn.ExecuteAsync(
                    "INSERT INTO userstatus (user_Id, nationalId, ozvsandogh, sabegheO, madrak) VALUES (@uid,@nid,1,1,1)",
                    new { uid = userId, nid = nationalId }, tx);

            await tx.CommitAsync();

            var token = _jwt.GenerateToken(nationalId, userRoles, regionName);

            return Ok(new
            {
                status = true,
                action = "updated",
                user = new
                {
                    id = nationalId,
                    national_id = nationalId,
                    personnel_code = user.personnel_code,
                    orgPositionDesc = user.org_position_desc,
                    full_name = $"{user.first_name} {user.last_name}".Trim(),
                    roles = new[] { userRoles },
                    userType = new[] { user.user_type },
                    regionName,
                    regionId = user.region_id
                },
                token
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}

public record LoginRequest(string? code, string? role);

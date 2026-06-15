using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class LogFeedbackController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly JalaliService _jalali;

    public LogFeedbackController(DatabaseService db, JalaliService jalali)
    {
        _db = db;
        _jalali = jalali;
    }

    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";

    // GET /api/getLogs?limit=200
    [HttpGet("getLogs")]
    public async Task<IActionResult> GetLogs([FromQuery] int limit = 200)
    {
        await using var conn = _db.CreateConnection();

        var user = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT roles FROM users WHERE national_id=@nid LIMIT 1", new { nid = NationalId });

        if (user == null || (string)user.roles != "ADMIN")
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });

        limit = Math.Clamp(limit <= 0 ? 200 : limit, 1, 1000);

        var rows = (await conn.QueryAsync<dynamic>(
            $"SELECT id, nationalId, action, description, create_date FROM logs ORDER BY id DESC LIMIT {limit}")).AsList();

        var list = rows.Select(r =>
        {
            var d = (IDictionary<string, object>)r;
            if (d["create_date"] is DateTime dt)
                d["create_date_shamsi"] = _jalali.FormatShort(dt);
            return d;
        });

        return Ok(new { status = true, data = list });
    }

    // GET /api/getRecentLogs?limit=20 - فعالیت‌های اخیر برای EXECUTIVE و SUPERVISOR فیلتر شده به منطقه
    [HttpGet("getRecentLogs")]
    public async Task<IActionResult> GetRecentLogs([FromQuery] int limit = 20)
    {
        await using var conn = _db.CreateConnection();

        var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT u.roles, u.region_id, u.first_name, u.last_name, r.ProvinceCode
              FROM users u LEFT JOIN region r ON r.id=u.region_id
              WHERE u.national_id=@nid LIMIT 1", new { nid = NationalId });

        if (me == null) return Unauthorized();

        string myRole = (string)me.roles;
        bool isAdmin = myRole == "ADMIN";
        bool isSupervisor = myRole == "SUPERVISOR";
        bool isExecutive = myRole == "EXECUTIVE";

        if (!isAdmin && !isSupervisor && !isExecutive)
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });

        limit = Math.Clamp(limit <= 0 ? 20 : limit, 1, 100);

        string sql;
        object param;

        if (isAdmin)
        {
            sql = $@"SELECT l.id, l.nationalId, l.action, l.description, l.create_date,
                            u.first_name, u.last_name
                     FROM logs l
                     LEFT JOIN users u ON u.national_id=l.nationalId
                     ORDER BY l.id DESC LIMIT {limit}";
            param = new { };
        }
        else
        {
            int myRegion = (int)me.region_id;
            int myProvince = (int)me.ProvinceCode;
            bool isProvinceSupervisor = isSupervisor && myRegion.ToString().EndsWith("00");

            if (isProvinceSupervisor)
            {
                sql = $@"SELECT l.id, l.nationalId, l.action, l.description, l.create_date,
                                u.first_name, u.last_name
                         FROM logs l
                         LEFT JOIN users u ON u.national_id=l.nationalId
                         LEFT JOIN region r ON r.id=u.region_id
                         WHERE r.ProvinceCode=@pcode OR l.nationalId=@nid
                         ORDER BY l.id DESC LIMIT {limit}";
                param = new { pcode = myProvince, nid = NationalId };
            }
            else
            {
                sql = $@"SELECT l.id, l.nationalId, l.action, l.description, l.create_date,
                                u.first_name, u.last_name
                         FROM logs l
                         LEFT JOIN users u ON u.national_id=l.nationalId
                         WHERE u.region_id=@rid OR l.nationalId=@nid
                         ORDER BY l.id DESC LIMIT {limit}";
                param = new { rid = myRegion, nid = NationalId };
            }
        }

        var rows = (await conn.QueryAsync<dynamic>(sql, param)).AsList();

        var list = rows.Select(r =>
        {
            var d = (IDictionary<string, object>)r;
            if (d["create_date"] is DateTime dt)
                d["create_date_shamsi"] = _jalali.FormatShort(dt);
            return d;
        });

        return Ok(new { status = true, data = list });
    }

    // POST /api/submitFeedback  {rating, comment}
    [HttpPost("submitFeedback")]
    public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackRequest req)
    {
        if (req.rating == 0)
            return BadRequest(new { status = false, message = "پارامتر الزامی است." });

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            var existing = await conn.QueryFirstOrDefaultAsync<int?>(
                "SELECT id FROM feedback WHERE national_id=@nid LIMIT 1", new { nid = NationalId }, tx);

            string message;
            if (!existing.HasValue)
            {
                await conn.ExecuteAsync(
                    "INSERT INTO feedback (national_id, rating, comment) VALUES (@nid, @r, @c)",
                    new { nid = NationalId, r = req.rating, c = req.comment ?? "" }, tx);
                message = "نظر شما با موفقیت ثبت شد.";
            }
            else
            {
                await conn.ExecuteAsync(
                    "UPDATE feedback SET rating=@r, comment=@c WHERE national_id=@nid",
                    new { r = req.rating, c = req.comment ?? "", nid = NationalId }, tx);
                message = "نظر شما با موفقیت ویرایش شد.";
            }

            await tx.CommitAsync();
            return Ok(new { status = true, data = message });
        }
        catch
        {
            await tx.RollbackAsync();
            return StatusCode(500, new { status = false, data = "خطا در ثبت نظر" });
        }
    }
}

public record FeedbackRequest(int rating, string? comment);

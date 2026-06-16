using System.Text.Json;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class AnnouncementController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly JalaliService _jalali;

    public AnnouncementController(DatabaseService db, JalaliService jalali)
    {
        _db = db;
        _jalali = jalali;
    }

    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";

    private const string CreateTable = @"CREATE TABLE IF NOT EXISTS announcements (
        id INT(11) NOT NULL AUTO_INCREMENT,
        title VARCHAR(255) NOT NULL,
        content TEXT NOT NULL,
        target_scope VARCHAR(20) NOT NULL DEFAULT 'region',
        target_ids JSON NULL,
        created_by VARCHAR(20) NOT NULL,
        is_active TINYINT(1) NOT NULL DEFAULT 1,
        created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
        updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
        PRIMARY KEY (id)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_persian_ci";

    // GET /api/getPublicAnnouncements — بدون نیاز به لاگین (فقط سراسری)
    [AllowAnonymous]
    [HttpGet("getPublicAnnouncements")]
    public async Task<IActionResult> GetPublicAnnouncements()
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(CreateTable);

        var rows = (await conn.QueryAsync<dynamic>(@"
            SELECT a.id, a.title, a.content, a.target_scope, a.created_at
            FROM announcements a
            WHERE a.is_active = 1 AND a.target_scope = 'country'
            ORDER BY a.created_at DESC
            LIMIT 10")).AsList();

        var list = rows.Select(r =>
        {
            var d = (IDictionary<string, object>)r;
            if (d["created_at"] is DateTime dt)
                d["created_at_shamsi"] = _jalali.Format(dt, "Y/n/j");
            return d;
        });

        return Ok(new { status = true, data = list });
    }

    // GET /api/getAnnouncements
    [HttpGet("getAnnouncements")]
    public async Task<IActionResult> GetAnnouncements()
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(CreateTable);

        var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT u.region_id, r.ProvinceCode FROM users u
              LEFT JOIN region r ON r.id=u.region_id
              WHERE u.national_id=@nid LIMIT 1", new { nid = NationalId });

        if (me == null) return Unauthorized();

        int myRegion   = Convert.ToInt32(me.region_id   ?? 0);
        int myProvince = Convert.ToInt32(me.ProvinceCode ?? 0);

        var rows = (await conn.QueryAsync<dynamic>(@"
            SELECT a.id, a.title, a.content, a.target_scope, a.target_ids,
                   a.created_by, a.is_active, a.created_at,
                   u.first_name, u.last_name
            FROM announcements a
            LEFT JOIN users u ON u.national_id = a.created_by COLLATE utf8mb4_persian_ci
            WHERE a.is_active = 1
              AND (
                  a.target_scope = 'country'
                  OR (a.target_scope = 'province' AND JSON_CONTAINS(a.target_ids, CAST(@pcode AS CHAR)))
                  OR (a.target_scope = 'region'   AND JSON_CONTAINS(a.target_ids, CAST(@rid  AS CHAR)))
              )
            ORDER BY a.created_at DESC",
            new { pcode = myProvince, rid = myRegion })).AsList();

        var list = rows.Select(r =>
        {
            var d = (IDictionary<string, object>)r;
            if (d["created_at"] is DateTime dt)
            {
                d["created_at_shamsi"] = _jalali.Format(dt, "Y/n/j");
                d["created_at_full"]   = _jalali.Format(dt, "H:i  Y/n/j");
            }
            // target_ids از دیتابیس رشته JSON است — به آرایه تبدیل کن
            if (d["target_ids"] is string tj)
                d["target_ids"] = JsonSerializer.Deserialize<int[]>(tj) ?? Array.Empty<int>();
            return d;
        });

        return Ok(new { status = true, data = list });
    }

    // GET /api/getMyAnnouncements
    [HttpGet("getMyAnnouncements")]
    public async Task<IActionResult> GetMyAnnouncements()
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(CreateTable);

        var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT roles FROM users WHERE national_id=@nid LIMIT 1", new { nid = NationalId });
        if (me == null) return Unauthorized();

        string myRole = (string)me.roles;
        if (!new[] { "ADMIN", "SUPERVISOR", "EXECUTIVE" }.Contains(myRole))
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید." });

        string sql = myRole == "ADMIN"
            ? @"SELECT a.*, u.first_name, u.last_name
                FROM announcements a
                LEFT JOIN users u ON u.national_id=a.created_by COLLATE utf8mb4_persian_ci
                ORDER BY a.created_at DESC"
            : @"SELECT a.*, u.first_name, u.last_name
                FROM announcements a
                LEFT JOIN users u ON u.national_id=a.created_by COLLATE utf8mb4_persian_ci
                WHERE a.created_by=@nid
                ORDER BY a.created_at DESC";

        var rows = (await conn.QueryAsync<dynamic>(sql, new { nid = NationalId })).AsList();

        var list = rows.Select(r =>
        {
            var d = (IDictionary<string, object>)r;
            if (d["created_at"] is DateTime dt)
                d["created_at_shamsi"] = _jalali.Format(dt, "Y/n/j");
            if (d.TryGetValue("target_ids", out var tj) && tj is string tjs)
                d["target_ids"] = JsonSerializer.Deserialize<int[]>(tjs) ?? Array.Empty<int>();
            return d;
        });

        return Ok(new { status = true, data = list });
    }

    // POST /api/saveAnnouncement
    [HttpPost("saveAnnouncement")]
    public async Task<IActionResult> SaveAnnouncement([FromBody] SaveAnnouncementRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.title) || string.IsNullOrWhiteSpace(req.content) || string.IsNullOrWhiteSpace(req.target_scope))
            return BadRequest(new { status = false, message = "عنوان، متن و محدوده اطلاعیه الزامی است." });

        if (!new[] { "country", "province", "region" }.Contains(req.target_scope))
            return BadRequest(new { status = false, message = "محدوده نامعتبر است." });

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        await conn.ExecuteAsync(CreateTable);

        var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT u.roles, u.region_id, r.ProvinceCode FROM users u
              LEFT JOIN region r ON r.id=u.region_id
              WHERE u.national_id=@nid LIMIT 1", new { nid = NationalId });

        if (me == null) return Unauthorized();

        string myRole    = (string)me.roles;
        int myRegion     = Convert.ToInt32(me.region_id   ?? 0);
        int myProvince   = Convert.ToInt32(me.ProvinceCode ?? 0);
        bool isAdmin     = myRole == "ADMIN";
        bool isSupervisor = myRole == "SUPERVISOR";
        bool isExecutive  = myRole == "EXECUTIVE";
        bool isProvinceSupervisor = isSupervisor && myRegion.ToString().EndsWith("00");

        if (!isAdmin && !isSupervisor && !isExecutive)
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید." });

        // تعیین target_ids نهایی
        int[] finalIds;
        if (req.target_scope == "country")
        {
            if (!isAdmin)
                return StatusCode(403, new { status = false, message = "فقط ادمین می‌تواند اطلاعیه سراسری ایجاد کند." });
            finalIds = Array.Empty<int>();
        }
        else if (req.target_scope == "province")
        {
            if (isExecutive)
                return StatusCode(403, new { status = false, message = "اجرایی نمی‌تواند اطلاعیه استانی ایجاد کند." });

            if (isAdmin)
            {
                // ادمین هر استانی را می‌تواند انتخاب کند
                finalIds = req.target_ids ?? Array.Empty<int>();
                if (finalIds.Length == 0)
                    return BadRequest(new { status = false, message = "حداقل یک استان را انتخاب کنید." });
            }
            else
            {
                // ناظر استانی فقط استان خودش
                if (!isProvinceSupervisor)
                    return StatusCode(403, new { status = false, message = "فقط ناظر استانی می‌تواند اطلاعیه استانی ایجاد کند." });
                finalIds = new[] { myProvince };
            }
        }
        else // region
        {
            if (isAdmin)
            {
                finalIds = req.target_ids ?? Array.Empty<int>();
                if (finalIds.Length == 0)
                    return BadRequest(new { status = false, message = "حداقل یک منطقه را انتخاب کنید." });
            }
            else if (isProvinceSupervisor)
            {
                // ناظر استانی می‌تواند چند منطقه از استان خودش انتخاب کند
                var ids = req.target_ids ?? Array.Empty<int>();
                if (ids.Length == 0) ids = new[] { myRegion };

                // اعتبارسنجی: همه مناطق باید در استان کاربر باشند
                var allInProvince = await conn.QueryFirstOrDefaultAsync<int>(
                    $@"SELECT COUNT(*) FROM region
                       WHERE id IN ({string.Join(",", ids)}) AND ProvinceCode != @pcode",
                    new { pcode = myProvince });
                if (allInProvince > 0)
                    return StatusCode(403, new { status = false, message = "فقط می‌توانید مناطق استان خود را انتخاب کنید." });

                finalIds = ids;
            }
            else
            {
                // اجرایی یا ناظر منطقه‌ای: فقط منطقه خودش
                finalIds = new[] { myRegion };
            }
        }

        string targetIdsJson = JsonSerializer.Serialize(finalIds);

        return await DbHelper.WithTransaction(conn, async tx =>
        {
            int id = req.id ?? 0;
            if (id > 0)
            {
                var existing = await conn.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT created_by FROM announcements WHERE id=@id LIMIT 1", new { id }, tx);
                if (existing == null)
                    return NotFound(new { status = false, message = "اطلاعیه یافت نشد." });
                if (!isAdmin && (string)existing.created_by != NationalId)
                    return StatusCode(403, new { status = false, message = "شما فقط می‌توانید اطلاعیه‌های خود را ویرایش کنید." });

                await conn.ExecuteAsync(
                    "UPDATE announcements SET title=@t, content=@c, target_scope=@s, target_ids=@ids WHERE id=@id",
                    new { t = req.title, c = req.content, s = req.target_scope, ids = targetIdsJson, id }, tx);
            }
            else
            {
                await conn.ExecuteAsync(
                    "INSERT INTO announcements (title, content, target_scope, target_ids, created_by) VALUES (@t,@c,@s,@ids,@by)",
                    new { t = req.title, c = req.content, s = req.target_scope, ids = targetIdsJson, by = NationalId }, tx);
                id = Convert.ToInt32(await conn.ExecuteScalarAsync("SELECT LAST_INSERT_ID()", transaction: tx));
            }

            await conn.ExecuteAsync(
                "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'ذخیره اطلاعیه',@desc)",
                new { nid = NationalId, desc = $"اطلاعیه '{req.title}' — محدوده: {req.target_scope}" }, tx);

            return Ok(new { status = true, message = "اطلاعیه با موفقیت ذخیره شد.", data = new { id } });
        });
    }

    // POST /api/deleteAnnouncement
    [HttpPost("deleteAnnouncement")]
    public async Task<IActionResult> DeleteAnnouncement([FromBody] DeleteAnnouncementRequest req)
    {
        if (req.id <= 0)
            return BadRequest(new { status = false, message = "شناسه اطلاعیه الزامی است." });

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT roles FROM users WHERE national_id=@nid LIMIT 1", new { nid = NationalId });
        if (me == null) return Unauthorized();

        bool isAdmin = (string)me.roles == "ADMIN";

        var ann = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT id, created_by, title FROM announcements WHERE id=@id AND is_active=1 LIMIT 1", new { id = req.id });
        if (ann == null)
            return NotFound(new { status = false, message = "اطلاعیه یافت نشد." });

        if (!isAdmin && (string)ann.created_by != NationalId)
            return StatusCode(403, new { status = false, message = "شما فقط می‌توانید اطلاعیه‌های خود را حذف کنید." });

        await conn.ExecuteAsync("UPDATE announcements SET is_active=0 WHERE id=@id", new { id = req.id });
        await conn.ExecuteAsync(
            "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'حذف اطلاعیه',@desc)",
            new { nid = NationalId, desc = $"اطلاعیه '{ann.title}' حذف شد" });

        return Ok(new { status = true, message = "اطلاعیه با موفقیت حذف شد." });
    }
}

public record SaveAnnouncementRequest(int? id, string? title, string? content, string? target_scope, int[]? target_ids);
public record DeleteAnnouncementRequest(int id);

using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ElectionController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly JalaliService _jalali;

    public ElectionController(DatabaseService db, JalaliService jalali)
    {
        _db = db;
        _jalali = jalali;
    }

    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";

    // GET /api/getConfig
    [HttpGet("getConfig")]
    public async Task<IActionResult> GetConfig()
    {
        await using var conn = _db.CreateConnection();

        var row = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT start_date, end_date FROM election_schedule_events WHERE event_key='voting' LIMIT 1");

        if (row == null)
            return BadRequest(new { status = false, message = "زمان‌بندی انتخابات تنظیم نشده است" });

        var now = DateTime.Now;
        var startDt = _jalali.NormalizeToGregorian((string?)row.start_date);
        var endDt = _jalali.NormalizeToGregorian((string?)row.end_date);
        int isActive = startDt.HasValue && endDt.HasValue && startDt <= now && now <= endDt ? 1 : 0;

        var cfg = new
        {
            id = 1,
            startDate = startDt?.ToString("yyyy-MM-dd HH:mm:ss"),
            EndDate = endDt?.ToString("yyyy-MM-dd HH:mm:ss"),
            create_date = (string?)null,
            active = isActive
        };

        return Ok(new
        {
            status = true,
            data = new
            {
                cfg.id, cfg.startDate, cfg.EndDate, cfg.create_date, cfg.active,
                startDates = startDt.HasValue ? _jalali.Format(startDt.Value, "l j F Y") : null,
                startTime = startDt.HasValue ? _jalali.Format(startDt.Value, "H:i") : null,
                endDates = endDt.HasValue ? _jalali.Format(endDt.Value, "l j F Y") : null,
                endTime = endDt.HasValue ? _jalali.Format(endDt.Value, "H:i") : null
            }
        });
    }

    // GET /api/getSystemSchedule
    [HttpGet("getSystemSchedule")]
    public async Task<IActionResult> GetSystemSchedule()
    {
        await using var conn = _db.CreateConnection();

        await conn.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS election_schedule_events (
            id INT(11) NOT NULL AUTO_INCREMENT,
            event_key VARCHAR(100) NOT NULL,
            event_name VARCHAR(255) NOT NULL,
            start_date DATETIME NULL,
            end_date DATETIME NULL,
            sort_order INT(11) NOT NULL DEFAULT 0,
            updated_by VARCHAR(20) DEFAULT NULL,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            UNIQUE KEY uniq_event_key (event_key)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

        var data = await conn.QueryAsync<dynamic>(
            "SELECT event_key, event_name, start_date, end_date, sort_order FROM election_schedule_events ORDER BY sort_order ASC, id ASC");

        return Ok(new { status = true, data });
    }

    // POST /api/saveSystemSchedule  {events: [{key, name, startDate, endDate, id}]}
    [HttpPost("saveSystemSchedule")]
    public async Task<IActionResult> SaveSystemSchedule([FromBody] SaveScheduleRequest req)
    {
        if (req.events == null || req.events.Length == 0)
            return BadRequest(new { status = false, message = "لیست رویدادها ارسال نشده است." });

        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS election_schedule_events (
            id INT(11) NOT NULL AUTO_INCREMENT,
            event_key VARCHAR(100) NOT NULL,
            event_name VARCHAR(255) NOT NULL,
            start_date DATETIME NULL,
            end_date DATETIME NULL,
            sort_order INT(11) NOT NULL DEFAULT 0,
            updated_by VARCHAR(20) DEFAULT NULL,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            UNIQUE KEY uniq_event_key (event_key)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            var sorted = req.events.OrderBy(e => e.id ?? 0).ToList();

            foreach (var ev in sorted)
            {
                if (string.IsNullOrWhiteSpace(ev.key) || string.IsNullOrWhiteSpace(ev.name)) continue;

                await conn.ExecuteAsync(
                    @"INSERT INTO election_schedule_events (event_key, event_name, start_date, end_date, sort_order, updated_by)
                      VALUES (@key, @name, @sd, @ed, @so, @by)
                      ON DUPLICATE KEY UPDATE
                        event_name=VALUES(event_name),
                        start_date=VALUES(start_date),
                        end_date=VALUES(end_date),
                        sort_order=VALUES(sort_order),
                        updated_by=VALUES(updated_by)",
                    new { key = ev.key, name = ev.name, sd = ev.startDate, ed = ev.endDate, so = ev.id ?? 0, by = NationalId }, tx);
            }

            await tx.CommitAsync();
            return Ok(new { status = true, message = "زمان‌بندی با موفقیت ذخیره شد." });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return BadRequest(new { status = false, message = ex.Message });
        }
    }

    // GET /api/getRegions
    [AllowAnonymous]
    [HttpGet("getRegions")]
    public async Task<IActionResult> GetRegions()
    {
        await using var conn = _db.CreateConnection();

        var rows = (await conn.QueryAsync<dynamic>(
            @"SELECT r.id, r.ProvinceCode, r.Name AS name, COALESCE(mv.maxVotes,1) AS maxVotes
              FROM region r
              LEFT JOIN (SELECT region_id, MAX(maxVotes) AS maxVotes FROM maxvotes GROUP BY region_id) mv ON mv.region_id=r.id
              ORDER BY r.ProvinceCode ASC, r.id ASC, r.Name ASC")).AsList();

        var provincesMap = new Dictionary<int, object>();
        var areasByProvince = new Dictionary<int, List<object>>();

        foreach (var r in rows)
        {
            int pcode = (int)r.ProvinceCode;
            string rid = r.id.ToString();

            if (rid.EndsWith("00"))
                provincesMap[pcode] = new { id = pcode, name = (string)r.name };

            if (!areasByProvince.ContainsKey(pcode))
                areasByProvince[pcode] = new List<object>();

            areasByProvince[pcode].Add(new { id = rid, name = (string)r.name, maxVotes = (int)r.maxVotes });
        }

        return Ok(new
        {
            status = true,
            data = provincesMap.Values,
            areasByProvince
        });
    }

    // POST /api/saveRegionMaxVotes  {region_id, maxVotes}
    [HttpPost("saveRegionMaxVotes")]
    public async Task<IActionResult> SaveRegionMaxVotes([FromBody] SaveMaxVotesRequest req)
    {
        if (req.region_id <= 0 || req.maxVotes <= 0 || req.maxVotes > 50)
            return BadRequest(new { status = false, message = "منطقه یا تعداد رأی مجاز معتبر نیست." });

        await using var conn = _db.CreateConnection();

        var currentUser = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT u.roles, u.region_id, r.ProvinceCode
              FROM users u LEFT JOIN region r ON r.id=u.region_id
              WHERE u.national_id=@nid LIMIT 1", new { nid = NationalId });

        bool isAdmin = currentUser != null && (string)currentUser.roles == "ADMIN";
        bool isProvinceSupervisor = currentUser != null
            && (string)currentUser.roles == "SUPERVISOR"
            && ((string)currentUser.region_id?.ToString()).EndsWith("00")
            && currentUser.ProvinceCode != null;

        if (!isAdmin && !isProvinceSupervisor)
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید." });

        var region = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT id, ProvinceCode, Name FROM region WHERE id=@id LIMIT 1", new { id = req.region_id });
        if (region == null)
            return BadRequest(new { status = false, message = "منطقه انتخاب شده معتبر نیست." });

        if (isProvinceSupervisor && (int)region.ProvinceCode != (int)currentUser.ProvinceCode)
            return StatusCode(403, new { status = false, message = "امکان ویرایش مناطق خارج از استان شما وجود ندارد." });

        var existing = await conn.QueryFirstOrDefaultAsync<int?>(
            "SELECT id FROM maxvotes WHERE region_id=@rid LIMIT 1", new { rid = req.region_id });

        if (existing.HasValue)
            await conn.ExecuteAsync("UPDATE maxvotes SET maxVotes=@mv WHERE region_id=@rid",
                new { mv = req.maxVotes, rid = req.region_id });
        else
            await conn.ExecuteAsync("INSERT INTO maxvotes (maxVotes, region_id) VALUES (@mv, @rid)",
                new { mv = req.maxVotes, rid = req.region_id });

        await conn.ExecuteAsync(
            "INSERT INTO logs (nationalId, action, description) VALUES (@nid, 'تنظیم تعداد رأی منطقه', @desc)",
            new { nid = NationalId, desc = $"تعداد رأی مجاز منطقه {region.Name} ({req.region_id}) به {req.maxVotes} تغییر کرد" });

        return Ok(new
        {
            status = true,
            message = "تعداد رأی مجاز منطقه با موفقیت ذخیره شد.",
            data = new { region_id = req.region_id, maxVotes = req.maxVotes }
        });
    }
}

public record SaveScheduleRequest(ScheduleEvent[]? events);
public record ScheduleEvent(string? key, string? name, string? startDate, string? endDate, int? id);
public record SaveMaxVotesRequest(int region_id, int maxVotes);

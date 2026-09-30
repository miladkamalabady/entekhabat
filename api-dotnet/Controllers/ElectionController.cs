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

        var row = await conn.QueryRowDict(
            "SELECT start_date, end_date FROM election_schedule_events WHERE event_key='voting'");

        if (row == null)
            return BadRequest(new { status = false, message = "زمان‌بندی انتخابات تنظیم نشده است" });

        var now = DateTime.Now;
        var startDt = _jalali.NormalizeToGregorian(row.GetValueOrDefault("start_date"));
        var endDt   = _jalali.NormalizeToGregorian(row.GetValueOrDefault("end_date"));
        int isActive = startDt.HasValue && endDt.HasValue && startDt <= now && now <= endDt ? 1 : 0;

        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var cfg = new
        {
            id = 1,
            startDate = startDt?.ToString("yyyy-MM-dd HH:mm:ss", inv),
            EndDate = endDt?.ToString("yyyy-MM-dd HH:mm:ss", inv),
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

        await conn.ExecuteAsync(@"IF OBJECT_ID(N'dbo.election_schedule_events', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.election_schedule_events (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        event_key NVARCHAR(100) NOT NULL UNIQUE,
        event_name NVARCHAR(255) NOT NULL,
        start_date DATETIME2 NULL, end_date DATETIME2 NULL,
        sort_order INT NOT NULL DEFAULT 0, updated_by NVARCHAR(20) NULL,
        created_at DATETIME2 NOT NULL DEFAULT GETDATE(), updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END");

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
        await conn.OpenAsync();

        await conn.ExecuteAsync(@"IF OBJECT_ID(N'dbo.election_schedule_events', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.election_schedule_events (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        event_key NVARCHAR(100) NOT NULL UNIQUE,
        event_name NVARCHAR(255) NOT NULL,
        start_date DATETIME2 NULL, end_date DATETIME2 NULL,
        sort_order INT NOT NULL DEFAULT 0, updated_by NVARCHAR(20) NULL,
        created_at DATETIME2 NOT NULL DEFAULT GETDATE(), updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END");

        var sorted = req.events.OrderBy(e => e.id ?? 0).ToList();

        return await DbHelper.WithTransaction(conn, async tx =>
        {
            foreach (var ev in sorted)
            {
                if (string.IsNullOrWhiteSpace(ev.key) || string.IsNullOrWhiteSpace(ev.name)) continue;

                await conn.ExecuteAsync(
                    @"IF EXISTS (SELECT 1 FROM dbo.election_schedule_events WHERE event_key=@key)
                      UPDATE dbo.election_schedule_events
                      SET event_name=@name,start_date=@sd,end_date=@ed,sort_order=@so,updated_by=@by,updated_at=GETDATE()
                      WHERE event_key=@key;
                      ELSE
                      INSERT INTO dbo.election_schedule_events (event_key,event_name,start_date,end_date,sort_order,updated_by)
                      VALUES (@key,@name,@sd,@ed,@so,@by);",
                    new { key = ev.key, name = ev.name, sd = ev.startDate, ed = ev.endDate, so = ev.id ?? 0, by = NationalId }, tx);
            }
            return Ok(new { status = true, message = "زمان‌بندی با موفقیت ذخیره شد." });
        });
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
              WHERE u.national_id=@nid", new { nid = NationalId });

        bool isAdmin = currentUser != null && (string)currentUser.roles == "ADMIN";
        bool isProvinceSupervisor = currentUser != null
            && (string)currentUser.roles == "SUPERVISOR"
            && ((string)currentUser.region_id?.ToString()).EndsWith("00")
            && currentUser.ProvinceCode != null;

        if (!isAdmin && !isProvinceSupervisor)
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید." });

        var region = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT id, ProvinceCode, Name FROM region WHERE id=@id", new { id = req.region_id });
        if (region == null)
            return BadRequest(new { status = false, message = "منطقه انتخاب شده معتبر نیست." });

        if (isProvinceSupervisor && (int)region.ProvinceCode != (int)currentUser.ProvinceCode)
            return StatusCode(403, new { status = false, message = "امکان ویرایش مناطق خارج از استان شما وجود ندارد." });

        var existing = await conn.QueryFirstOrDefaultAsync<int?>(
            "SELECT id FROM maxvotes WHERE region_id=@rid", new { rid = req.region_id });

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

    private const string CreateInvalidationsTable = @"IF OBJECT_ID(N'dbo.election_invalidations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.election_invalidations (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY, region_id INT NULL, reason NVARCHAR(MAX) NOT NULL,
        invalidated_by NVARCHAR(20) NOT NULL, created_at DATETIME2 NOT NULL DEFAULT GETDATE());
END";

    // POST /api/invalidateElection  {region_id?, reason}
    [HttpPost("invalidateElection")]
    public async Task<IActionResult> InvalidateElection([FromBody] InvalidateElectionRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.reason))
            return BadRequest(new { status = false, message = "دلیل ابطال الزامی است." });

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT u.roles, u.region_id, r.ProvinceCode
              FROM users u LEFT JOIN region r ON r.id=u.region_id
              WHERE u.national_id=@nid", new { nid = NationalId });

        if (me == null) return Unauthorized();

        string myRole = (string)me.roles;
        bool isAdmin = myRole == "ADMIN";
        bool isSupervisor = myRole == "SUPERVISOR";

        if (!isAdmin && !isSupervisor)
            return StatusCode(403, new { status = false, message = "فقط ناظر یا ادمین می‌تواند انتخابات را باطل کند." });

        await conn.ExecuteAsync(CreateInvalidationsTable);

        int targetRegion = req.region_id ?? 0;

        if (!isAdmin && isSupervisor && targetRegion > 0)
        {
            int myRegion = (int)me.region_id;
            int myProvince = (int)me.ProvinceCode;
            bool isProvinceSupervisor = myRegion.ToString().EndsWith("00");

            if (!isProvinceSupervisor)
            {
                if (targetRegion != myRegion)
                    return StatusCode(403, new { status = false, message = "شما فقط می‌توانید منطقه خود را باطل کنید." });
            }
            else
            {
                var targetReg = await conn.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT ProvinceCode FROM region WHERE id=@id", new { id = targetRegion });
                if (targetReg == null || (int)targetReg.ProvinceCode != myProvince)
                    return StatusCode(403, new { status = false, message = "امکان ابطال مناطق خارج از استان شما وجود ندارد." });
            }
        }

        return await DbHelper.WithTransaction(conn, async tx =>
        {
            await conn.ExecuteAsync(
                "INSERT INTO election_invalidations (region_id, reason, invalidated_by) VALUES (@rid, @reason, @by)",
                new { rid = targetRegion == 0 ? (object)DBNull.Value : targetRegion, reason = req.reason, by = NationalId }, tx);

            await conn.ExecuteAsync(
                "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'ابطال انتخابات',@desc)",
                new { nid = NationalId, desc = $"انتخابات منطقه {targetRegion} باطل شد - دلیل: {req.reason}" }, tx);

            return Ok(new { status = true, message = "انتخابات با موفقیت باطل اعلام شد.", data = new { region_id = targetRegion, reason = req.reason } });
        });
    }

    // GET /api/getElectionInvalidations
    [HttpGet("getElectionInvalidations")]
    public async Task<IActionResult> GetElectionInvalidations()
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(CreateInvalidationsTable);

        var rows = await conn.QueryAsync<dynamic>(@"
            SELECT ei.id, ei.region_id, ei.reason, ei.invalidated_by, ei.created_at,
                   r.Name AS region_name, u.first_name, u.last_name
            FROM election_invalidations ei
            LEFT JOIN region r ON r.id=ei.region_id
            LEFT JOIN users u ON u.national_id=ei.invalidated_by
            ORDER BY ei.created_at DESC");

        return Ok(new { status = true, data = rows });
    }
}

public record SaveScheduleRequest(ScheduleEvent[]? events);
public record ScheduleEvent(string? key, string? name, string? startDate, string? endDate, int? id);
public record SaveMaxVotesRequest(int region_id, int maxVotes);
public record InvalidateElectionRequest(int? region_id, string? reason);

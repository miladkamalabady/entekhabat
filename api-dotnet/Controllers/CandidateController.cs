using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class CandidateController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly JalaliService _jalali;

    public CandidateController(DatabaseService db, JalaliService jalali)
    {
        _db = db;
        _jalali = jalali;
    }

    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";

    // GET /api/getCandidsList
    [HttpGet("getCandidsList")]
    public async Task<IActionResult> GetCandidsList()
    {
        await using var conn = _db.CreateConnection();

        // Check voting schedule
        var schedule = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT start_date, end_date FROM election_schedule_events WHERE event_key='voting' LIMIT 1");

        if (schedule == null)
            return BadRequest(new { status = false, message = "زمان‌بندی انتخابات تنظیم نشده است" });

        var startDt = _jalali.NormalizeToGregorian((string?)schedule.start_date);
        var endDt = _jalali.NormalizeToGregorian((string?)schedule.end_date);

        if (!startDt.HasValue || !endDt.HasValue)
            return BadRequest(new { status = false, message = "زمان‌بندی انتخابات تنظیم نشده است" });

        var now = DateTime.Now;
        if (now < startDt.Value)
        {
            var remaining = startDt.Value - now;
            return BadRequest(new
            {
                status = false,
                message = "زمان انتخابات فرا نرسیده است",
                remaining_time = new
                {
                    days = (int)remaining.TotalDays,
                    hours = remaining.Hours,
                    minutes = remaining.Minutes,
                    total_seconds = (int)remaining.TotalSeconds
                }
            });
        }

        var rows = (await conn.QueryAsync<dynamic>(
            @"SELECT f.id AS codeentekhabati, tracking_code, f.create_date,
                     u.id, u.national_Id, u.first_name, u.last_name,
                     u.persian_birth_date, u.personnel_code, u.gender,
                     u.father_name, u.org_position_desc, u.regionName,
                     ud.user_photo, ua.post_code, ua.address
              FROM final_submissions f
              JOIN users u ON u.national_id=f.nationalId
              JOIN user_documents ud ON ud.nationalId=f.nationalId
              LEFT JOIN user_addresses ua ON ua.user_id=u.id
              WHERE requestStatus='SUPERVISION_APPROVED'
              ORDER BY create_date DESC")).AsList();

        var list = rows.Select(r =>
        {
            var d = (IDictionary<string, object>)r;
            if (d["create_date"] is DateTime cd)
                d["create_datesh"] = _jalali.Format(cd, "H:i Y-n-j ");
            return d;
        });

        return Ok(new { status = true, data = list });
    }

    // GET /api/getstateCandid
    [HttpGet("getstateCandid")]
    public async Task<IActionResult> GetStateCandid()
    {
        await using var conn = _db.CreateConnection();

        // Check if edited_at column exists
        var hasEditedAt = await conn.QueryFirstOrDefaultAsync<string>(
            "SHOW COLUMNS FROM final_submissions LIKE 'edited_at'");
        var editedExpr = hasEditedAt != null ? "edited_at" : "NULL AS edited_at";

        var row = await conn.QueryFirstOrDefaultAsync<dynamic>(
            $"SELECT requestStatus, tracking_code, reson, {editedExpr} FROM final_submissions WHERE nationalId=@nid",
            new { nid = NationalId });

        if (row == null)
            return NotFound(new { status = false, message = "وضعیت یافت نشد!" });

        DateTime? editedAt = row.edited_at;
        return Ok(new
        {
            status = true,
            data = new
            {
                requestStatus = row.requestStatus,
                tracking_code = row.tracking_code,
                reson = row.reson,
                edited_at = row.edited_at,
                edited_at_sh = editedAt.HasValue ? _jalali.Format(editedAt.Value, "H:i Y-n-j") : null
            }
        });
    }

    // POST /api/FinalSubmit  {tracking_code}
    [HttpPost("FinalSubmit")]
    public async Task<IActionResult> FinalSubmit([FromBody] FinalSubmitRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.tracking_code))
            return BadRequest(new { status = false, message = "پارامتر tracking_code الزامی است." });

        await using var conn = _db.CreateConnection();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            // Check registration schedule
            var ev = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT start_date, end_date FROM election_schedule_events WHERE event_key='candidate_registration' LIMIT 1",
                transaction: tx);

            if (ev == null)
                return StatusCode(403, new { status = false, message = "زمان ثبت‌نام انتخابات در سیستم تعریف نشده است." });

            var start = _jalali.ParseJalaliSchedule((string?)ev.start_date);
            var end = _jalali.ParseJalaliSchedule((string?)ev.end_date);
            var now = DateTime.Now;

            if (!start.HasValue || now < start.Value)
            {
                await tx.RollbackAsync();
                return StatusCode(403, new { status = false, message = "هنوز مهلت ثبت‌نام داوطلبان آغاز نشده است." });
            }
            if (!end.HasValue || now > end.Value)
            {
                await tx.RollbackAsync();
                return StatusCode(403, new { status = false, message = "مهلت ثبت‌نام داوطلبان به پایان رسیده است." });
            }

            // Check if already submitted
            var existing = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT nationalId FROM final_submissions WHERE nationalId=@nid", new { nid = NationalId }, tx);
            if (existing != null)
            {
                await tx.RollbackAsync();
                return StatusCode(404, new { status = false, message = "این کاربر قبلا ثبت نام کرده است.!" });
            }

            await conn.ExecuteAsync(
                @"INSERT INTO final_submissions (nationalId, tracking_code, requestStatus)
                  VALUES (@nid, @tc, 'SUBMITTED')
                  ON DUPLICATE KEY UPDATE tracking_code=VALUES(tracking_code), create_date=NOW()",
                new { nid = NationalId, tc = req.tracking_code }, tx);

            await conn.ExecuteAsync(
                "UPDATE users SET roles='CANDIDATE' WHERE national_id=@nid",
                new { nid = NationalId }, tx);

            await conn.ExecuteAsync(
                "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'ثبت کاندید',@desc)",
                new { nid = NationalId, desc = $"تغییر کد {NationalId} ثبت نام کرد" }, tx);

            await tx.CommitAsync();
            return Ok(new
            {
                status = true,
                message = "ثبت نهایی با موفقیت انجام شد.",
                data = new { nationalId = NationalId, tracking_code = req.tracking_code }
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // POST /api/canselRequestCANDIDATE
    [HttpPost("canselRequestCANDIDATE")]
    public async Task<IActionResult> CancelCandidateRequest()
    {
        await using var conn = _db.CreateConnection();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            var existing = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT nationalId FROM final_submissions WHERE nationalId=@nid", new { nid = NationalId }, tx);
            if (existing == null)
            {
                await tx.RollbackAsync();
                return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });
            }

            var ev = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT start_date, end_date FROM election_schedule_events WHERE event_key='voting' LIMIT 1",
                transaction: tx);
            if (ev == null)
            {
                await tx.RollbackAsync();
                return StatusCode(403, new { status = false, message = "زمان انتخابات در سیستم تعریف نشده است." });
            }

            var votingStart = _jalali.ParseJalaliSchedule((string?)ev.start_date);
            if (!votingStart.HasValue)
            {
                await tx.RollbackAsync();
                return StatusCode(403, new { status = false, message = "زمان انتخابات در سیستم تعریف نشده است." });
            }

            var deadline = votingStart.Value.AddHours(-48);
            if (DateTime.Now > deadline)
            {
                await tx.RollbackAsync();
                return StatusCode(403, new { status = false, message = "زمان مجاز انصراف، 48 ساعت قبل شروع انتخابات می‌باشد." });
            }

            await conn.ExecuteAsync("DELETE FROM final_submissions WHERE nationalId=@nid", new { nid = NationalId }, tx);
            await conn.ExecuteAsync("UPDATE users SET roles='VOTER' WHERE national_id=@nid", new { nid = NationalId }, tx);
            await conn.ExecuteAsync(
                "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'حذف کاندید','حذف کاندید توسط خودش')",
                new { nid = NationalId }, tx);

            await tx.CommitAsync();
            return Ok(new { status = true, message = "با موفقیت انجام شد." + NationalId });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}

public record FinalSubmitRequest(string tracking_code);

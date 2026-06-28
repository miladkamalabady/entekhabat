using System.Text.RegularExpressions;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class AdvertisementController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly JalaliService _jalali;
    private readonly IWebHostEnvironment _env;

    public AdvertisementController(DatabaseService db, JalaliService jalali, IWebHostEnvironment env)
    {
        _db = db;
        _jalali = jalali;
        _env = env;
    }

    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";
    private string UserRole => User.Claims.FirstOrDefault(c => c.Type == "roles")?.Value ?? "";

    private static readonly string[] AllowedImages = { "image/jpeg", "image/png", "image/jpg" };

    // POST /api/advertisementsSave  (multipart/form-data)
    [HttpPost("advertisementsSave")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> AdvertisementsSave(
        [FromForm] AdvSaveRequest? formReq,
        [FromForm] IFormFile? image)
    {
        var req = formReq;
        if (req == null)
            return BadRequest(new { status = false, message = "داده ارسال نشده." });

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        // Check advertising schedule
        var adsStartRow = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT start_date FROM election_schedule_events WHERE event_key='ads_upload_start' LIMIT 1");
        var votingRow = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT start_date FROM election_schedule_events WHERE event_key='voting' LIMIT 1");

        if (adsStartRow != null && votingRow != null)
        {
            DateTime? adsStart = _jalali.NormalizeToGregorian((object?)adsStartRow.start_date);
            DateTime? votingStart = _jalali.NormalizeToGregorian((object?)votingRow.start_date);
            if (adsStart.HasValue && votingStart.HasValue)
            {
                var adsEnd = new[] { adsStart.Value.AddDays(7), votingStart.Value.AddDays(-1) }.Min();
                var now = DateTime.Now;
                if (now < adsStart.Value || now > adsEnd)
                    return StatusCode(403, new { status = false, message = "امکان ارسال تبلیغ خارج از تاریخ بازه قانونی وجود ندارد." });
            }
        }

        if (string.IsNullOrWhiteSpace(req.title) || string.IsNullOrWhiteSpace(req.description)
            || string.IsNullOrWhiteSpace(req.type) || string.IsNullOrWhiteSpace(req.status))
            return BadRequest(new { status = false, message = "تمام فیلدهای ضروری باید ارسال شوند." });

        int id = req.id ?? 0;

        // Check if second ad requires payment
        if (id <= 0)
        {
            var existingCount = await conn.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(*) FROM advertisements WHERE nationalId=@nid AND deleter IS NULL", new { nid = NationalId });
            if (existingCount >= 1 && req.isPaid != 1)
                return StatusCode(402, new { status = false, message = "ثبت تبلیغ دوم نیازمند پرداخت است.", requiresPayment = true });
        }

        string? storedImagePath = string.IsNullOrWhiteSpace(req.imagePath) ? null : req.imagePath;

        // Handle image upload
        if (image != null && image.Length > 0)
        {
            if (image.Length > 2 * 1024 * 1024)
                return BadRequest(new { status = false, message = "حجم تصویر نباید بیشتر از 2 مگابایت باشد." });
            if (!AllowedImages.Contains(image.ContentType))
                return BadRequest(new { status = false, message = "فرمت تصویر مجاز نیست." });

            var dir = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads", "advertisements");
            Directory.CreateDirectory(dir);
            var ext = Path.GetExtension(image.FileName).ToLower().TrimStart('.');
            if (string.IsNullOrEmpty(ext)) ext = image.ContentType == "image/png" ? "png" : "jpg";
            var fileName = $"ad_{DateTime.Now:yyyyMMdd_HHmmss}_{Random.Shared.Next(0xFFFF):X4}.{ext}";
            await using var fs = System.IO.File.Create(Path.Combine(dir, fileName));
            await image.CopyToAsync(fs);
            storedImagePath = $"uploads/advertisements/{fileName}";
        }

        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            await conn.ExecuteAsync(
                @"INSERT INTO advertisements (id, nationalId, title, description, type, image, target_link, status,
                    managerialRecords, academicRecords, honors, plans, slogan)
                  VALUES (@id, @nid, @title, @desc, @type, @img, @link, 'pending',
                    @mgr, @acad, @hon, @plans, @slogan)
                  ON DUPLICATE KEY UPDATE
                    title=VALUES(title), description=VALUES(description), type=VALUES(type),
                    image=IF(VALUES(image) IS NULL, image, VALUES(image)),
                    target_link=VALUES(target_link), managerialRecords=VALUES(managerialRecords),
                    academicRecords=VALUES(academicRecords), honors=VALUES(honors),
                    plans=VALUES(plans), slogan=VALUES(slogan), status=VALUES(status)",
                new
                {
                    id, nid = NationalId, title = req.title, desc = req.description,
                    type = req.type, img = storedImagePath ?? "", link = req.targetLink ?? "",
                    mgr = req.managerialRecords ?? "", acad = req.academicRecords ?? "",
                    hon = req.honors ?? "", plans = req.plans ?? "", slogan = req.slogan ?? ""
                }, tx);

            long insertId = id > 0 ? id : Convert.ToInt64(await conn.ExecuteScalarAsync("SELECT LAST_INSERT_ID()", transaction: tx));
            await tx.CommitAsync();

            return Ok(new { status = true, message = "تبلیغ با موفقیت ذخیره شد.", data = new { id = insertId, image = storedImagePath } });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // GET /api/getAdvertisements
    [HttpGet("getAdvertisements")]
    public async Task<IActionResult> GetAdvertisements()
    {
        await using var conn = _db.CreateConnection();

        var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT roles, region_id FROM users WHERE national_id=@nid", new { nid = NationalId });
        if (me == null) return Unauthorized();

        string roles = (string)me.roles;
        int regionId = (int)me.region_id;

        string sql = roles == "CANDIDATE"
            ? @"SELECT f.id AS codeentekhabati, ad.*, u.first_name, u.last_name, u.id AS code,
                       re.name AS regionName, u.user_type, uc.education, uc.yearsOfService
                FROM advertisements ad
                JOIN users u ON u.national_id=ad.nationalId
                LEFT JOIN final_submissions f ON ad.nationalId=f.nationalId
                LEFT JOIN region re ON re.id=u.region_id
                LEFT JOIN userscheck uc ON uc.national_id=u.national_id
                WHERE ad.nationalId=@nid ORDER BY ad.create_date DESC"
            : @"SELECT f.id AS codeentekhabati, ad.*, u.first_name, u.last_name, u.id AS code,
                       re.name AS regionName, u.user_type, uc.education, uc.yearsOfService
                FROM advertisements ad
                JOIN users u ON u.national_id=ad.nationalId
                LEFT JOIN final_submissions f ON ad.nationalId=f.nationalId
                LEFT JOIN region re ON re.id=u.region_id
                LEFT JOIN userscheck uc ON uc.national_id=u.national_id
                WHERE u.region_id=@rid ORDER BY ad.create_date DESC";

        var param = roles == "CANDIDATE"
            ? (object)new { nid = NationalId }
            : new { rid = regionId };

        var rows = (await conn.QueryAsync<dynamic>(sql, param)).AsList();
        var list = rows.Select(r =>
        {
            var d = (IDictionary<string, object>)r;
            if (d["create_date"] is DateTime cd) d["create_atsh"] = _jalali.Format(cd, "H:i Y-n-j ");
            if (d["updated_at"] is DateTime ud) d["updated_atsh"] = _jalali.Format(ud, "H:i Y-n-j");
            return d;
        });

        return Ok(new { status = true, data = list });
    }

    // POST /api/deleteAdv  {code, reson, status}
    [HttpPost("deleteAdv")]
    public async Task<IActionResult> DeleteAdv([FromBody] DeleteAdvRequest? req)
    {
        if (req == null || req.code == null)
            return BadRequest(new { status = false, message = "پارامتر کد الزامی است." });

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT roles FROM users WHERE national_id=@nid", new { nid = NationalId }, tx);

            if (me == null || !new[] { "EXECUTIVE", "SUPERVISOR", "CANDIDATE" }.Contains((string)me.roles))
                return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });

            string myRole = (string)me.roles;

            string selectSql = myRole == "CANDIDATE"
                ? "SELECT deleter FROM advertisements WHERE nationalId=@nid AND id=@id"
                : "SELECT deleter FROM advertisements WHERE id=@id";

            var adRow = await conn.QueryFirstOrDefaultAsync<dynamic>(selectSql,
                new { id = req.code, nid = NationalId }, tx);
            if (adRow == null)
                return NotFound(new { status = false, message = "تبلیغ یافت نشد." });

            string? deleter = (string?)adRow.deleter;
            string? newDeleter = deleter == myRole ? null : myRole;

            string updateSql = !string.IsNullOrEmpty(req.status)
                ? "UPDATE advertisements SET deleter=NULL, reson=NULL, status='active' WHERE id=@id"
                : "UPDATE advertisements SET deleter=@d, reson=@r WHERE id=@id";

            await conn.ExecuteAsync(updateSql, new { id = req.code, d = newDeleter, r = req.reson ?? "" }, tx);
            await conn.ExecuteAsync(
                "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'تغییر وضعیت تبلیغ',@desc)",
                new { nid = NationalId, desc = $"تغییر کد {req.code} به {newDeleter}" }, tx);

            await tx.CommitAsync();
            return Ok(new { status = true, message = "با موفقیت انجام شد.", data = new { id = req.code } });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // POST /api/approveAdvertisement  {id, status, reason}
    // status: "active" = تایید، "rejected" = رد
    [HttpPost("approveAdvertisement")]
    public async Task<IActionResult> ApproveAdvertisement([FromBody] ApproveAdvRequest? req)
    {
        if (req == null || req.id <= 0 || string.IsNullOrWhiteSpace(req.status))
            return BadRequest(new { status = false, message = "پارامترهای ضروری ارسال نشده‌اند." });

        if (req.status != "active" && req.status != "rejected")
            return BadRequest(new { status = false, message = "وضعیت باید 'active' یا 'rejected' باشد." });

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT roles, region_id FROM users WHERE national_id=@nid", new { nid = NationalId });
        if (me == null) return Unauthorized();

        string myRole = (string)me.roles;
        if (!new[] { "SUPERVISOR", "EXECUTIVE", "ADMIN" }.Contains(myRole))
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید." });

        var ad = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT id, nationalId, status FROM advertisements WHERE id=@id LIMIT 1", new { id = req.id });
        if (ad == null)
            return NotFound(new { status = false, message = "تبلیغ یافت نشد." });

        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            if (req.status == "active")
                await conn.ExecuteAsync(
                    "UPDATE advertisements SET status='active', deleter=NULL, reson=NULL WHERE id=@id",
                    new { id = req.id }, tx);
            else
                await conn.ExecuteAsync(
                    "UPDATE advertisements SET status='rejected', deleter=@role, reson=@reason WHERE id=@id",
                    new { id = req.id, role = myRole, reason = req.reason ?? "" }, tx);

            await conn.ExecuteAsync(
                "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'بررسی تبلیغ',@desc)",
                new { nid = NationalId, desc = $"تبلیغ {req.id} به وضعیت {req.status} تغییر کرد - دلیل: {req.reason}" }, tx);

            await tx.CommitAsync();
            return Ok(new
            {
                status = true,
                message = req.status == "active" ? "تبلیغ با موفقیت تایید شد." : "تبلیغ رد شد.",
                data = new { id = req.id, newStatus = req.status }
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // GET /api/getPublicAdvertisements - نمایش تبلیغات تایید شده (بدون احراز هویت)
    [AllowAnonymous]
    [HttpGet("getPublicAdvertisements")]
    public async Task<IActionResult> GetPublicAdvertisements()
    {
        await using var conn = _db.CreateConnection();

        // بررسی بازه نمایش: از ۲۴ ساعت قبل انتخابات تا پایان آن
        var votingRow = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT start_date, end_date FROM election_schedule_events WHERE event_key='voting' LIMIT 1");

        if (votingRow != null)
        {
            DateTime? votingStart = _jalali.NormalizeToGregorian((object?)votingRow.start_date);
            DateTime? votingEnd   = _jalali.NormalizeToGregorian((object?)votingRow.end_date);
            if (votingStart.HasValue)
            {
                var viewStart = votingStart.Value.AddHours(-24);
                var now = DateTime.Now;
                if (now < viewStart || (votingEnd.HasValue && now > votingEnd.Value))
                    return StatusCode(403, new { status = false, message = "نمایش تبلیغات در این بازه زمانی مجاز نیست." });
            }
        }

        var rows = (await conn.QueryAsync<dynamic>(@"
            SELECT f.id AS codeentekhabati, ad.id, ad.nationalId, ad.title, ad.description, ad.image,
                   ad.type, ad.status, ad.views, ad.target_link, ad.managerialRecords, ad.academicRecords,
                   ad.honors, ad.plans, ad.slogan, ad.create_date,
                   u.first_name, u.last_name, u.education, u.user_type, u.yearsOfService,
                   re.name AS regionName
            FROM advertisements ad
            JOIN users u ON u.national_id=ad.nationalId
            LEFT JOIN final_submissions f ON f.nationalId=ad.nationalId
            LEFT JOIN region re ON re.id=u.region_id
            WHERE ad.status='active' AND ad.deleter IS NULL
            ORDER BY ad.create_date DESC")).AsList();

        var list = rows.Select(r =>
        {
            var d = (IDictionary<string, object>)r;
            if (d["create_date"] is DateTime cd) d["create_atsh"] = _jalali.Format(cd, "H:i Y-n-j");
            return d;
        });

        return Ok(new { status = true, data = list });
    }

    // POST /api/increaseViewAdd  {id}
    [HttpPost("increaseViewAdd")]
    public async Task<IActionResult> IncreaseViewAdd([FromBody] IncreaseViewRequest? req)
    {
        if (req == null || req.id == null)
            return BadRequest(new { status = false, message = "پارامتر کد الزامی است." });

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            await conn.ExecuteAsync(
                "UPDATE advertisements SET views=COALESCE(views,0)+1 WHERE id=@id",
                new { id = req.id }, tx);
            await tx.CommitAsync();
            return Ok(new { status = true, message = "با موفقیت انجام شد.", data = new { id = req.id } });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}

public record AdvSaveRequest(
    int? id, string? title, string? description, string? type, string? status,
    string? targetLink, string? imagePath, string? managerialRecords,
    string? academicRecords, string? honors, string? plans, string? slogan, int isPaid = 0);

public class DeleteAdvRequest
{
    public long? code { get; set; }
    public string? reson { get; set; }
    public string? status { get; set; }
}
public class IncreaseViewRequest { public long? id { get; set; } }
public record ApproveAdvRequest(long id, string? status, string? reason);

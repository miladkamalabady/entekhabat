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

    // POST /api/advertisementsSave  (multipart or JSON)
    [HttpPost("advertisementsSave")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> AdvertisementsSave(
        [FromForm] AdvSaveRequest? formReq,
        [FromBody] AdvSaveRequest? bodyReq,
        IFormFile? image)
    {
        var req = formReq ?? bodyReq;
        if (req == null)
            return BadRequest(new { status = false, message = "داده ارسال نشده." });

        await using var conn = _db.CreateConnection();

        // Check advertising schedule
        var adsStartRow = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT start_date FROM election_schedule_events WHERE event_key='ads_upload_start' LIMIT 1");
        var votingRow = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT start_date FROM election_schedule_events WHERE event_key='voting' LIMIT 1");

        if (adsStartRow != null && votingRow != null)
        {
            var adsStart = _jalali.ParseJalaliSchedule((string?)adsStartRow.start_date);
            var votingStart = _jalali.ParseJalaliSchedule((string?)votingRow.start_date);
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

        int id = req.id;

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
                    type = req.type, img = storedImagePath, link = req.targetLink ?? "",
                    mgr = req.managerialRecords ?? "", acad = req.academicRecords ?? "",
                    hon = req.honors ?? "", plans = req.plans ?? "", slogan = req.slogan ?? ""
                }, tx);

            long insertId = id > 0 ? id : (long)await conn.ExecuteScalarAsync("SELECT LAST_INSERT_ID()", transaction: tx);
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
                       re.name AS regionName, u.education, u.user_type, u.yearsOfService
                FROM advertisements ad
                JOIN users u ON u.national_id=ad.nationalId
                LEFT JOIN final_submissions f ON ad.nationalId=f.nationalId
                LEFT JOIN region re ON re.id=u.region_id
                WHERE ad.nationalId=@nid ORDER BY ad.create_date DESC"
            : @"SELECT f.id AS codeentekhabati, ad.*, u.first_name, u.last_name, u.id AS code,
                       re.name AS regionName, u.education, u.user_type, u.yearsOfService
                FROM advertisements ad
                JOIN users u ON u.national_id=ad.nationalId
                LEFT JOIN final_submissions f ON ad.nationalId=f.nationalId
                LEFT JOIN region re ON re.id=u.region_id
                WHERE region_id=@rid ORDER BY create_date DESC";

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
    public async Task<IActionResult> DeleteAdv([FromBody] DeleteAdvRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.code))
            return BadRequest(new { status = false, message = "پارامتر کد الزامی است." });

        await using var conn = _db.CreateConnection();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT roles FROM users WHERE national_id=@nid", new { nid = NationalId }, tx);

            if (me == null || !new[] { "EXECUTIVE", "SUPERVISOR", "CANDIDATE" }.Contains((string)me.roles))
                return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });

            string myRole = (string)me.roles;

            string selectSql = myRole == "CANDIDATE"
                ? "SELECT deleter FROM advertisements WHERE id=@id"
                : "SELECT deleter FROM advertisements WHERE nationalId=@nid AND id=@id";

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

    // POST /api/increaseViewAdd  {id}
    [HttpPost("increaseViewAdd")]
    public async Task<IActionResult> IncreaseViewAdd([FromBody] IncreaseViewRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.id))
            return BadRequest(new { status = false, message = "پارامتر کد الزامی است." });

        await using var conn = _db.CreateConnection();
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
    int id, string? title, string? description, string? type, string? status,
    string? targetLink, string? imagePath, string? managerialRecords,
    string? academicRecords, string? honors, string? plans, string? slogan, int isPaid = 0);

public record DeleteAdvRequest(string code, string? reson, string? status);
public record IncreaseViewRequest(string id);

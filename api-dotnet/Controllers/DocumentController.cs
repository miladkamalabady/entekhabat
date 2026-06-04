using System.Text.RegularExpressions;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public DocumentController(DatabaseService db, IWebHostEnvironment env, IConfiguration config)
    {
        _db = db;
        _env = env;
        _config = config;
    }

    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";
    private string UserRole => User.Claims.FirstOrDefault(c => c.Type == "roles")?.Value ?? "";

    private static readonly string[] AllowedImages = { "image/jpeg", "image/png", "image/jpg" };
    private static readonly string[] AllowedDocs = { "image/jpeg", "image/png", "image/jpg", "application/pdf" };
    private const long MaxSize = 1 * 1024 * 1024; // 1 MB

    // POST /api/UploadUserDocuments  (multipart form)
    // Required: user_photo, soPishine_cert, ravan_cert
    // Optional: education_doc
    [HttpPost("UploadUserDocuments")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadUserDocuments(
        IFormFile? user_photo,
        IFormFile? soPishine_cert,
        IFormFile? ravan_cert,
        IFormFile? education_doc)
    {
        if (string.IsNullOrWhiteSpace(NationalId))
            return BadRequest(new { status = false, message = "پارامتر nationalId الزامی است." });

        var required = new[]
        {
            ("user_photo", user_photo, AllowedImages),
            ("soPishine_cert", soPishine_cert, AllowedDocs),
            ("ravan_cert", ravan_cert, AllowedDocs)
        };

        // Validate required files
        foreach (var (field, file, allowed) in required)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { status = false, message = $"فایل {field} ارسال نشده." });
            if (file.Length > MaxSize)
                return BadRequest(new { status = false, message = $"فایل {field} نباید بیشتر از 1 مگابایت باشد." });
            if (!allowed.Contains(file.ContentType))
                return BadRequest(new { status = false, message = $"فرمت فایل {field} مجاز نیست." });
        }

        var baseDir = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads", "user_documents");
        var userKey = "nid_" + Regex.Replace(NationalId, "[^0-9]", "");
        var userDir = Path.Combine(baseDir, userKey);
        Directory.CreateDirectory(userDir);

        var paths = new Dictionary<string, string>();

        async Task<string> SaveFile(IFormFile f, string fieldName)
        {
            var ext = Path.GetExtension(f.FileName).ToLower().TrimStart('.');
            if (string.IsNullOrEmpty(ext)) ext = f.ContentType == "application/pdf" ? "pdf" : "jpg";
            var fileName = $"{fieldName}_{DateTime.Now:yyyyMMdd_HHmm}_{Random.Shared.Next(0xFFFF):X4}.{ext}";
            var fullPath = Path.Combine(userDir, fileName);
            await using var stream = System.IO.File.Create(fullPath);
            await f.CopyToAsync(stream);
            return $"uploads/user_documents/{userKey}/{fileName}";
        }

        paths["user_photo"] = await SaveFile(user_photo!, "user_photo");
        paths["soPishine_cert"] = await SaveFile(soPishine_cert!, "soPishine_cert");
        paths["ravan_cert"] = await SaveFile(ravan_cert!, "ravan_cert");

        if (education_doc != null && education_doc.Length > 0)
        {
            if (education_doc.Length <= MaxSize && AllowedDocs.Contains(education_doc.ContentType))
                paths["education_doc"] = await SaveFile(education_doc, "education_doc");
        }

        await using var conn = _db.CreateConnection();
        await using var tx = await conn.BeginTransactionAsync();

        if (paths.ContainsKey("education_doc"))
            await conn.ExecuteAsync(
                @"INSERT INTO user_documents (nationalId, user_photo, education_doc, employment_cert, soPishine_cert, ravan_cert)
                  VALUES (@nid,@up,@ed,'',@sp,@rc)
                  ON DUPLICATE KEY UPDATE user_photo=VALUES(user_photo), education_doc=VALUES(education_doc),
                  soPishine_cert=VALUES(soPishine_cert), ravan_cert=VALUES(ravan_cert), updated_at=NOW()",
                new { nid = NationalId, up = paths["user_photo"], ed = paths["education_doc"], sp = paths["soPishine_cert"], rc = paths["ravan_cert"] }, tx);
        else
            await conn.ExecuteAsync(
                @"INSERT INTO user_documents (nationalId, user_photo, employment_cert, soPishine_cert, ravan_cert)
                  VALUES (@nid,@up,'',@sp,@rc)
                  ON DUPLICATE KEY UPDATE user_photo=VALUES(user_photo),
                  soPishine_cert=VALUES(soPishine_cert), ravan_cert=VALUES(ravan_cert), updated_at=NOW()",
                new { nid = NationalId, up = paths["user_photo"], sp = paths["soPishine_cert"], rc = paths["ravan_cert"] }, tx);

        await tx.CommitAsync();

        var responseData = new Dictionary<string, object>
        {
            ["nationalId"] = NationalId,
            ["user_photo"] = paths["user_photo"],
            ["soPishine_cert"] = paths["soPishine_cert"],
            ["ravan_cert"] = paths["ravan_cert"]
        };
        if (paths.ContainsKey("education_doc"))
            responseData["education_doc"] = paths["education_doc"];

        return Ok(new { status = true, message = "فایل‌ها با موفقیت ذخیره شدند.", data = responseData });
    }

    // POST /api/UpdateDocumentReview  {national_Id, documentKey, reviewStatus}
    [HttpPost("UpdateDocumentReview")]
    public async Task<IActionResult> UpdateDocumentReview([FromBody] DocumentReviewRequest req)
    {
        if (UserRole != "SUPERVISOR" && UserRole != "EXECUTIVE")
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });

        var allowedKeys = new[] { "user_photo", "education_doc", "employment_cert", "soPishine_cert", "ravan_cert" };
        var allowedStatuses = new[] { "approved", "rejected", "pending" };

        if (string.IsNullOrWhiteSpace(req.national_Id) || string.IsNullOrWhiteSpace(req.documentKey) || string.IsNullOrWhiteSpace(req.reviewStatus))
            return BadRequest(new { status = false, message = "پارامترهای کد ملی، نوع مدرک و وضعیت بررسی الزامی است." });
        if (!allowedKeys.Contains(req.documentKey))
            return BadRequest(new { status = false, message = "نوع مدرک نامعتبر است." });
        if (!allowedStatuses.Contains(req.reviewStatus))
            return BadRequest(new { status = false, message = "وضعیت بررسی نامعتبر است." });

        await using var conn = _db.CreateConnection();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            await conn.ExecuteAsync(
                @"UPDATE user_documents
                  SET document_reviews = JSON_SET(
                    COALESCE(document_reviews, JSON_OBJECT()),
                    CONCAT('$.', @key),
                    JSON_OBJECT('status',@status,'reviewed_by',@by,'reviewed_at',DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s'))
                  ) WHERE nationalId=@nid",
                new { key = req.documentKey, status = req.reviewStatus, by = NationalId, nid = req.national_Id }, tx);

            await conn.ExecuteAsync(
                "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'بررسی مدرک',@desc)",
                new { nid = NationalId, desc = $"بررسی {req.documentKey} برای {req.national_Id} با وضعیت {req.reviewStatus}" }, tx);

            await tx.CommitAsync();
            return Ok(new
            {
                status = true,
                message = "نظر بررسی مدرک ثبت شد.",
                data = new { nationalId = req.national_Id, documentKey = req.documentKey, reviewStatus = req.reviewStatus, reviewedBy = NationalId }
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}

public record DocumentReviewRequest(string national_Id, string documentKey, string reviewStatus);

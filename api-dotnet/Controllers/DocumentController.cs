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
    private static readonly string[] AllowedDocs   = { "image/jpeg", "image/png", "image/jpg", "application/pdf" };
    private const long MaxSize = 1 * 1024 * 1024; // 1 MB

    // اعتبارسنجی magic bytes — جلوگیری از جعل Content-Type
    private static bool IsValidMagicBytes(IFormFile file)
    {
        using var reader = new BinaryReader(file.OpenReadStream());
        var header = reader.ReadBytes(8);
        // JPEG: FF D8 FF
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;
        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (header.Length >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return true;
        // PDF: 25 50 44 46 (%PDF)
        if (header.Length >= 4 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46) return true;
        return false;
    }

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

        var fieldNames = new Dictionary<string, string>
        {
            ["user_photo"]     = "عکس پرسنلی",
            ["soPishine_cert"] = "گواهی عدم سوپیشینه",
            ["ravan_cert"]     = "گواهی سلامت جسمی و روانی",
            ["education_doc"]  = "مدرک تحصیلی"
        };

        var required = new[]
        {
            ("user_photo", user_photo, AllowedImages),
            ("soPishine_cert", soPishine_cert, AllowedDocs),
            ("ravan_cert", ravan_cert, AllowedDocs)
        };

        foreach (var (field, file, allowed) in required)
        {
            var label = fieldNames.GetValueOrDefault(field, field);
            if (file == null || file.Length == 0)
                return BadRequest(new { status = false, message = $"فایل {label} ارسال نشده." });
            if (file.Length > MaxSize)
                return BadRequest(new { status = false, message = $"فایل {label} نباید بیشتر از ۱ مگابایت باشد." });
            if (!allowed.Contains(file.ContentType))
                return BadRequest(new { status = false, message = $"فرمت فایل {label} مجاز نیست. فقط JPG، PNG و PDF قابل قبول است." });
            if (!IsValidMagicBytes(file))
                return BadRequest(new { status = false, message = $"محتوای فایل {label} معتبر نیست." });
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
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        if (paths.ContainsKey("education_doc"))
            await conn.ExecuteAsync(
                @"IF EXISTS (SELECT 1 FROM dbo.user_documents WHERE nationalId=@nid)
                  UPDATE dbo.user_documents SET user_photo=@up, education_doc=@ed, soPishine_cert=@sp, ravan_cert=@rc, updated_at=GETDATE() WHERE nationalId=@nid
                  ELSE INSERT INTO dbo.user_documents (nationalId,user_photo,education_doc,employment_cert,soPishine_cert,ravan_cert) VALUES (@nid,@up,@ed,'',@sp,@rc)",
                new { nid = NationalId, up = paths["user_photo"], ed = paths["education_doc"], sp = paths["soPishine_cert"], rc = paths["ravan_cert"] }, tx);
        else
            await conn.ExecuteAsync(
                @"IF EXISTS (SELECT 1 FROM dbo.user_documents WHERE nationalId=@nid)
                  UPDATE dbo.user_documents SET user_photo=@up, soPishine_cert=@sp, ravan_cert=@rc, updated_at=GETDATE() WHERE nationalId=@nid
                  ELSE INSERT INTO dbo.user_documents (nationalId,user_photo,employment_cert,soPishine_cert,ravan_cert) VALUES (@nid,@up,'',@sp,@rc)",
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
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            await conn.ExecuteAsync(
                @"UPDATE dbo.user_documents
                  SET document_reviews = JSON_MODIFY(COALESCE(NULLIF(document_reviews,''),'{}'), CONCAT('$.', @key),
                      JSON_QUERY((SELECT @status AS [status], @by AS reviewed_by, CONVERT(varchar(19),GETDATE(),120) AS reviewed_at FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))
                  WHERE nationalId=@nid",
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

    // GET /api/getCandidateDocuments
    [HttpGet("getCandidateDocuments")]
    public async Task<IActionResult> GetCandidateDocuments()
    {
        if (string.IsNullOrWhiteSpace(NationalId))
            return Unauthorized(new { status = false, message = "کاربر احراز هویت نشده است." });

        await using var conn = _db.CreateConnection();

        var row = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT TOP (1)
                    nationalId,
                    user_photo,
                    education_doc,
                    employment_cert,
                    soPishine_cert,
                    ravan_cert,
                    CASE WHEN COL_LENGTH('dbo.user_documents','transparency_form') IS NOT NULL
                         THEN transparency_form ELSE NULL END AS transparency_form,
                    document_reviews,
                    updated_at
              FROM dbo.user_documents
              WHERE nationalId=@nid",
            new { nid = NationalId });

        if (row == null)
            return Ok(new
            {
                status = true,
                data = new
                {
                    photo_url = (string?)null,
                    degree_url = (string?)null,
                    soPishine_url = (string?)null,
                    ravan_url = (string?)null,
                    transparencyForm_url = (string?)null
                }
            });

        return Ok(new
        {
            status = true,
            data = new
            {
                nationalId = (string?)row.nationalId,
                photo_url = (string?)row.user_photo,
                degree_url = (string?)row.education_doc,
                soPishine_url = (string?)row.soPishine_cert,
                ravan_url = (string?)row.ravan_cert,
                transparencyForm_url = (string?)row.transparency_form,
                document_reviews = (string?)row.document_reviews,
                updated_at = row.updated_at
            }
        });
    }

    // POST /api/UpdateUserDocuments
    // در حالت اصلاح، فقط فایل‌های ارسال‌شده جایگزین می‌شوند و فایل‌های قبلی دست‌نخورده می‌مانند.
    [HttpPost("UpdateUserDocuments")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UpdateUserDocuments(
        IFormFile? user_photo,
        IFormFile? education_doc,
        IFormFile? soPishine_cert,
        IFormFile? ravan_cert,
        IFormFile? transparency_form)
    {
        if (string.IsNullOrWhiteSpace(NationalId))
            return Unauthorized(new { status = false, message = "کاربر احراز هویت نشده است." });

        var supplied = new Dictionary<string, (IFormFile? file, string[] allowed, string label)>
        {
            ["user_photo"] = (user_photo, AllowedImages, "عکس پرسنلی"),
            ["education_doc"] = (education_doc, AllowedDocs, "مدرک تحصیلی"),
            ["soPishine_cert"] = (soPishine_cert, AllowedDocs, "گواهی عدم سوءپیشینه"),
            ["ravan_cert"] = (ravan_cert, AllowedDocs, "گواهی سلامت جسمی و روانی"),
            ["transparency_form"] = (transparency_form, AllowedDocs, "فرم تعهد شفافیت")
        };

        var changed = supplied.Where(x => x.Value.file != null && x.Value.file.Length > 0).ToList();
        if (changed.Count == 0)
            return BadRequest(new { status = false, message = "هیچ فایل جدیدی برای ویرایش ارسال نشده است." });

        foreach (var item in changed)
        {
            var file = item.Value.file!;
            if (file.Length > MaxSize)
                return BadRequest(new { status = false, message = $"فایل {item.Value.label} نباید بیشتر از ۱ مگابایت باشد." });
            if (!item.Value.allowed.Contains(file.ContentType))
                return BadRequest(new { status = false, message = $"فرمت فایل {item.Value.label} مجاز نیست." });
            if (!IsValidMagicBytes(file))
                return BadRequest(new { status = false, message = $"محتوای فایل {item.Value.label} معتبر نیست." });
        }

        var baseDir = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads", "user_documents");
        var userKey = "nid_" + Regex.Replace(NationalId, "[^0-9]", "");
        var userDir = Path.Combine(baseDir, userKey);
        Directory.CreateDirectory(userDir);

        async Task<string> SaveFile(IFormFile f, string fieldName)
        {
            var ext = Path.GetExtension(f.FileName).ToLower().TrimStart('.');
            if (string.IsNullOrEmpty(ext)) ext = f.ContentType == "application/pdf" ? "pdf" : "jpg";
            var fileName = $"{fieldName}_{DateTime.Now:yyyyMMdd_HHmmss}_{Random.Shared.Next(0xFFFF):X4}.{ext}";
            var fullPath = Path.Combine(userDir, fileName);
            await using var stream = System.IO.File.Create(fullPath);
            await f.CopyToAsync(stream);
            return $"uploads/user_documents/{userKey}/{fileName}";
        }

        var saved = new Dictionary<string, string>();
        foreach (var item in changed)
            saved[item.Key] = await SaveFile(item.Value.file!, item.Key);

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            var exists = await conn.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(1) FROM dbo.user_documents WHERE nationalId=@nid",
                new { nid = NationalId }, tx);

            if (exists == 0)
                return NotFound(new { status = false, message = "مدارک قبلی کاربر یافت نشد." });

            var setParts = new List<string>();
            var parameters = new DynamicParameters();
            parameters.Add("nid", NationalId);

            foreach (var kv in saved)
            {
                if (kv.Key == "transparency_form")
                {
                    var hasColumn = await conn.QueryFirstOrDefaultAsync<int>(
                        @"SELECT COUNT(1) FROM sys.columns
                          WHERE object_id = OBJECT_ID(N'dbo.user_documents')
                            AND name = N'transparency_form'", transaction: tx);

                    if (hasColumn == 0)
                        continue;
                }

                setParts.Add($"[{kv.Key}] = @{kv.Key}");
                parameters.Add(kv.Key, kv.Value);
            }

            if (setParts.Count == 0)
                return BadRequest(new { status = false, message = "هیچ مدرک قابل‌ذخیره‌ای ارسال نشده است." });

            setParts.Add("updated_at = GETDATE()");

            // فقط همان مدارکی که تغییر کرده‌اند دوباره در وضعیت pending قرار می‌گیرند.
            foreach (var key in saved.Keys)
            {
                if (key == "transparency_form")
                    continue;

                setParts.Add($@"document_reviews = JSON_MODIFY(
                    COALESCE(NULLIF(document_reviews,''),'{{}}'),
                    '$.{key}',
                    JSON_QUERY('{{"status":"pending"}}')
                )");
            }

            var updateSql = $@"UPDATE dbo.user_documents
                               SET {string.Join(", ", setParts)}
                               WHERE nationalId=@nid";

            await conn.ExecuteAsync(updateSql, parameters, tx);

            // پرونده پس از اصلاح مدارک دوباره برای بررسی ارسال می‌شود.
            await conn.ExecuteAsync(
                @"UPDATE dbo.final_submissions
                  SET requestStatus='SUBMITTED',
                      reson=NULL
                  WHERE nationalId=@nid",
                new { nid = NationalId }, tx);

            await conn.ExecuteAsync(
                "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'ویرایش مدارک',@desc)",
                new { nid = NationalId, desc = $"ویرایش مدارک: {string.Join(", ", saved.Keys)}" }, tx);

            await tx.CommitAsync();

            return Ok(new
            {
                status = true,
                message = "مدارک با موفقیت به‌روزرسانی و برای بررسی مجدد ارسال شد.",
                data = saved
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

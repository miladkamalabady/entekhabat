using System.Text.Json;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ObjectionController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly IWebHostEnvironment _env;

    public ObjectionController(DatabaseService db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";
    private string UserRole => User.Claims.FirstOrDefault(c => c.Type == "roles")?.Value ?? "";

    private static readonly string[] ReviewerRoles = { "SUPERVISOR", "EXECUTIVE", "ADMIN" };

    private async Task EnsureObjectionTables(MySqlConnector.MySqlConnection conn)
    {
        await conn.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS objections (
            id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
            tracking_code VARCHAR(30) NOT NULL,
            national_id VARCHAR(20) NOT NULL,
            decision_type VARCHAR(80) NULL,
            case_number VARCHAR(80) NULL,
            candidate_name VARCHAR(255) NULL,
            candidate_region VARCHAR(255) NULL,
            candidate_position VARCHAR(255) NULL,
            subject VARCHAR(255) NOT NULL,
            description TEXT NOT NULL,
            reasons TEXT NULL,
            urgency VARCHAR(20) DEFAULT 'normal',
            status VARCHAR(30) DEFAULT 'pending',
            declaration TINYINT(1) DEFAULT 1,
            response_text TEXT NULL,
            response_by VARCHAR(20) NULL,
            response_at DATETIME NULL,
            cancelled_at DATETIME NULL,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            UNIQUE KEY uq_tracking_code (tracking_code)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

        await conn.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS objection_documents (
            id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
            objection_id BIGINT UNSIGNED NOT NULL,
            file_name VARCHAR(255) NOT NULL,
            file_path VARCHAR(500) NOT NULL,
            file_size INT UNSIGNED DEFAULT 0,
            file_type VARCHAR(100) NULL,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            INDEX idx_objection_id (objection_id),
            FOREIGN KEY (objection_id) REFERENCES objections(id) ON DELETE CASCADE) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");
    }

    // GET /api/getObjections?trackingCode=&status=
    [HttpGet("getObjections")]
    public async Task<IActionResult> GetObjections([FromQuery] string? trackingCode, [FromQuery] string? status)
    {
        await using var conn = _db.CreateConnection();
        await EnsureObjectionTables(conn);

        bool isReviewer = ReviewerRoles.Contains(UserRole);
        var where = new List<string>();

        if (!isReviewer) where.Add("o.national_id=@nid");
        if (!string.IsNullOrWhiteSpace(trackingCode)) where.Add("o.tracking_code=@tc");
        if (!string.IsNullOrWhiteSpace(status)) where.Add("o.status=@st");

        string whereSql = where.Any() ? "WHERE " + string.Join(" AND ", where) : "";

        var rows = (await conn.QueryAsync<dynamic>(
            $@"SELECT o.*, COUNT(d.id) AS documents_count
               FROM objections o
               LEFT JOIN objection_documents d ON o.id=d.objection_id
               {whereSql}
               GROUP BY o.id ORDER BY o.created_at DESC",
            new { nid = NationalId, tc = trackingCode, st = status })).AsList();

        var result = new List<object>();
        foreach (var r in rows)
        {
            var d = (IDictionary<string, object>)r;
            long objId = Convert.ToInt64(d["id"]);

            var docs = (await conn.QueryAsync<dynamic>(
                "SELECT id, file_name, file_path, file_size, file_type, created_at FROM objection_documents WHERE objection_id=@id ORDER BY created_at ASC",
                new { id = objId })).AsList();

            string desc = (string)(d["description"] ?? "");
            string preview = desc.Length > 150 ? desc[..150] + "..." : desc;

            object? reasons = null;
            if (d["reasons"] is string rStr && !string.IsNullOrWhiteSpace(rStr))
                try { reasons = JsonSerializer.Deserialize<object>(rStr); } catch { }

            result.Add(new
            {
                id = objId,
                trackingCode = d["tracking_code"],
                nationalId = d["national_id"],
                decisionType = d["decision_type"],
                caseNumber = d["case_number"],
                candidateName = d["candidate_name"],
                candidateRegion = d["candidate_region"],
                candidatePosition = d["candidate_position"],
                subject = d["subject"],
                description = desc,
                preview,
                reasons = reasons ?? new object[0],
                urgency = d["urgency"] ?? "normal",
                status = d["status"],
                declaration = Convert.ToBoolean(d.GetValueOrDefault("declaration") ?? false),
                submittedDate = d["created_at"],
                lastUpdate = d["updated_at"],
                documentsCount = (int)Convert.ToInt32(d["documents_count"]),
                documents = docs.Select(doc => new
                {
                    id = (int)Convert.ToInt32(((IDictionary<string, object>)doc)["id"]),
                    name = ((IDictionary<string, object>)doc)["file_name"],
                    path = ((IDictionary<string, object>)doc)["file_path"],
                    size = (int)Convert.ToInt32(((IDictionary<string, object>)doc)["file_size"]),
                    type = ((IDictionary<string, object>)doc)["file_type"],
                    uploadedAt = ((IDictionary<string, object>)doc)["created_at"]
                }),
                responseText = d["response_text"],
                responseBy = d["response_by"],
                responseAt = d["response_at"]
            });
        }

        return Ok(new { status = true, data = result });
    }

    // POST /api/saveObjection  (multipart/form-data)
    [HttpPost("saveObjection")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> SaveObjection(
        [FromForm] ObjectionFormRequest? formReq,
        [FromForm] List<IFormFile>? documents)
    {
        if (formReq == null)
            return BadRequest(new { status = false, message = "داده ارسال نشده." });

        string? decisionType = formReq.decisionType, description = formReq.description;
        string subject = string.IsNullOrWhiteSpace(formReq.subject) ? "-" : formReq.subject;
        string caseNumber = formReq.caseNumber ?? "", candidateName = formReq.candidateName ?? "";
        string candidateRegion = formReq.candidateRegion ?? "", candidatePosition = formReq.candidatePosition ?? "";
        string urgency = formReq.urgency ?? "normal";
        bool declaration = formReq.declaration is "1" or "true" or "True";
        string[] reasons = Array.Empty<string>();
        if (!string.IsNullOrWhiteSpace(formReq.reasons))
            try { reasons = JsonSerializer.Deserialize<string[]>(formReq.reasons) ?? Array.Empty<string>(); } catch { }

        if (string.IsNullOrWhiteSpace(decisionType) || string.IsNullOrWhiteSpace(description) || !declaration)
            return BadRequest(new { status = false, message = "پارامترهای الزامی ناقص است." });

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        await EnsureObjectionTables(conn);

        return await DbHelper.WithTransaction(conn, async tx =>
        {
            // Generate unique tracking code
            string tc;
            do
            {
                tc = DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture) + Random.Shared.Next(1, 9999).ToString("D4");
                var exists = await conn.QueryFirstOrDefaultAsync<long?>("SELECT id FROM objections WHERE tracking_code=@tc", new { tc }, tx);
                if (!exists.HasValue) break;
            } while (true);

            long objId = await conn.ExecuteScalarAsync<long>(
                @"INSERT INTO objections (tracking_code, national_id, decision_type, case_number, candidate_name,
                    candidate_region, candidate_position, subject, description, reasons, urgency, status, declaration)
                  VALUES (@tc,@nid,@dt,@cn,@cname,@cr,@cp,@subj,@desc,@reas,@urg,'pending',@decl);
                  SELECT LAST_INSERT_ID();",
                new { tc, nid = NationalId, dt = decisionType, cn = caseNumber, cname = candidateName,
                      cr = candidateRegion, cp = candidatePosition, subj = subject, desc = description,
                      reas = JsonSerializer.Serialize(reasons), urg = urgency, decl = declaration ? 1 : 0 }, tx);

            // Handle uploaded files
            var uploadedFiles = new List<object>();
            if (documents != null && documents.Any())
            {
                var uploadDir = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads", "objections");
                Directory.CreateDirectory(uploadDir);

                foreach (var file in documents.Where(f => f.Length > 0))
                {
                    var uniqueName = $"{DateTimeOffset.Now.ToUnixTimeSeconds()}_{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(uploadDir, uniqueName);
                    await using var fs = System.IO.File.Create(filePath);
                    await file.CopyToAsync(fs);

                    var relPath = $"uploads/objections/{uniqueName}";
                    await conn.ExecuteAsync(
                        "INSERT INTO objection_documents (objection_id, file_name, file_path, file_size, file_type) VALUES (@oid,@fn,@fp,@fs,@ft)",
                        new { oid = objId, fn = file.FileName, fp = relPath, fs = (int)file.Length, ft = file.ContentType }, tx);

                    uploadedFiles.Add(new { name = file.FileName, path = relPath, size = (int)file.Length });
                }
            }

            await conn.ExecuteAsync(
                "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'ثبت اعتراض',@desc)",
                new { nid = NationalId, desc = $"ثبت اعتراض با کد {tc} با {uploadedFiles.Count} فایل ضمیمه" }, tx);

            return Ok(new
            {
                status = true,
                message = "اعتراض با موفقیت ثبت شد.",
                data = new { trackingCode = tc, objectionId = objId, filesCount = uploadedFiles.Count, uploadedFiles }
            });
        });
    }

    // POST /api/updateObjectionStatus  {id, status, responseText}
    [HttpPost("updateObjectionStatus")]
    public async Task<IActionResult> UpdateObjectionStatus([FromBody] UpdateObjectionRequest req)
    {
        var allowed = new[] { "pending", "under_review", "approved", "rejected", "cancelled" };
        if (req.id <= 0 || !allowed.Contains(req.status))
            return BadRequest(new { status = false, message = "پارامترها نامعتبر است." });

        bool isReviewer = ReviewerRoles.Contains(UserRole);
        if ((req.status == "approved" || req.status == "rejected" || req.status == "under_review") && !isReviewer)
            return StatusCode(403, new { status = false, message = "دسترسی لازم برای تایید/رد اعتراض را ندارید." });

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            string ownerClause = isReviewer ? "" : "AND national_id=@nid";
            var affected = await conn.ExecuteAsync(
                $@"UPDATE objections SET
                    status=@st,
                    response_text=CASE WHEN @rt <> '' THEN @rt ELSE response_text END,
                    response_by=CASE WHEN @st IN ('approved','rejected','under_review') THEN @by ELSE response_by END,
                    response_at=CASE WHEN @st IN ('approved','rejected','under_review') THEN NOW() ELSE response_at END,
                    cancelled_at=CASE WHEN @st='cancelled' THEN NOW() ELSE cancelled_at END,
                    updated_at=NOW()
                  WHERE id=@id {ownerClause}",
                new { st = req.status, rt = req.responseText ?? "", by = NationalId, id = req.id, nid = NationalId }, tx);

            if (affected <= 0)
            {
                await tx.RollbackAsync();
                return NotFound(new { status = false, message = "اعتراضی برای بروزرسانی یافت نشد." });
            }

            await conn.ExecuteAsync(
                "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'بروزرسانی اعتراض',@desc)",
                new { nid = NationalId, desc = $"اعتراض {req.id} به وضعیت {req.status} تغییر یافت" }, tx);

            await tx.CommitAsync();
            return Ok(new { status = true, message = "وضعیت اعتراض با موفقیت بروزرسانی شد.", data = new { id = req.id, status = req.status } });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // GET /api/downloadObjectionFile?id=
    [HttpGet("downloadObjectionFile")]
    public async Task<IActionResult> DownloadObjectionFile([FromQuery] int id)
    {
        if (id <= 0) return BadRequest("Invalid file ID");

        await using var conn = _db.CreateConnection();
        var file = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT d.*, o.national_id FROM objection_documents d JOIN objections o ON d.objection_id=o.id WHERE d.id=@id",
            new { id });

        if (file == null) return NotFound("File not found");

        bool isReviewer = ReviewerRoles.Contains(UserRole);
        if (!isReviewer && (string)file.national_id != NationalId)
            return StatusCode(403, "Access denied");

        var filePath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, (string)file.file_path);
        if (!System.IO.File.Exists(filePath)) return NotFound("File not found on server");

        var contentType = (string?)file.file_type ?? "application/octet-stream";
        return PhysicalFile(filePath, contentType, (string)file.file_name);
    }
}

public record ObjectionFormRequest(
    string? decisionType, string? caseNumber, string? candidateName,
    string? candidateRegion, string? candidatePosition,
    string? subject, string? description, string? reasons, string? urgency, string? declaration);

public record ObjectionJsonRequest(
    string? decisionType, string? caseNumber, string? candidateName,
    string? candidateRegion, string? candidatePosition,
    string? subject, string? description, string[]? reasons, string? urgency, bool declaration);

public record UpdateObjectionRequest(long id, string status, string? responseText);

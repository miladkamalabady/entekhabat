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

    private async Task EnsureObjectionTables(Microsoft.Data.SqlClient.SqlConnection conn)
    {
        await conn.ExecuteAsync(@"IF OBJECT_ID(N'dbo.objections', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.objections (id BIGINT IDENTITY(1,1) PRIMARY KEY, tracking_code NVARCHAR(30) NOT NULL UNIQUE, national_id NVARCHAR(20) NOT NULL, decision_type NVARCHAR(80) NULL, case_number NVARCHAR(80) NULL, candidate_name NVARCHAR(255) NULL, candidate_region NVARCHAR(255) NULL, candidate_position NVARCHAR(255) NULL, subject NVARCHAR(255) NOT NULL, description NVARCHAR(MAX) NOT NULL, reasons NVARCHAR(MAX) NULL, urgency NVARCHAR(20) DEFAULT N'normal', status NVARCHAR(30) DEFAULT N'pending', declaration BIT DEFAULT 1, response_text NVARCHAR(MAX) NULL, response_by NVARCHAR(20) NULL, response_at DATETIME2 NULL, cancelled_at DATETIME2 NULL, created_at DATETIME2 DEFAULT GETDATE(), updated_at DATETIME2 DEFAULT GETDATE());
END;
IF OBJECT_ID(N'dbo.objection_documents', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.objection_documents (id BIGINT IDENTITY(1,1) PRIMARY KEY, objection_id BIGINT NOT NULL, file_name NVARCHAR(255) NOT NULL, file_path NVARCHAR(500) NOT NULL, file_size INT DEFAULT 0, file_type NVARCHAR(100) NULL, created_at DATETIME2 DEFAULT GETDATE(), CONSTRAINT FK_objection_documents_objections FOREIGN KEY(objection_id) REFERENCES dbo.objections(id) ON DELETE CASCADE);
 CREATE INDEX idx_objection_id ON dbo.objection_documents(objection_id);
END");
    }

    // GET /api/getObjections?trackingCode=&status=
    [HttpGet("getObjections")]
    public async Task<IActionResult> GetObjections([FromQuery] string? trackingCode, [FromQuery] string? status)
    {
        await using var conn = _db.CreateConnection();
        await EnsureObjectionTables(conn);

        var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT TOP (1) u.roles, u.region_id, r.ProvinceCode
              FROM dbo.users u LEFT JOIN dbo.region r ON r.id=u.region_id
              WHERE u.national_id=@nid", new { nid = NationalId });

        string role = me == null ? "" : (Convert.ToString(me.roles) ?? "");
        int regionId = me == null ? 0 : Convert.ToInt32(me.region_id ?? 0);
        int provinceCode = me?.ProvinceCode == null ? 0 : Convert.ToInt32(me.ProvinceCode);
        bool isAdmin = role == "ADMIN";
        bool isSupervisor = role == "SUPERVISOR";
        bool isProvinceSupervisor = isSupervisor && regionId != 1000 && regionId % 100 == 0;

        var where = new List<string>();
        if (!isAdmin && !isSupervisor)
            where.Add("o.national_id=@nid");
        else if (isProvinceSupervisor)
            // اعتراضات داوطلبان تمام مناطق همان استان فقط در کارتابل نظارت استان دیده می‌شود.
            where.Add("r.ProvinceCode=@provinceCode");
        else if (isSupervisor)
            // ناظر منطقه فقط اعتراضات حوزه خودش را می‌بیند؛ تصمیم نهایی اعتراض با استان است.
            where.Add("u.region_id=@regionId");

        if (!string.IsNullOrWhiteSpace(trackingCode)) where.Add("o.tracking_code=@tc");
        if (!string.IsNullOrWhiteSpace(status)) where.Add("o.status=@st");

        string whereSql = where.Any() ? "WHERE " + string.Join(" AND ", where) : "";

        var rows = (await conn.QueryAsync<dynamic>(
            $@"SELECT o.*, COUNT(d.id) AS documents_count
               FROM dbo.objections o
               LEFT JOIN dbo.objection_documents d ON o.id=d.objection_id
               LEFT JOIN dbo.users u ON u.national_id=o.national_id
               LEFT JOIN dbo.region r ON r.id=u.region_id
               {whereSql}
               GROUP BY o.id, o.tracking_code, o.national_id, o.decision_type, o.case_number,
                        o.candidate_name, o.candidate_region, o.candidate_position, o.subject,
                        o.description, o.reasons, o.urgency, o.status, o.declaration,
                        o.response_text, o.response_by, o.response_at, o.cancelled_at,
                        o.created_at, o.updated_at
               ORDER BY o.created_at DESC",
            new { nid = NationalId, tc = trackingCode, st = status, regionId, provinceCode })).AsList();

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
                id = objId, trackingCode = d["tracking_code"], nationalId = d["national_id"],
                decisionType = d["decision_type"], caseNumber = d["case_number"],
                candidateName = d["candidate_name"], candidateRegion = d["candidate_region"],
                candidatePosition = d["candidate_position"], subject = d["subject"],
                description = desc, preview, reasons = reasons ?? new object[0],
                urgency = d["urgency"] ?? "normal", status = d["status"],
                declaration = Convert.ToBoolean(d.GetValueOrDefault("declaration") ?? false),
                submittedDate = d["created_at"], lastUpdate = d["updated_at"],
                documentsCount = Convert.ToInt32(d["documents_count"]),
                documents = docs.Select(doc => new {
                    id = Convert.ToInt32(((IDictionary<string, object>)doc)["id"]),
                    name = ((IDictionary<string, object>)doc)["file_name"],
                    path = ((IDictionary<string, object>)doc)["file_path"],
                    size = Convert.ToInt32(((IDictionary<string, object>)doc)["file_size"]),
                    type = ((IDictionary<string, object>)doc)["file_type"],
                    uploadedAt = ((IDictionary<string, object>)doc)["created_at"]
                }),
                responseText = d["response_text"], responseBy = d["response_by"], responseAt = d["response_at"]
            });
        }

        return Ok(new { status = true, data = result, scope = isProvinceSupervisor ? "province" : isSupervisor ? "region" : isAdmin ? "all" : "self" });
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
                  OUTPUT INSERTED.id
                  VALUES (@tc,@nid,@dt,@cn,@cname,@cr,@cp,@subj,@desc,@reas,@urg,'pending',@decl);",
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

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT TOP (1) u.roles, u.region_id, r.ProvinceCode
              FROM dbo.users u LEFT JOIN dbo.region r ON r.id=u.region_id
              WHERE u.national_id=@nid", new { nid = NationalId });
        string role = me == null ? "" : (Convert.ToString(me.roles) ?? "");
        int reviewerRegionId = me == null ? 0 : Convert.ToInt32(me.region_id ?? 0);
        int reviewerProvinceCode = me?.ProvinceCode == null ? 0 : Convert.ToInt32(me.ProvinceCode);
        bool isAdmin = role == "ADMIN";
        bool isProvinceSupervisor = role == "SUPERVISOR" && reviewerRegionId != 1000 && reviewerRegionId % 100 == 0;
        bool isReviewer = isAdmin || isProvinceSupervisor;

        // تایید/رد اعتراض فقط توسط نظارت استان (یا ADMIN) مجاز است.
        if ((req.status == "approved" || req.status == "rejected" || req.status == "under_review") && !isReviewer)
            return StatusCode(403, new { status = false, message = "رسیدگی نهایی اعتراض فقط برای نظارت استان مجاز است." });

        if (isProvinceSupervisor)
        {
            var objectionProvince = await conn.QueryFirstOrDefaultAsync<int?>(
                @"SELECT r.ProvinceCode
                  FROM dbo.objections o
                  JOIN dbo.users u ON u.national_id=o.national_id
                  JOIN dbo.region r ON r.id=u.region_id
                  WHERE o.id=@id", new { id = req.id });
            if (!objectionProvince.HasValue || objectionProvince.Value != reviewerProvinceCode)
                return StatusCode(403, new { status = false, message = "این اعتراض مربوط به استان شما نیست." });
        }
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            string ownerClause = isReviewer ? "" : "AND national_id=@nid";
            var affected = await conn.ExecuteAsync(
                $@"UPDATE objections SET
                    status=@st,
                    response_text=CASE WHEN @rt <> '' THEN @rt ELSE response_text END,
                    response_by=CASE WHEN @st IN ('approved','rejected','under_review') THEN @by ELSE response_by END,
                    response_at=CASE WHEN @st IN ('approved','rejected','under_review') THEN GETDATE() ELSE response_at END,
                    cancelled_at=CASE WHEN @st='cancelled' THEN GETDATE() ELSE cancelled_at END,
                    updated_at=GETDATE()
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

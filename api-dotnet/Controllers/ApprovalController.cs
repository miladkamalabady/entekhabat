using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ApprovalController : ControllerBase
{
    private readonly DatabaseService _db;

    public ApprovalController(DatabaseService db) => _db = db;

    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";

    private static string GeneratePass(int regionId)
    {
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var rng = new char[6];
        for (int i = 0; i < 6; i++)
            rng[i] = chars[Random.Shared.Next(chars.Length)];
        return regionId + new string(rng);
    }

    // GET /api/getFinalResultsApprovalStatus
    [HttpGet("getFinalResultsApprovalStatus")]
    public async Task<IActionResult> GetFinalResultsApprovalStatus()
    {
        await using var conn = _db.CreateConnection();

        var user = await conn.QueryRowDict(
            "SELECT roles, region_id FROM users WHERE national_id=@nid", new { nid = NationalId });
        if (user == null) return Unauthorized();

        int regionId = user.Int("region_id");

        var row = await conn.QueryRowDict(
            "SELECT executive_approved, supervisor_approved, is_active FROM final_results_approvals WHERE region_id=@rid",
            new { rid = regionId });

        if (row == null)
        {
            await conn.ExecuteAsync(
                @"IF NOT EXISTS (SELECT 1 FROM dbo.final_results_approvals WHERE region_id=@rid) INSERT INTO dbo.final_results_approvals (region_id, EXECUTIVEPass, SUPERVISORPass) VALUES (@rid,@ep,@sp)",
                new { rid = regionId, ep = GeneratePass(regionId), sp = GeneratePass(regionId) });
        }

        return Ok(new
        {
            status = true,
            data = new
            {
                executiveApproved  = Convert.ToBoolean(row?.GetValueOrDefault("executive_approved")  ?? false),
                supervisorApproved = Convert.ToBoolean(row?.GetValueOrDefault("supervisor_approved") ?? false),
                isActive           = Convert.ToBoolean(row?.GetValueOrDefault("is_active")           ?? false)
            }
        });
    }

    // GET /api/getRegionalApprovalStatuses
    [HttpGet("getRegionalApprovalStatuses")]
    public async Task<IActionResult> GetRegionalApprovalStatuses([FromQuery] int? province = null)
    {
        await using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(@"
            SELECT r.id AS regionId, r.name AS regionName, r.ProvinceCode AS provinceCode,
                   ISNULL(fra.executive_approved,0) AS executiveApproved,
                   ISNULL(fra.supervisor_approved,0) AS supervisorApproved,
                   ISNULL(fra.is_active,0) AS isActive
            FROM dbo.region r
            LEFT JOIN dbo.final_results_approvals fra ON fra.region_id=r.id
            WHERE r.id % 100 <> 0 AND (@province IS NULL OR r.ProvinceCode=@province)
            ORDER BY r.ProvinceCode,r.id", new { province });
        return Ok(new { status=true, data=rows });
    }

    // POST /api/submitFinalResultsApproval  {role, passcode1, passcode2}
    [HttpPost("submitFinalResultsApproval")]
    public async Task<IActionResult> SubmitFinalResultsApproval([FromBody] SubmitApprovalRequest req)
    {
        if (!new[] { "EXECUTIVE", "SUPERVISOR" }.Contains(req.role?.ToUpper()))
            return BadRequest(new { status = false, message = "نقش تایید نامعتبر است." });

        string role = req.role!.ToUpper();
        await using var conn = _db.CreateConnection();

        var user = await conn.QueryRowDict(
            "SELECT roles, region_id FROM users WHERE national_id=@nid", new { nid = NationalId });
        if (user == null) return Unauthorized();

        int    regionId  = user.Int("region_id");
        string userRoles = user.Str("roles").ToUpper();

        var passCodes = await conn.QueryRowDict(
            "SELECT EXECUTIVEPass, SUPERVISORPass FROM final_results_approvals WHERE region_id=@rid",
            new { rid = regionId });

        if (passCodes == null
            || string.IsNullOrWhiteSpace(req.passcode1) || string.IsNullOrWhiteSpace(req.passcode2)
            || req.passcode1 != passCodes.Str("EXECUTIVEPass")
            || req.passcode2 != passCodes.Str("SUPERVISORPass"))
            return BadRequest(new { status = false, message = "رمز وارد شده صحیح نیست." });

        if (!userRoles.Contains(role))
            return StatusCode(403, new { status = false, message = "شما مجوز تایید با این نقش را ندارید." });

        return await DbHelper.WithTransaction(conn, async tx =>
        {
            await conn.ExecuteAsync(
                @"UPDATE final_results_approvals
                  SET executive_approved=1, executive_approved_by=@nid, executive_approved_at=GETDATE(),
                      supervisor_approved=1, supervisor_approved_by=@nid, supervisor_approved_at=GETDATE()
                  WHERE region_id=@rid",
                new { nid = NationalId, rid = regionId }, tx);

            var row = await conn.QueryRowDict(
                "SELECT executive_approved, supervisor_approved FROM final_results_approvals WITH (UPDLOCK, ROWLOCK) WHERE region_id=@rid",
                new { rid = regionId }, tx: tx);

            bool ea = Convert.ToBoolean(row?.GetValueOrDefault("executive_approved") ?? false);
            bool sa = Convert.ToBoolean(row?.GetValueOrDefault("supervisor_approved") ?? false);

            await conn.ExecuteAsync(
                "UPDATE final_results_approvals SET is_active=0 WHERE region_id=@rid",
                new { rid = regionId }, tx);

            return Ok(new
            {
                status = true,
                message = "با موفقیت ثبت شد.",
                data = new { executiveApproved = ea, supervisorApproved = sa, isActive = false }
            });
        });
    }

    // POST /api/publishFinalResults - admin publishes a single approved region with minutes.
    [HttpPost("publishFinalResults")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> PublishFinalResults([FromForm] int regionId, [FromForm] IFormFile? document)
    {
        if (document == null || document.Length == 0 || document.Length > 20 * 1024 * 1024)
            return BadRequest(new { status=false, message="بارگذاری صورتجلسه معتبر الزامی است (حداکثر ۲۰ مگابایت)." });
        var extension = Path.GetExtension(document.FileName).ToLowerInvariant();
        if (!new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" }.Contains(extension))
            return BadRequest(new { status=false, message="فرمت صورتجلسه مجاز نیست." });

        await using var conn = _db.CreateConnection();
        var role = await conn.QueryFirstOrDefaultAsync<string>(
            "SELECT roles FROM dbo.users WHERE national_id=@nid", new { nid=NationalId });
        if (!string.Equals(role?.Trim(), "ADMIN", StringComparison.OrdinalIgnoreCase))
            return StatusCode(403, new { status=false, message="انتشار نتایج فقط توسط مدیر مجاز است." });

        var approval = await conn.QueryRowDict(@"
            SELECT executive_approved, supervisor_approved
            FROM dbo.final_results_approvals WHERE region_id=@rid",
            new { rid=regionId });
        if (approval == null || !Convert.ToBoolean(approval.GetValueOrDefault("executive_approved") ?? false)
            || !Convert.ToBoolean(approval.GetValueOrDefault("supervisor_approved") ?? false))
            return BadRequest(new { status=false, message="تأیید اجرایی و نظارت این منطقه کامل نشده است." });

        var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "final-results");
        Directory.CreateDirectory(folder);
        var fileName = Guid.NewGuid().ToString("N") + extension;
        var fullPath = Path.Combine(folder, fileName);
        await using (var stream = System.IO.File.Create(fullPath))
            await document.CopyToAsync(stream);

        try
        {
            await conn.ExecuteAsync(@"
                IF OBJECT_ID(N'dbo.final_results_minutes',N'U') IS NULL
                    CREATE TABLE dbo.final_results_minutes (
                        region_id INT NOT NULL PRIMARY KEY,
                        file_path NVARCHAR(500) NOT NULL,
                        file_name NVARCHAR(255) NOT NULL,
                        published_by NVARCHAR(50) NOT NULL,
                        published_at DATETIME2 NOT NULL DEFAULT GETDATE()
                    );");
            await conn.ExecuteAsync(@"
                UPDATE dbo.final_results_approvals SET is_active=1
                WHERE region_id=@rid AND executive_approved=1 AND supervisor_approved=1;
                UPDATE dbo.final_results_minutes
                SET file_path=@path, file_name=@name, published_by=@nid, published_at=GETDATE()
                WHERE region_id=@rid;
                IF @@ROWCOUNT=0
                    INSERT INTO dbo.final_results_minutes(region_id,file_path,file_name,published_by)
                    VALUES(@rid,@path,@name,@nid);",
                new { rid=regionId, path="uploads/final-results/"+fileName,
                      name=Path.GetFileName(document.FileName), nid=NationalId });
            return Ok(new { status=true, message="صورتجلسه ثبت و نتایج منطقه منتشر شد." });
        }
        catch
        {
            System.IO.File.Delete(fullPath);
            throw;
        }
    }

    // POST /api/setFinalResultsApproval
    [HttpPost("setFinalResultsApproval")]
    public async Task<IActionResult> SetFinalResultsApproval()
    {
        await using var conn = _db.CreateConnection();

        var user = await conn.QueryRowDict(
            "SELECT roles, region_id FROM users WHERE national_id=@nid", new { nid = NationalId });
        if (user == null) return Unauthorized();

        string userRoles = user.Str("roles").ToUpper();
        if (!new[] { "EXECUTIVE", "SUPERVISOR" }.Contains(userRoles))
            return BadRequest(new { status = false, message = "نقش تایید نامعتبر است." });

        int regionId = user.Int("region_id");

        return await DbHelper.WithTransaction(conn, async tx =>
        {
            var row = await conn.QueryRowDict(
                "SELECT executive_approved, supervisor_approved FROM final_results_approvals WITH (UPDLOCK, ROWLOCK) WHERE region_id=@rid",
                new { rid = regionId }, tx: tx);

            bool ea = Convert.ToBoolean(row?.GetValueOrDefault("executive_approved") ?? false);
            bool sa = Convert.ToBoolean(row?.GetValueOrDefault("supervisor_approved") ?? false);
            bool isActive = ea && sa;

            await conn.ExecuteAsync(
                "UPDATE final_results_approvals SET is_active=@ia WHERE region_id=@rid",
                new { ia = isActive ? 1 : 0, rid = regionId }, tx);

            return Ok(new
            {
                status = true,
                message = "با موفقیت ثبت شد.",
                data = new { executiveApproved = ea, supervisorApproved = sa, isActive }
            });
        });
    }
}

public record SubmitApprovalRequest(string? role, string? passcode1, string? passcode2);

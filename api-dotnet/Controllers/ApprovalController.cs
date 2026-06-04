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
            "SELECT roles, region_id FROM users WHERE national_id=@nid LIMIT 1", new { nid = NationalId });
        if (user == null) return Unauthorized();

        int regionId = user.Int("region_id");

        var row = await conn.QueryRowDict(
            "SELECT executive_approved, supervisor_approved, is_active FROM final_results_approvals WHERE region_id=@rid",
            new { rid = regionId });

        if (row == null)
        {
            await conn.ExecuteAsync(
                "INSERT IGNORE INTO final_results_approvals (region_id, EXECUTIVEPass, SUPERVISORPass) VALUES (@rid,@ep,@sp)",
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

    // POST /api/submitFinalResultsApproval  {role, passcode1, passcode2}
    [HttpPost("submitFinalResultsApproval")]
    public async Task<IActionResult> SubmitFinalResultsApproval([FromBody] SubmitApprovalRequest req)
    {
        if (!new[] { "EXECUTIVE", "SUPERVISOR" }.Contains(req.role?.ToUpper()))
            return BadRequest(new { status = false, message = "نقش تایید نامعتبر است." });

        string role = req.role!.ToUpper();
        await using var conn = _db.CreateConnection();

        var user = await conn.QueryRowDict(
            "SELECT roles, region_id FROM users WHERE national_id=@nid LIMIT 1", new { nid = NationalId });
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
                  SET executive_approved=1, executive_approved_by=@nid, executive_approved_at=NOW(),
                      supervisor_approved=1, supervisor_approved_by=@nid, supervisor_approved_at=NOW()
                  WHERE region_id=@rid",
                new { nid = NationalId, rid = regionId }, tx);

            var row = await conn.QueryRowDict(
                "SELECT executive_approved, supervisor_approved FROM final_results_approvals WHERE region_id=@rid FOR UPDATE",
                tx: tx);

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

    // POST /api/setFinalResultsApproval
    [HttpPost("setFinalResultsApproval")]
    public async Task<IActionResult> SetFinalResultsApproval()
    {
        await using var conn = _db.CreateConnection();

        var user = await conn.QueryRowDict(
            "SELECT roles, region_id FROM users WHERE national_id=@nid LIMIT 1", new { nid = NationalId });
        if (user == null) return Unauthorized();

        string userRoles = user.Str("roles").ToUpper();
        if (!new[] { "EXECUTIVE", "SUPERVISOR" }.Contains(userRoles))
            return BadRequest(new { status = false, message = "نقش تایید نامعتبر است." });

        int regionId = user.Int("region_id");

        return await DbHelper.WithTransaction(conn, async tx =>
        {
            var row = await conn.QueryRowDict(
                "SELECT executive_approved, supervisor_approved FROM final_results_approvals WHERE region_id=@rid FOR UPDATE",
                tx: tx);

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

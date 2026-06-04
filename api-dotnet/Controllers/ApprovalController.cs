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

        var user = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT roles, region_id FROM users WHERE national_id=@nid LIMIT 1", new { nid = NationalId });
        if (user == null) return Unauthorized();

        int regionId = (int)(user.region_id ?? 0);

        var row = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT executive_approved, supervisor_approved, is_active FROM final_results_approvals WHERE region_id=@rid",
            new { rid = regionId });

        if (row == null)
        {
            string ep = GeneratePass(regionId);
            string sp = GeneratePass(regionId);
            await conn.ExecuteAsync(
                "INSERT IGNORE INTO final_results_approvals (region_id, EXECUTIVEPass, SUPERVISORPass) VALUES (@rid,@ep,@sp)",
                new { rid = regionId, ep, sp });
        }

        return Ok(new
        {
            status = true,
            data = new
            {
                executiveApproved = (bool)((int)(row?.executive_approved ?? 0) == 1),
                supervisorApproved = (bool)((int)(row?.supervisor_approved ?? 0) == 1),
                isActive = (bool)((int)(row?.is_active ?? 0) == 1)
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

        var user = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT roles, region_id FROM users WHERE national_id=@nid LIMIT 1", new { nid = NationalId });
        if (user == null) return Unauthorized();

        int regionId = (int)(user.region_id ?? 0);

        var passCodes = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT EXECUTIVEPass, SUPERVISORPass FROM final_results_approvals WHERE region_id=@rid",
            new { rid = regionId });

        if (passCodes == null
            || string.IsNullOrWhiteSpace(req.passcode1) || string.IsNullOrWhiteSpace(req.passcode2)
            || req.passcode1 != (string)passCodes.EXECUTIVEPass
            || req.passcode2 != (string)passCodes.SUPERVISORPass)
            return BadRequest(new { status = false, message = "رمز وارد شده صحیح نیست." });

        string userRoles = ((string)(user.roles ?? "")).ToUpper();
        if (!userRoles.Contains(role))
            return StatusCode(403, new { status = false, message = "شما مجوز تایید با این نقش را ندارید." });

        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            await conn.ExecuteAsync(
                @"UPDATE final_results_approvals
                  SET executive_approved=1, executive_approved_by=@nid, executive_approved_at=NOW(),
                      supervisor_approved=1, supervisor_approved_by=@nid, supervisor_approved_at=NOW()
                  WHERE region_id=@rid",
                new { nid = NationalId, rid = regionId }, tx);

            var row = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT executive_approved, supervisor_approved FROM final_results_approvals WHERE region_id=@rid FOR UPDATE",
                new { rid = regionId }, tx);

            int ea = (int)(row?.executive_approved ?? 0);
            int sa = (int)(row?.supervisor_approved ?? 0);
            int isActive = 0; // stays 0 per original logic

            await conn.ExecuteAsync(
                "UPDATE final_results_approvals SET is_active=@ia WHERE region_id=@rid",
                new { ia = isActive, rid = regionId }, tx);

            await tx.CommitAsync();
            return Ok(new
            {
                status = true,
                message = "با موفقیت ثبت شد.",
                data = new { executiveApproved = ea == 1, supervisorApproved = sa == 1, isActive = isActive == 1 }
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // POST /api/setFinalResultsApproval
    [HttpPost("setFinalResultsApproval")]
    public async Task<IActionResult> SetFinalResultsApproval()
    {
        await using var conn = _db.CreateConnection();

        var user = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT roles, region_id FROM users WHERE national_id=@nid LIMIT 1", new { nid = NationalId });
        if (user == null) return Unauthorized();

        string userRoles = ((string)(user.roles ?? "")).ToUpper();
        if (!new[] { "EXECUTIVE", "SUPERVISOR" }.Contains(userRoles))
            return BadRequest(new { status = false, message = "نقش تایید نامعتبر است." });

        int regionId = (int)(user.region_id ?? 0);

        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            var row = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT executive_approved, supervisor_approved FROM final_results_approvals WHERE region_id=@rid FOR UPDATE",
                new { rid = regionId }, tx);

            int ea = (int)(row?.executive_approved ?? 0);
            int sa = (int)(row?.supervisor_approved ?? 0);
            int isActive = ea == 1 && sa == 1 ? 1 : 0;

            await conn.ExecuteAsync(
                "UPDATE final_results_approvals SET is_active=@ia WHERE region_id=@rid",
                new { ia = isActive, rid = regionId }, tx);

            await tx.CommitAsync();
            return Ok(new
            {
                status = true,
                message = "با موفقیت ثبت شد.",
                data = new { executiveApproved = ea == 1, supervisorApproved = sa == 1, isActive = isActive == 1 }
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}

public record SubmitApprovalRequest(string? role, string? passcode1, string? passcode2);

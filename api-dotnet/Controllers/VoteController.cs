using System.Security.Cryptography;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class VoteController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly JalaliService _jalali;

    public VoteController(DatabaseService db, JalaliService jalali)
    {
        _db = db;
        _jalali = jalali;
    }

    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";

    // GET /api/createVoteToken
    [HttpGet("createVoteToken")]
    public async Task<IActionResult> CreateVoteToken()
    {
        await using var conn = _db.CreateConnection();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            // Check if already voted
            var already = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT usernationalid FROM voters WHERE usernationalid=@nid", new { nid = NationalId }, tx);
            if (already != null)
            {
                await tx.RollbackAsync();
                return StatusCode(403, new { status = false, message = "شما قبلا رأی داده‌اید" });
            }

            var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLower();
            var expires = DateTime.Now.AddMinutes(2).ToString("yyyy-MM-dd HH:mm:ss");

            await conn.ExecuteAsync(
                "INSERT INTO voting_tokens (user_id, token_hash, expires_at) VALUES (@nid, @hash, @exp)",
                new { nid = NationalId, hash = tokenHash, exp = expires }, tx);

            await tx.CommitAsync();
            return Ok(new { status = true, vote_token = rawToken, expires_in = 120 });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // POST /api/insertVote  {candidateIds: [], vote_token: ""}
    [HttpPost("insertVote")]
    public async Task<IActionResult> InsertVote([FromBody] InsertVoteRequest req)
    {
        if (req.candidateIds == null || req.candidateIds.Length == 0 || string.IsNullOrWhiteSpace(req.vote_token))
            return BadRequest(new { status = false, message = "پارامتر الزامی است." });

        await using var conn = _db.CreateConnection();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            // Generate unique tracking code
            string trackingCode;
            do
            {
                trackingCode = Convert.ToHexString(RandomNumberGenerator.GetBytes(6)).ToUpper();
                var exists = await conn.QueryFirstOrDefaultAsync<int?>(
                    "SELECT id FROM election_participants WHERE tracking_code=@tc LIMIT 1",
                    new { tc = trackingCode }, tx);
                if (!exists.HasValue) break;
            } while (true);

            // Validate vote token
            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(req.vote_token))).ToLower();
            var token = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM voting_tokens WHERE token_hash=@hash AND user_id=@nid LIMIT 1 FOR UPDATE",
                new { hash = tokenHash, nid = NationalId }, tx);

            if (token == null)
            {
                await tx.RollbackAsync();
                return StatusCode(403, new { status = false, message = "توکن نامعتبر" });
            }
            if ((int)token.used == 1)
            {
                await tx.RollbackAsync();
                return StatusCode(409, new { status = false, message = "شما قبلا رأی خود را ثبت کرده‌اید" });
            }
            if ((DateTime)token.expires_at < DateTime.Now)
            {
                await tx.RollbackAsync();
                return StatusCode(403, new { status = false, message = "زمان رأی‌گیری طولانی شده است، لطفا مجدد اقدام به ثبت رای نمایید" });
            }

            // Check max votes
            var maxRow = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT maxVotes FROM users JOIN maxvotes ON maxvotes.region_id=users.region_id WHERE national_id=@nid",
                new { nid = NationalId }, tx);
            int maxVotes = maxRow != null ? (int)maxRow.maxVotes : 1;

            if (req.candidateIds.Length > maxVotes)
            {
                await tx.RollbackAsync();
                return StatusCode(403, new { status = false, message = "تعداد کاندیداهای انتخابی صحیح نمی‌باشد" });
            }

            // Register participant if not yet
            var participant = await conn.QueryFirstOrDefaultAsync<int?>(
                "SELECT id FROM election_participants WHERE national_id=@nid LIMIT 1", new { nid = NationalId }, tx);
            if (!participant.HasValue)
                await conn.ExecuteAsync(
                    "INSERT INTO election_participants (national_id, tracking_code) VALUES (@nid,@tc)",
                    new { nid = NationalId, tc = trackingCode }, tx);

            // Insert votes
            foreach (var candidateId in req.candidateIds)
            {
                var dup = await conn.QueryFirstOrDefaultAsync<int?>(
                    "SELECT id FROM votes WHERE national_id=@nid AND candidate_id=@cid LIMIT 1",
                    new { nid = NationalId, cid = candidateId }, tx);
                if (dup.HasValue) continue;

                await conn.ExecuteAsync(
                    "INSERT INTO votes (national_id, candidate_id) VALUES (@nid, @cid)",
                    new { nid = NationalId, cid = candidateId }, tx);

                await conn.ExecuteAsync(
                    "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'ثبت رای',@desc)",
                    new { nid = NationalId, desc = $"کد {NationalId} به {candidateId} رای داد" }, tx);
            }

            // Mark token used
            await conn.ExecuteAsync(
                "UPDATE voting_tokens SET used=1, used_at=NOW() WHERE id=@id",
                new { id = (int)token.id }, tx);

            await tx.CommitAsync();
            return Ok(new
            {
                status = true,
                message = "ثبت رای با موفقیت انجام شد.",
                data = new { tracking_code = trackingCode }
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // GET /api/getVote
    [HttpGet("getVote")]
    public async Task<IActionResult> GetVote()
    {
        await using var conn = _db.CreateConnection();

        var votes = (await conn.QueryAsync<dynamic>(
            @"SELECT fi.id AS codeentekhabati, v.candidate_id, v.created_at,
                     f.tracking_code, u.first_name, u.last_name
              FROM votes v
              JOIN final_submissions fi ON fi.id=v.candidate_id
              JOIN users u ON fi.nationalid=u.national_id
              JOIN election_participants f ON f.national_id=v.national_id
              WHERE v.national_id=@nid", new { nid = NationalId })).AsList();

        if (votes.Any())
        {
            var list = votes.Select(r =>
            {
                var d = (IDictionary<string, object>)r;
                if (d["created_at"] is DateTime dt)
                {
                    d["date1"] = _jalali.Format(dt, "l j F Y");
                    d["Time1"] = _jalali.Format(dt, "H:i");
                }
                return d;
            });
            return Ok(new { status = true, message = "دریافت لیست رای‌ها با موفقیت انجام شد.", data = list });
        }

        // Return max votes if no votes yet
        var maxRow = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT maxVotes FROM users JOIN maxvotes ON maxvotes.region_id=users.region_id WHERE national_id=@nid",
            new { nid = NationalId });
        int maxVotes = maxRow != null ? (int)maxRow.maxVotes : 1;

        return Ok(new { status = true, message = "دریافت لیست رای‌ها با موفقیت انجام شد.", data = maxVotes });
    }

    // GET /api/getInfoVote  (alias of getVote)
    [HttpGet("getInfoVote")]
    public Task<IActionResult> GetInfoVote() => GetVote();

    // GET /api/searchUserVotes?q=...&limit=200
    [HttpGet("searchUserVotes")]
    public async Task<IActionResult> SearchUserVotes([FromQuery] string? q, [FromQuery] int limit = 200)
    {
        await using var conn = _db.CreateConnection();

        var currentUser = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT u.roles, u.region_id, r.ProvinceCode
              FROM users u LEFT JOIN region r ON r.id=u.region_id
              WHERE u.national_id=@nid LIMIT 1", new { nid = NationalId });

        if (currentUser == null) return Forbid();

        string role = ((string)currentUser.roles).ToUpper();
        bool isAdmin = role.Contains("ADMIN");
        bool isSupervisor = role.Contains("SUPERVISOR");

        if (!isAdmin && !isSupervisor)
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });

        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(new { status = true, data = new { scope = "all", items = Array.Empty<object>(), summary = Array.Empty<object>() }, message = "برای جستجو حداقل دو کاراکتر وارد کنید." });

        limit = Math.Clamp(limit <= 0 ? 200 : limit, 1, 1000);

        var whereParts = new List<string>
        {
            $"(vu.national_id LIKE @q OR vu.personnel_code LIKE @q OR vu.first_name LIKE @q OR vu.last_name LIKE @q OR CONCAT(COALESCE(vu.first_name,''),' ',COALESCE(vu.last_name,'')) LIKE @q)"
        };

        string scope = "all";
        if (!isAdmin)
        {
            int regionId = (int)currentUser.region_id;
            int provinceCode = currentUser.ProvinceCode != null ? (int)currentUser.ProvinceCode : 0;
            if (regionId.ToString().EndsWith("00") && provinceCode > 0)
            {
                whereParts.Add($"vr.ProvinceCode={provinceCode}");
                scope = "province";
            }
            else
            {
                whereParts.Add($"vu.region_id={regionId}");
                scope = "region";
            }
        }

        var sql = $@"SELECT
            vu.id AS voter_user_id, vu.national_id AS voter_national_id,
            vu.first_name AS voter_first_name, vu.last_name AS voter_last_name,
            vu.personnel_code AS voter_personnel_code,
            vu.region_id AS voter_region_id, vr.Name AS voter_region_name,
            vr.ProvinceCode AS voter_province_code, vp.Name AS voter_province_name,
            ep.tracking_code, ep.created_at AS participant_created_at,
            v.id AS vote_id, v.created_at AS voted_at,
            f.id AS candidate_submission_id, f.tracking_code AS candidate_tracking_code,
            cu.national_id AS candidate_national_id,
            cu.first_name AS candidate_first_name, cu.last_name AS candidate_last_name,
            cu.org_position_desc AS candidate_position,
            cu.region_id AS candidate_region_id, cr.Name AS candidate_region_name,
            cp.Name AS candidate_province_name
          FROM users vu
          LEFT JOIN region vr ON vr.id=vu.region_id
          LEFT JOIN region vp ON vp.id=(vr.ProvinceCode*100)
          LEFT JOIN election_participants ep ON ep.national_id=vu.national_id
          LEFT JOIN votes v ON v.national_id=vu.national_id
          LEFT JOIN final_submissions f ON f.id=v.candidate_id
          LEFT JOIN users cu ON cu.national_id=f.nationalId
          LEFT JOIN region cr ON cr.id=cu.region_id
          LEFT JOIN region cp ON cp.id=(cr.ProvinceCode*100)
          WHERE {string.Join(" AND ", whereParts)}
          ORDER BY vu.id DESC, v.created_at DESC
          LIMIT {limit}";

        var rows = (await conn.QueryAsync<dynamic>(sql, new { q = $"%{q}%" })).AsList();

        var items = rows.Select(r =>
        {
            var d = (IDictionary<string, object>)r;
            if (d["voted_at"] is DateTime vt)
                d["voted_at_shamsi"] = _jalali.FormatShort(vt);
            if (d["participant_created_at"] is DateTime pct)
                d["participant_created_at_shamsi"] = _jalali.FormatShort(pct);
            return d;
        }).ToList();

        // Build summary
        var summary = new Dictionary<string, object>();
        foreach (var row in items)
        {
            var d = (IDictionary<string, object>)row;
            var voterId = (string)d["voter_national_id"];
            if (!summary.ContainsKey(voterId))
                summary[voterId] = new
                {
                    national_id = voterId,
                    first_name = d["voter_first_name"],
                    last_name = d["voter_last_name"],
                    personnel_code = d["voter_personnel_code"],
                    region_id = d["voter_region_id"],
                    region_name = d["voter_region_name"],
                    province_name = d["voter_province_name"],
                    tracking_code = d["tracking_code"],
                    vote_count = items.Count(x => ((IDictionary<string, object>)x)["voter_national_id"]?.ToString() == voterId
                                                && ((IDictionary<string, object>)x)["vote_id"] != null)
                };
        }

        await conn.ExecuteAsync(
            "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'جستجوی آرای کاربر',@desc)",
            new { nid = NationalId, desc = $"جستجوی آرای کاربران با عبارت {q}" });

        return Ok(new
        {
            status = true,
            data = new { scope, items, summary = summary.Values },
            message = "جستجوی آرای کاربران با موفقیت انجام شد."
        });
    }
}

public record InsertVoteRequest(int[] candidateIds, string vote_token);

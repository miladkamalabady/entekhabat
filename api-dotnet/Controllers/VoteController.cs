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
    private readonly BaleService _bale;

    public VoteController(DatabaseService db, JalaliService jalali, BaleService bale)
    {
        _db = db;
        _jalali = jalali;
        _bale = bale;
    }

    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";

    // GET /api/createVoteToken
    [HttpGet("createVoteToken")]
    [HttpPost("createVoteToken")]
    public async Task<IActionResult> CreateVoteToken()
    {
        await using var conn = _db.CreateConnection();

        return await DbHelper.WithTransaction(conn, async tx =>
        {
            var already = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT usernationalid FROM voters WHERE usernationalid=@nid", new { nid = NationalId }, tx);
            if (already != null)
                return StatusCode(403, new { status = false, message = "شما قبلا رأی داده‌اید" });

            var rawToken  = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLower();
            var expires   = DateTime.Now.AddMinutes(2).ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

            await conn.ExecuteAsync(
                "INSERT INTO voting_tokens (user_id, token_hash, expires_at) VALUES (@nid, @hash, @exp)",
                new { nid = NationalId, hash = tokenHash, exp = expires }, tx);

            return Ok(new { status = true, vote_token = rawToken, expires_in = 120 });
        });
    }

    // POST /api/insertVote  {candidateIds: [], vote_token: ""}
    [HttpPost("insertVote")]
    public async Task<IActionResult> InsertVote([FromBody] InsertVoteRequest req)
    {
        if (req.candidateIds == null || req.candidateIds.Length == 0 || string.IsNullOrWhiteSpace(req.vote_token))
            return BadRequest(new { status = false, message = "پارامتر الزامی است." });

        await using var conn = _db.CreateConnection();

        return await DbHelper.WithTransaction(conn, async tx =>
        {
            // کد رهگیری یکتا
            string trackingCode;
            do
            {
                trackingCode = Convert.ToHexString(RandomNumberGenerator.GetBytes(6)).ToUpper();
                var exists = await conn.QueryFirstOrDefaultAsync<int?>(
                    "SELECT id FROM election_participants WHERE tracking_code=@tc LIMIT 1",
                    new { tc = trackingCode }, tx);
                if (!exists.HasValue) break;
            } while (true);

            // اعتبارسنجی توکن
            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(req.vote_token))).ToLower();
            var token = await conn.QueryRowDict(
                "SELECT * FROM voting_tokens WHERE token_hash=@hash AND user_id=@nid LIMIT 1 FOR UPDATE",
                new { hash = tokenHash, nid = NationalId }, tx);

            if (token == null)
                return StatusCode(403, new { status = false, message = "توکن نامعتبر" });
            if (Convert.ToInt32(token.GetValueOrDefault("used") ?? 0) == 1)
                return StatusCode(409, new { status = false, message = "شما قبلا رأی خود را ثبت کرده‌اید" });
            if (token.GetValueOrDefault("expires_at") is DateTime exp && exp < DateTime.Now)
                return StatusCode(403, new { status = false, message = "زمان رأی‌گیری طولانی شده است، لطفا مجدد اقدام به ثبت رای نمایید" });

            // حداکثر رأی مجاز
            var maxRow = await conn.QueryRowDict(
                "SELECT maxVotes FROM users JOIN maxvotes ON maxvotes.region_id=users.region_id WHERE national_id=@nid",
                new { nid = NationalId }, tx);
            int maxVotes = Convert.ToInt32(maxRow?.GetValueOrDefault("maxVotes") ?? 1);

            if (req.candidateIds.Length > maxVotes)
                return StatusCode(403, new { status = false, message = "تعداد کاندیداهای انتخابی صحیح نمی‌باشد" });

            // ثبت شرکت‌کننده
            var participant = await conn.QueryFirstOrDefaultAsync<int?>(
                "SELECT id FROM election_participants WHERE national_id=@nid LIMIT 1", new { nid = NationalId }, tx);
            if (!participant.HasValue)
                await conn.ExecuteAsync(
                    "INSERT INTO election_participants (national_id, tracking_code) VALUES (@nid,@tc)",
                    new { nid = NationalId, tc = trackingCode }, tx);

            // ثبت رأی‌ها
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

            // مصرف توکن
            int tokenId = Convert.ToInt32(token.GetValueOrDefault("id") ?? 0);
            await conn.ExecuteAsync(
                "UPDATE voting_tokens SET used=1, used_at=NOW() WHERE id=@id",
                new { id = tokenId }, tx);

            var mobile = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT mobile FROM users WHERE national_id=@nid LIMIT 1", new { nid = NationalId }, tx);
            if (!string.IsNullOrWhiteSpace(mobile))
                _bale.SendAsync(mobile, $"رأی شما با موفقیت ثبت شد. کد رهگیری: {trackingCode}");

            return Ok(new
            {
                status = true,
                message = "ثبت رای با موفقیت انجام شد.",
                data = new { tracking_code = trackingCode }
            });
        });
    }

    // GET /api/getVote
    [HttpGet("getVote")]
    public async Task<IActionResult> GetVote()
    {
        await using var conn = _db.CreateConnection();

        var votes = (await conn.QueryListDict(
            @"SELECT fi.id AS codeentekhabati, v.candidate_id, v.created_at,
                     f.tracking_code, u.first_name, u.last_name
              FROM votes v
              JOIN final_submissions fi ON fi.id=v.candidate_id
              JOIN users u ON fi.nationalid=u.national_id
              JOIN election_participants f ON f.national_id=v.national_id
              WHERE v.national_id=@nid", new { nid = NationalId })).AsList();

        if (votes.Any())
        {
            foreach (var r in votes)
            {
                if (r.GetValueOrDefault("created_at") is DateTime dt)
                {
                    r["date1"] = _jalali.Format(dt, "l j F Y");
                    r["Time1"] = _jalali.Format(dt, "H:i");
                }
            }
            return Ok(new { status = true, message = "دریافت لیست رای‌ها با موفقیت انجام شد.", data = votes });
        }

        var maxRow = await conn.QueryRowDict(
            "SELECT maxVotes FROM users JOIN maxvotes ON maxvotes.region_id=users.region_id WHERE national_id=@nid",
            new { nid = NationalId });
        int maxVotes = Convert.ToInt32(maxRow?.GetValueOrDefault("maxVotes") ?? 1);

        return Ok(new { status = true, message = "دریافت لیست رای‌ها با موفقیت انجام شد.", data = maxVotes });
    }

    // GET /api/getInfoVote?province=11&region=5 - آمار زنده انتخابات (province=ProvinceCode, region=region_id اختیاری)
    [HttpGet("getInfoVote")]
    public async Task<IActionResult> GetInfoVote([FromQuery] int? province = null, [FromQuery] int? region = null)
    {
        await using var conn = _db.CreateConnection();

        var p = province.HasValue ? (object)province.Value : DBNull.Value;
        var r = region.HasValue ? (object)region.Value : DBNull.Value;

        // eligible voters: filtered by region > province > all
        var totalVoters = await conn.QueryFirstOrDefaultAsync<int>(@"
            SELECT COALESCE(SUM(fra.totalEligible), 0)
            FROM final_results_approvals fra
            JOIN region reg ON reg.id = fra.region_id
            WHERE (@r IS NOT NULL AND fra.region_id = @r)
               OR (@r IS NULL AND (@p IS NULL OR reg.ProvinceCode = @p))",
            new { p, r });

        // total votes cast: filtered by voter's region > province > all
        var totalVotes = await conn.QueryFirstOrDefaultAsync<int>(@"
            SELECT COUNT(DISTINCT v.national_id)
            FROM votes v
            JOIN users u ON u.national_id = v.national_id
            JOIN region reg ON reg.id = u.region_id
            WHERE (@r IS NOT NULL AND u.region_id = @r)
               OR (@r IS NULL AND (@p IS NULL OR reg.ProvinceCode = @p))",
            new { p, r });

        var participants = await conn.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(*) FROM election_participants");

        var totalCandidates = await conn.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(*) FROM final_submissions");

        var activeCandidates = await conn.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(*) FROM final_submissions WHERE requestStatus='SUPERVISION_APPROVED'");

        // vote_count per candidate filtered by voter's region > province > all
        var listCan = (await conn.QueryAsync<dynamic>(@"
            SELECT fi.id AS codeentekhabati,
                   u.national_id, u.first_name, u.last_name,
                   u.org_position_desc, u.gender, u.region_id,
                   reg.name AS regname,
                   ud.user_photo,
                   COUNT(CASE WHEN (@r IS NOT NULL AND voter.region_id = @r)
                                OR (@r IS NULL AND (@p IS NULL OR vr.ProvinceCode = @p))
                              THEN v.id ELSE NULL END) AS vote_count,
                   fi.requestStatus
            FROM final_submissions fi
            JOIN users u ON u.national_id = fi.nationalId
            LEFT JOIN region reg ON reg.id = u.region_id
            LEFT JOIN user_documents ud ON ud.nationalId = fi.nationalId
            LEFT JOIN votes v ON v.candidate_id = fi.id
            LEFT JOIN users voter ON voter.national_id = v.national_id
            LEFT JOIN region vr ON vr.id = voter.region_id
            WHERE fi.requestStatus = 'SUPERVISION_APPROVED'
            GROUP BY fi.id, u.national_id, u.first_name, u.last_name,
                     u.org_position_desc, u.gender, u.region_id, reg.name,
                     ud.user_photo, fi.requestStatus
            ORDER BY vote_count DESC",
            new { p, r })).AsList();

        var regionVoteStatsRaw = (await conn.QueryAsync<dynamic>(@"
            SELECT u.region_id, COUNT(v.id) AS votes
            FROM votes v
            JOIN final_submissions fi ON fi.id = v.candidate_id
            JOIN users u ON u.national_id = fi.nationalId
            GROUP BY u.region_id")).AsList();

        var regionVoteStats = regionVoteStatsRaw.ToDictionary(
            r => (object)r.region_id,
            r => (object)new { votes = Convert.ToInt32(r.votes) });

        var provinceVoteStatsRaw = (await conn.QueryAsync<dynamic>(@"
            SELECT (r.ProvinceCode * 100) AS province_id,
                   COUNT(CASE WHEN (@r IS NOT NULL AND voter.region_id = @r)
                                OR (@r IS NULL AND (@p IS NULL OR vr.ProvinceCode = @p))
                              THEN v.id ELSE NULL END) AS votes,
                   COUNT(DISTINCT CASE WHEN (@r IS NOT NULL AND voter.region_id = @r)
                                         OR (@r IS NULL AND (@p IS NULL OR vr.ProvinceCode = @p))
                                       THEN v.national_id ELSE NULL END) AS eligible
            FROM votes v
            JOIN final_submissions fi ON fi.id = v.candidate_id
            JOIN users u ON u.national_id = fi.nationalId
            JOIN region r ON r.id = u.region_id
            LEFT JOIN users voter ON voter.national_id = v.national_id
            LEFT JOIN region vr ON vr.id = voter.region_id
            WHERE r.ProvinceCode > 0
            GROUP BY r.ProvinceCode",
            new { p, r })).AsList();

        var eligibleByProvince = (await conn.QueryAsync<dynamic>(@"
            SELECT r.ProvinceCode, COALESCE(SUM(fra.totalEligible), 0) AS eligible
            FROM final_results_approvals fra
            JOIN region r ON r.id = fra.region_id
            WHERE r.ProvinceCode > 0
            GROUP BY r.ProvinceCode")).AsList();

        var eligibleMap = eligibleByProvince.ToDictionary(
            r => Convert.ToInt64(r.ProvinceCode) * 100L,
            r => Convert.ToInt32(r.eligible));

        var provinceVoteStats = provinceVoteStatsRaw.ToDictionary(
            r => (object)r.province_id,
            r => (object)new {
                votes = Convert.ToInt32(r.votes),
                eligible = eligibleMap.GetValueOrDefault((long)r.province_id, 0)
            });

        var eligiblePerProvince = eligibleMap.ToDictionary(
            kvp => (object)(long)kvp.Key,
            kvp => (object)kvp.Value);

        return Ok(new
        {
            status = true,
            data = new
            {
                totalVoters,
                totalVotes,
                participants,
                Candidates = totalCandidates,
                activeCandidates,
                listCan,
                regionVoteStats,
                provinceVoteStats,
                eligiblePerProvince
            }
        });
    }

    // GET /api/searchUserVotes?q=...&limit=200
    [HttpGet("searchUserVotes")]
    public async Task<IActionResult> SearchUserVotes([FromQuery] string? q, [FromQuery] int limit = 200)
    {
        await using var conn = _db.CreateConnection();

        var currentUser = await conn.QueryRowDict(
            @"SELECT u.roles, u.region_id, r.ProvinceCode
              FROM users u LEFT JOIN region r ON r.id=u.region_id
              WHERE u.national_id=@nid LIMIT 1", new { nid = NationalId });

        if (currentUser == null) return Forbid();

        string role    = (currentUser.Str("roles")).ToUpper();
        bool isAdmin   = role.Contains("ADMIN");
        bool isSupervisor = role.Contains("SUPERVISOR");

        if (!isAdmin && !isSupervisor)
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });

        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(new { status = true, data = new { scope = "all", items = Array.Empty<object>(), summary = Array.Empty<object>() }, message = "برای جستجو حداقل دو کاراکتر وارد کنید." });

        limit = Math.Clamp(limit <= 0 ? 200 : limit, 1, 1000);

        var whereParts = new List<string>
        {
            "(vu.national_id LIKE @q OR vu.personnel_code LIKE @q OR vu.first_name LIKE @q OR vu.last_name LIKE @q OR CONCAT(COALESCE(vu.first_name,''),' ',COALESCE(vu.last_name,'')) LIKE @q)"
        };

        string scope = "all";
        if (!isAdmin)
        {
            int regionId     = currentUser.Int("region_id");
            int provinceCode = currentUser.Int("ProvinceCode");
            if (regionId.ToString().EndsWith("00") && provinceCode > 0)
            { whereParts.Add($"vr.ProvinceCode={provinceCode}"); scope = "province"; }
            else
            { whereParts.Add($"vu.region_id={regionId}"); scope = "region"; }
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

        var rows = (await conn.QueryListDict(sql, new { q = $"%{q}%" })).AsList();

        foreach (var r in rows)
        {
            if (r.GetValueOrDefault("voted_at") is DateTime vt)
                r["voted_at_shamsi"] = _jalali.FormatShort(vt);
            if (r.GetValueOrDefault("participant_created_at") is DateTime pct)
                r["participant_created_at_shamsi"] = _jalali.FormatShort(pct);
        }

        var summary = new Dictionary<string, object>();
        foreach (var row in rows)
        {
            var voterId = row.Str("voter_national_id");
            if (!summary.ContainsKey(voterId))
                summary[voterId] = new
                {
                    national_id    = voterId,
                    first_name     = row.GetValueOrDefault("voter_first_name"),
                    last_name      = row.GetValueOrDefault("voter_last_name"),
                    personnel_code = row.GetValueOrDefault("voter_personnel_code"),
                    region_id      = row.GetValueOrDefault("voter_region_id"),
                    region_name    = row.GetValueOrDefault("voter_region_name"),
                    province_name  = row.GetValueOrDefault("voter_province_name"),
                    tracking_code  = row.GetValueOrDefault("tracking_code"),
                    vote_count     = rows.Count(x => x.Str("voter_national_id") == voterId && x.GetValueOrDefault("vote_id") != null)
                };
        }

        await conn.ExecuteAsync(
            "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'جستجوی آرای کاربر',@desc)",
            new { nid = NationalId, desc = $"جستجوی آرای کاربران با عبارت {q}" });

        return Ok(new
        {
            status = true,
            data = new { scope, items = rows, summary = summary.Values },
            message = "جستجوی آرای کاربران با موفقیت انجام شد."
        });
    }

    // GET /api/exportResults?region=&province=  - خروجی CSV نتایج برای ادمین
    [HttpGet("exportResults")]
    public async Task<IActionResult> ExportResults([FromQuery] int? region = null, [FromQuery] int? province = null)
    {
        await using var conn = _db.CreateConnection();

        var me = await conn.QueryRowDict(
            "SELECT roles FROM users WHERE national_id=@nid LIMIT 1", new { nid = NationalId });
        if (me == null) return Unauthorized();

        if (me.Str("roles") != "ADMIN")
            return StatusCode(403, new { status = false, message = "فقط ادمین می‌تواند نتایج را دریافت کند." });

        var p = province.HasValue ? (object)province.Value : DBNull.Value;
        var r = region.HasValue   ? (object)region.Value   : DBNull.Value;

        var candidates = (await conn.QueryAsync<dynamic>(@"
            SELECT fi.id AS codeentekhabati,
                   u.national_id, u.first_name, u.last_name,
                   u.org_position_desc, u.gender, u.education, u.yearsOfService,
                   reg.name AS region_name, reg.ProvinceCode,
                   prov.Name AS province_name,
                   COUNT(CASE WHEN (@r IS NOT NULL AND voter.region_id = @r)
                                OR (@r IS NULL AND (@p IS NULL OR vr.ProvinceCode = @p))
                              THEN v.id ELSE NULL END) AS vote_count,
                   fi.requestStatus, fi.create_date
            FROM final_submissions fi
            JOIN users u ON u.national_id = fi.nationalId
            LEFT JOIN region reg ON reg.id = u.region_id
            LEFT JOIN region prov ON prov.id = (reg.ProvinceCode * 100)
            LEFT JOIN votes v ON v.candidate_id = fi.id
            LEFT JOIN users voter ON voter.national_id = v.national_id
            LEFT JOIN region vr ON vr.id = voter.region_id
            WHERE fi.requestStatus IN ('SUPERVISION_APPROVED','SUPERVISION_REJECTED','SUBMITTED')
            GROUP BY fi.id, u.national_id, u.first_name, u.last_name,
                     u.org_position_desc, u.gender, u.education, u.yearsOfService,
                     reg.name, reg.ProvinceCode, prov.Name, fi.requestStatus, fi.create_date
            ORDER BY vote_count DESC, u.last_name ASC",
            new { p, r })).AsList();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("ردیف,کد انتخاباتی,کد ملی,نام,نام خانوادگی,جنسیت,تحصیلات,سابقه خدمت,سمت سازمانی,استان,منطقه,وضعیت,تعداد آرا,تاریخ ثبت نام");

        int row = 1;
        foreach (IDictionary<string, object> c in candidates)
        {
            string statusFa = c.Str("requestStatus") switch
            {
                "SUPERVISION_APPROVED"  => "تایید شده",
                "SUPERVISION_REJECTED"  => "رد شده",
                "SUBMITTED"             => "در انتظار بررسی",
                _                       => c.Str("requestStatus")
            };

            string regDate = c.GetValueOrDefault("create_date") is DateTime dt
                ? _jalali.Format(dt, "Y/n/j") : "";

            sb.AppendLine(string.Join(",", new[]
            {
                row++.ToString(),
                CsvCell(c.Str("codeentekhabati")),
                CsvCell(c.Str("national_id")),
                CsvCell(c.Str("first_name")),
                CsvCell(c.Str("last_name")),
                CsvCell(c.Str("gender")),
                CsvCell(c.Str("education")),
                CsvCell(c.Str("yearsOfService")),
                CsvCell(c.Str("org_position_desc")),
                CsvCell(c.Str("province_name")),
                CsvCell(c.Str("region_name")),
                CsvCell(statusFa),
                c.GetValueOrDefault("vote_count")?.ToString() ?? "0",
                CsvCell(regDate)
            }));
        }

        await conn.ExecuteAsync(
            "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'دریافت خروجی اکسل نتایج',@desc)",
            new { nid = NationalId, desc = $"خروجی نتایج - منطقه:{region} استان:{province}" });

        // UTF-8 BOM برای نمایش صحیح فارسی در اکسل
        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var csvBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        var result = new byte[bom.Length + csvBytes.Length];
        bom.CopyTo(result, 0);
        csvBytes.CopyTo(result, bom.Length);

        var fileName = $"election_results_{DateTime.Now:yyyyMMdd_HHmm}.csv";
        return File(result, "text/csv; charset=utf-8", fileName);
    }

    private static string CsvCell(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}

public record InsertVoteRequest(int[] candidateIds, string vote_token);


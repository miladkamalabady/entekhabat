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


    // رمز دوم ادمین فقط از دیتابیس خوانده می‌شود و هیچ API برای تغییر آن وجود ندارد.
    private async Task EnsureAdminSecondPasswordSetting(System.Data.IDbConnection conn)
    {
        await conn.ExecuteAsync(@"IF OBJECT_ID(N'dbo.app_security_settings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.app_security_settings (
        setting_key NVARCHAR(100) NOT NULL PRIMARY KEY,
        setting_value NVARCHAR(500) NOT NULL,
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_app_security_settings_updated_at DEFAULT GETDATE()
    );
END;
IF NOT EXISTS (SELECT 1 FROM dbo.app_security_settings WHERE setting_key=N'ADMIN_SECOND_PASSWORD')
BEGIN
    INSERT INTO dbo.app_security_settings(setting_key, setting_value)
    VALUES (N'ADMIN_SECOND_PASSWORD', N'136555');
END;");
    }

    private async Task<bool> IsAdminWithSecondPassword(System.Data.IDbConnection conn, string? suppliedPassword)
    {
        var role = await conn.QueryFirstOrDefaultAsync<string>(
            "SELECT roles FROM dbo.users WHERE national_id=@nid", new { nid = NationalId });
        if (!string.Equals((role ?? "").Trim(), "ADMIN", StringComparison.OrdinalIgnoreCase))
            return false;

        await EnsureAdminSecondPasswordSetting(conn);
        var dbPassword = await conn.QueryFirstOrDefaultAsync<string>(
            "SELECT setting_value FROM dbo.app_security_settings WHERE setting_key=N'ADMIN_SECOND_PASSWORD'");

        if (string.IsNullOrEmpty(dbPassword) || suppliedPassword == null)
            return false;

        // مقایسه ثابت‌زمان برای جلوگیری از نشت زمانی ساده.
        var a = Encoding.UTF8.GetBytes(dbPassword);
        var b = Encoding.UTF8.GetBytes(suppliedPassword);
        return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
    }

    private async Task<bool> CanReviewEndedElectionResults(System.Data.IDbConnection conn)
    {
        var schedule = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT TOP (1) end_date FROM dbo.election_schedule_events WHERE event_key='voting'");
        if (schedule == null) return false;
        DateTime? end = _jalali.NormalizeToGregorian(schedule.end_date);
        if (!end.HasValue || DateTime.Now <= end.Value) return false;

        var roles = await conn.QueryFirstOrDefaultAsync<string>(
            "SELECT roles FROM dbo.users WHERE national_id=@nid", new { nid = NationalId });
        var roleSet = (roles ?? "").ToUpperInvariant()
            .Split(new[] { ',', ';', ' ', '|' }, StringSplitOptions.RemoveEmptyEntries);
        return roleSet.Contains("EXECUTIVE") || roleSet.Contains("SUPERVISOR");
    }

    private string? GetSecondPassword(string? queryPassword)
    {
        if (!string.IsNullOrWhiteSpace(queryPassword)) return queryPassword;
        return Request.Headers.TryGetValue("X-Admin-Second-Password", out var v) ? v.ToString() : null;
    }

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
            var expires   = DateTime.Now.AddMinutes(2);

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
                    "SELECT id FROM election_participants WHERE tracking_code=@tc",
                    new { tc = trackingCode }, tx);
                if (!exists.HasValue) break;
            } while (true);

            // اعتبارسنجی توکن
            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(req.vote_token))).ToLower();
            var token = await conn.QueryRowDict(
                "SELECT TOP (1) * FROM dbo.voting_tokens WITH (UPDLOCK, ROWLOCK) WHERE token_hash=@hash AND user_id=@nid",
                new { hash = tokenHash, nid = NationalId }, tx);

            if (token == null)
                return StatusCode(403, new { status = false, message = "توکن نامعتبر" });
            if (Convert.ToInt32(token.GetValueOrDefault("used") ?? 0) == 1)
                return StatusCode(409, new { status = false, message = "شما قبلا رأی خود را ثبت کرده‌اید" });
            if (token.GetValueOrDefault("expires_at") is DateTime exp && exp < DateTime.Now)
                return StatusCode(403, new { status = false, message = "زمان رأی‌گیری طولانی شده است، لطفا مجدد اقدام به ثبت رای نمایید" });

            // تعیین حوزه رأی مؤثر و سقف رأی مجاز
            // اگر منطقه کاربر ظرفیت داشته باشد، فقط همان منطقه معتبر است.
            // اگر ظرفیت منطقه کاربر صفر باشد، کاربر فقط می‌تواند یک منطقه دارای ظرفیت
            // از همان استان را به عنوان حوزه جایگزین انتخاب کند.
            var userRegion = await conn.QueryFirstOrDefaultAsync<dynamic>(
                @"
                SELECT r.id, r.ProvinceCode,
                       CASE
                           WHEN mv.maxVotes IS NOT NULL THEN mv.maxVotes
                           WHEN COUNT(CASE WHEN COALESCE(NULLIF(LTRIM(RTRIM(u2.roles)), N''), N'VOTER') = N'VOTER' THEN 1 END) < 1000 THEN 0
                           ELSE CEILING(COUNT(CASE WHEN COALESCE(NULLIF(LTRIM(RTRIM(u2.roles)), N''), N'VOTER') = N'VOTER' THEN 1 END) / 1000.0)
                       END AS allowedVotes
                FROM users u
                JOIN region r ON r.id=u.region_id
                OUTER APPLY (SELECT MAX(maxVotes) AS maxVotes FROM maxvotes WHERE region_id=r.id) mv
                LEFT JOIN users u2 ON u2.region_id=r.id
                WHERE u.national_id=@nid
                GROUP BY r.id, r.ProvinceCode, mv.maxVotes",
                new { nid = NationalId }, tx);

            if (userRegion == null)
                return BadRequest(new { status=false, message="منطقه رأی‌دهنده یافت نشد." });

            int userRegionId = Convert.ToInt32(userRegion.id);
            int provinceCode = Convert.ToInt32(userRegion.ProvinceCode);
            int allowedVotes = Convert.ToInt32(userRegion.allowedVotes);
            int effectiveRegionId = userRegionId;

            // Small regions vote for approved candidates within their own province.
            // No candidate from another province can be selected.
            bool provinceWideVoting = allowedVotes <= 0;
            if (provinceWideVoting)
            {
                allowedVotes = await conn.QueryFirstOrDefaultAsync<int>(@"
                    SELECT COALESCE(MAX(m.maxVotes),1)
                    FROM dbo.region r
                    LEFT JOIN dbo.maxvotes m ON m.region_id=r.id
                    WHERE r.ProvinceCode=@province AND r.id % 100 <> 0",
                    new { province = provinceCode }, tx);
                allowedVotes = Math.Max(1, allowedVotes);
            }
            else if (req.regionId.HasValue && req.regionId.Value != userRegionId)
            {
                return BadRequest(new { status=false, message="منطقه انتخابی با منطقه رأی‌دهنده مطابقت ندارد." });
            }

            // شناسه نامزدها باید یکتا باشد و همه نامزدها باید در حوزه رأی مؤثر،
            // فعال و تأییدشده باشند.
            var candidateIds = req.candidateIds.Distinct().ToArray();
            if (candidateIds.Length > allowedVotes)
                return StatusCode(403, new { status = false, message = "تعداد کاندیداهای انتخابی بیش از سقف مجاز منطقه است." });

            var validCandidateCount = await conn.QueryFirstOrDefaultAsync<int>(
                @"SELECT COUNT(*)
                  FROM final_submissions f
                  JOIN users cu ON cu.national_id=f.nationalId
                  WHERE f.id IN @candidateIds
                    AND f.requestStatus='SUPERVISION_APPROVED'
                    AND ((@provinceWideVoting=0 AND cu.region_id=@regionId)
                      OR (@provinceWideVoting=1 AND EXISTS (
                          SELECT 1 FROM dbo.region cr
                          WHERE cr.id=cu.region_id AND cr.ProvinceCode=@province)))",
                new { candidateIds, regionId=effectiveRegionId, provinceWideVoting, province=provinceCode }, tx);

            if (validCandidateCount != candidateIds.Length)
                return BadRequest(new { status=false, message="یک یا چند نامزد متعلق به حوزه مجاز رأی‌دهنده نیستند." });

            // ثبت شرکت‌کننده
            var participant = await conn.QueryFirstOrDefaultAsync<int?>(
                "SELECT id FROM election_participants WHERE national_id=@nid", new { nid = NationalId }, tx);
            if (!participant.HasValue)
                await conn.ExecuteAsync(
                    "INSERT INTO election_participants (national_id, tracking_code) VALUES (@nid,@tc)",
                    new { nid = NationalId, tc = trackingCode }, tx);

            // ثبت رأی‌ها
            foreach (var candidateId in candidateIds)
            {
                var dup = await conn.QueryFirstOrDefaultAsync<int?>(
                    "SELECT id FROM votes WHERE national_id=@nid AND candidate_id=@cid",
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
                "UPDATE voting_tokens SET used=1, used_at=GETDATE() WHERE id=@id",
                new { id = tokenId }, tx);

            var mobile = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT mobile FROM users WHERE national_id=@nid", new { nid = NationalId }, tx);
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
    public async Task<IActionResult> GetInfoVote([FromQuery] int? province = null, [FromQuery] int? region = null, [FromQuery] string? secondPassword = null)
    {
        await using var conn = _db.CreateConnection();

        // تا قبل از تایید نهایی آرا، مشاهده آمار رأی فقط برای ADMIN با رمز دوم مجاز است.
        var approvalPending = await conn.QueryFirstOrDefaultAsync<int>(@"
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM dbo.region rg
                LEFT JOIN dbo.final_results_approvals fra ON fra.region_id = rg.id
                WHERE rg.id % 100 <> 0
                  AND (@region IS NULL OR rg.id=@region)
                  AND (@region IS NOT NULL OR @province IS NULL OR rg.ProvinceCode=@province)
                  AND ISNULL(fra.is_active,0)=0
            ) THEN 1 ELSE 0 END", new { region, province });

        // آمار عمومی مشارکت بدون جزئیات رأی نامزدها در دسترس است.
        // دسترسی به اطلاعات نامزدها و شمارش رأی هر نامزد همچنان محدود می‌ماند.
        var canSeeCandidateVotes = false; // این API عمومی هرگز رأی نامزدها را برنمی‌گرداند.

        var p = province.HasValue ? (object)province.Value : DBNull.Value;
        var r = region.HasValue ? (object)region.Value : DBNull.Value;

        // eligible voters: filtered by region > province > all
        // Eligible totals come from election-region approval records, not users.
        var totalVoters = await conn.QueryFirstOrDefaultAsync<int>(@"
            SELECT COALESCE(SUM(CAST(fra.totalEligible AS BIGINT)),0)
            FROM dbo.final_results_approvals fra
            JOIN dbo.region rg ON rg.id=fra.region_id
            WHERE rg.id % 100 <> 0
              AND (@r IS NULL OR rg.id=@r)
              AND (@r IS NOT NULL OR @p IS NULL OR rg.ProvinceCode=@p)",
            new { p, r });

        // total votes cast: filtered by voter's region > province > all
        var totalVotes = await conn.QueryFirstOrDefaultAsync<int>(@"
            SELECT COUNT(DISTINCT v.national_id)
            FROM votes v
            JOIN users u ON u.national_id = v.national_id
            JOIN region reg ON reg.id = u.region_id
            WHERE EXISTS (SELECT 1 FROM dbo.final_results_approvals fra WHERE fra.region_id=u.region_id)
              AND ((@r IS NOT NULL AND u.region_id = @r)
               OR (@r IS NULL AND (@p IS NULL OR reg.ProvinceCode = @p)))",
            new { p, r });

        var participants = totalVotes;

        var totalCandidates = await conn.QueryFirstOrDefaultAsync<int>(@"
            SELECT COUNT(*) FROM final_submissions fi JOIN users u ON u.national_id=fi.nationalId
            JOIN region reg ON reg.id=u.region_id
            WHERE (@r IS NULL OR u.region_id=@r) AND (@r IS NOT NULL OR @p IS NULL OR reg.ProvinceCode=@p)", new { p, r });

        var activeCandidates = await conn.QueryFirstOrDefaultAsync<int>(@"
            SELECT COUNT(*) FROM final_submissions fi JOIN users u ON u.national_id=fi.nationalId
            JOIN region reg ON reg.id=u.region_id
            WHERE fi.requestStatus='SUPERVISION_APPROVED'
              AND (@r IS NULL OR u.region_id=@r) AND (@r IS NOT NULL OR @p IS NULL OR reg.ProvinceCode=@p)", new { p, r });

        // فهرست عمومی نامزدهای تاییدشده؛ بدون تعداد رأی یا رتبه‌بندی.
        var listCan = (await conn.QueryAsync<dynamic>(@"
            SELECT fi.id AS codeentekhabati,
                   u.first_name, u.last_name, u.org_position_desc,
                   u.gender, u.region_id, reg.name AS regname, reg.ProvinceCode AS provinceCode,
                   (SELECT TOP 1 pr.name FROM dbo.region pr WHERE pr.id = reg.ProvinceCode * 100) AS provinceName,
                   ud.user_photo, fi.requestStatus
            FROM final_submissions fi
            JOIN users u ON u.national_id = fi.nationalId
            LEFT JOIN region reg ON reg.id = u.region_id
            LEFT JOIN user_documents ud ON ud.nationalId = fi.nationalId
            WHERE fi.requestStatus = 'SUPERVISION_APPROVED'
              AND (@r IS NULL OR u.region_id = @r)
              AND (@r IS NOT NULL OR @p IS NULL OR reg.ProvinceCode = @p)
            ORDER BY u.last_name, u.first_name, fi.id", new { p, r })).AsList();

        // Participation belongs to the voter's registered region, never the candidate's region.
        var regional = (await conn.QueryAsync<dynamic>(@"
            SELECT rg.id AS region_id, rg.ProvinceCode,
                   COALESCE(MAX(CAST(fra.totalEligible AS BIGINT)),0) AS eligible,
                   COUNT(DISTINCT v.national_id) AS votes
            FROM dbo.region rg
            JOIN dbo.final_results_approvals fra ON fra.region_id=rg.id
            LEFT JOIN dbo.users voter ON voter.region_id=rg.id
            LEFT JOIN dbo.votes v ON v.national_id=voter.national_id
            WHERE rg.id % 100 <> 0 AND rg.ProvinceCode > 0
            GROUP BY rg.id, rg.ProvinceCode")).AsList();

        var electionRegionCount = regional.Count;

        var regionVoteStats = regional.ToDictionary(
            x => (object)x.region_id,
            x => (object)new { votes = Convert.ToInt32(x.votes), eligible = Convert.ToInt32(x.eligible) });

        var provinceVoteStats = regional.GroupBy(x => Convert.ToInt32(x.ProvinceCode))
            .ToDictionary(g => (object)(g.Key * 100),
                g => (object)new {
                    votes = g.Sum(x => Convert.ToInt32(x.votes)),
                    eligible = g.Sum(x => Convert.ToInt32(x.eligible))
                });

        var eligiblePerProvince = regional.GroupBy(x => Convert.ToInt32(x.ProvinceCode))
            .ToDictionary(g => (object)(g.Key * 100),
                g => (object)g.Sum(x => Convert.ToInt32(x.eligible)));

        return Ok(new
        {
            status = true,
            data = new
            {
                totalVoters,
                electionRegionCount,
                totalVotes,
                participants,
                Candidates = totalCandidates,
                activeCandidates,
                listCan,
                candidateVotesHidden = !canSeeCandidateVotes,
                regionVoteStats,
                provinceVoteStats,
                eligiblePerProvince
            }
        });
    }

    // GET /api/searchUserVotes?q=...&limit=200
    [HttpGet("searchUserVotes")]
    public async Task<IActionResult> SearchUserVotes([FromQuery] string? q, [FromQuery] int limit = 200, [FromQuery] string? secondPassword = null)
    {
        await using var conn = _db.CreateConnection();

        var currentUser = await conn.QueryRowDict(
            @"SELECT u.roles, u.region_id, r.ProvinceCode
              FROM users u LEFT JOIN region r ON r.id=u.region_id
              WHERE u.national_id=@nid", new { nid = NationalId });

        if (currentUser == null) return Forbid();

        string role = (currentUser.Str("roles")).Trim().ToUpperInvariant();
        bool isAdmin = role == "ADMIN";
        bool isSupervisor = role.Contains("SUPERVISOR");

        if (!isAdmin && !isSupervisor)
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید." });

        if (isAdmin)
        {
            var supplied = GetSecondPassword(secondPassword);
            if (!await IsAdminWithSecondPassword(conn, supplied))
                return StatusCode(403, new { status = false, secondPasswordRequired = true, message = "برای مشاهده جزئیات رأی کاربران ورود رمز دوم ادمین الزامی است." });
        }

        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(new { status = true, data = new { scope = "all", items = Array.Empty<object>(), summary = Array.Empty<object>() }, message = "برای جستجو حداقل دو کاراکتر وارد کنید." });

        limit = Math.Clamp(limit <= 0 ? 200 : limit, 1, 1000);

        var whereParts = new List<string>
        {
            @"(vu.national_id LIKE @q
               OR vu.personnel_code LIKE @q
               OR REPLACE(REPLACE(REPLACE(COALESCE(vu.first_name, N''), N'ي', N'ی'), N'ى', N'ی'), N'ك', N'ک') LIKE @q
               OR REPLACE(REPLACE(REPLACE(COALESCE(vu.last_name, N''), N'ي', N'ی'), N'ى', N'ی'), N'ك', N'ک') LIKE @q
               OR CONCAT(
                    REPLACE(REPLACE(REPLACE(COALESCE(vu.first_name, N''), N'ي', N'ی'), N'ى', N'ی'), N'ك', N'ک'),
                    N' ',
                    REPLACE(REPLACE(REPLACE(COALESCE(vu.last_name, N''), N'ي', N'ی'), N'ى', N'ی'), N'ك', N'ک')
                  ) LIKE @q)"
        };

        string scope = "all";
        if (isSupervisor && !isAdmin)
        {
            int regionId = currentUser.Int("region_id");
            int provinceCode = currentUser.Int("ProvinceCode");
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
          OFFSET 0 ROWS FETCH NEXT {limit} ROWS ONLY";

        var normalizedQ = PersianText.NormalizeForSearch(q);
        var rows = (await conn.QueryListDict(sql, new { q = $"%{normalizedQ}%" })).AsList();

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
                    voted_at       = row.GetValueOrDefault("voted_at"),
                    voted_at_shamsi= row.GetValueOrDefault("voted_at_shamsi"),
                    vote_count     = rows.Count(x => x.Str("voter_national_id") == voterId && x.GetValueOrDefault("vote_id") != null)
                };
        }

        await conn.ExecuteAsync(
            "INSERT INTO logs (nationalId, action, description) VALUES (@nid,'جستجوی آرای کاربر',@desc)",
            new { nid = NationalId, desc = $"جستجوی آرای کاربران با عبارت {q}" });

        return Ok(new
        {
            status = true,
            data = new { scope, items = isAdmin ? rows : new List<IDictionary<string, object>>(), summary = summary.Values },
            message = "جستجوی آرای کاربران با موفقیت انجام شد."
        });
    }

    // Published final candidate results, grouped by each candidate's electoral region.
    [HttpGet("getFinalCandidateResults")]
    public async Task<IActionResult> GetFinalCandidateResults([FromQuery] int? province = null, [FromQuery] int? region = null, string? secondPassword = null)
    {
        await using var conn = _db.CreateConnection();
        var pending = await conn.ExecuteScalarAsync<int>(@"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.region rg
                LEFT JOIN dbo.final_results_approvals fra ON fra.region_id=rg.id
                WHERE rg.id % 100 <> 0
                  AND (@region IS NULL OR rg.id=@region)
                  AND (@region IS NOT NULL OR @province IS NULL OR rg.ProvinceCode=@province)
                  AND ISNULL(fra.is_active,0)=0
            ) THEN 1 ELSE 0 END", new { region, province });
        bool adminPreview = false;
        if (pending == 1)
        {
            // Approved regions remain publicly visible; pending regions are excluded
            // unless an administrator supplies the second password.
            adminPreview = await IsAdminWithSecondPassword(conn, GetSecondPassword(secondPassword))
                || await CanReviewEndedElectionResults(conn);
        }

        var results = await conn.QueryAsync<dynamic>(@"
            SELECT fi.id AS id, fi.id AS codeentekhabati,
                   u.first_name, u.last_name, u.org_position_desc, ud.user_photo,
                   u.region_id AS regionId, rg.name AS regname,
                   rg.ProvinceCode AS provinceCode, pr.name AS provinceName,
                   COUNT(v.id) AS vote_count
            FROM dbo.final_submissions fi
            JOIN dbo.users u ON u.national_id=fi.nationalId
            JOIN dbo.region rg ON rg.id=u.region_id
            LEFT JOIN dbo.region pr ON pr.id=rg.ProvinceCode*100
            LEFT JOIN dbo.user_documents ud ON ud.nationalId=fi.nationalId
            LEFT JOIN dbo.votes v ON v.candidate_id=fi.id
            WHERE fi.requestStatus='SUPERVISION_APPROVED'
              AND (@region IS NULL OR u.region_id=@region)
              AND (@region IS NOT NULL OR @province IS NULL OR rg.ProvinceCode=@province)
              AND (@adminPreview=1 OR EXISTS (
                  SELECT 1 FROM dbo.final_results_approvals fra
                  WHERE fra.region_id=u.region_id AND ISNULL(fra.is_active,0)=1))
            GROUP BY fi.id,u.first_name,u.last_name,u.org_position_desc,ud.user_photo,
                     u.region_id,rg.name,rg.ProvinceCode,pr.name
            ORDER BY rg.ProvinceCode,u.region_id,vote_count DESC",
            new { province, region, adminPreview });
        return Ok(new { status=true, data=results, previewRequired = pending == 1 && !adminPreview });
    }

    // GET /api/exportResults?region=&province=  - خروجی CSV نتایج برای ادمین
    [HttpGet("exportResults")]
    public async Task<IActionResult> ExportResults([FromQuery] int? region = null, [FromQuery] int? province = null)
    {
        await using var conn = _db.CreateConnection();

        var me = await conn.QueryRowDict(
            "SELECT roles, region_id FROM users WHERE national_id=@nid", new { nid = NationalId });
        if (me == null) return Unauthorized();

        var roles = me.Str("roles").ToUpperInvariant()
            .Split(new[] { ',', ';', ' ', '|' }, StringSplitOptions.RemoveEmptyEntries);
        bool admin = roles.Contains("ADMIN");
        bool committee = roles.Contains("EXECUTIVE") || roles.Contains("SUPERVISOR");
        if (!admin && (!committee || !await CanReviewEndedElectionResults(conn)))
            return StatusCode(403, new { status = false, message = "دریافت خروجی فقط برای مدیر یا اعضای اجرایی و نظارت پس از پایان انتخابات مجاز است." });

        int? allowedRegion = admin ? null : me.Int("region_id");
        if (!admin && region.HasValue && region.Value != allowedRegion)
            return StatusCode(403, new { status = false, message = "دسترسی به نتایج این منطقه مجاز نیست." });
        if (!admin) region = allowedRegion;

        var p = province.HasValue ? (object)province.Value : DBNull.Value;
        var r = region.HasValue   ? (object)region.Value   : DBNull.Value;

        var candidates = (await conn.QueryAsync<dynamic>(@"
            SELECT fi.id AS codeentekhabati,
                   u.national_id, u.first_name, u.last_name,
                   u.org_position_desc, u.gender, uc.education, uc.yearsOfService,
                   reg.name AS region_name, reg.ProvinceCode,
                   prov.Name AS province_name,
                   COUNT(CASE WHEN (@r IS NOT NULL AND voter.region_id = @r)
                                OR (@r IS NULL AND (@p IS NULL OR vr.ProvinceCode = @p))
                              THEN v.id ELSE NULL END) AS vote_count,
                   fi.requestStatus, fi.create_date
            FROM final_submissions fi
            JOIN users u ON u.national_id = fi.nationalId
            LEFT JOIN userscheck uc ON uc.national_id = u.national_id
            LEFT JOIN region reg ON reg.id = u.region_id
            LEFT JOIN region prov ON prov.id = (reg.ProvinceCode * 100)
            LEFT JOIN votes v ON v.candidate_id = fi.id
            LEFT JOIN users voter ON voter.national_id = v.national_id
            LEFT JOIN region vr ON vr.id = voter.region_id
            WHERE (@allowedRegion IS NULL OR u.region_id=@allowedRegion)
              AND (@r IS NULL OR u.region_id=@r)
              AND (@r IS NOT NULL OR @p IS NULL OR reg.ProvinceCode=@p)
              AND fi.requestStatus IN ('SUPERVISION_APPROVED','SUPERVISION_REJECTED','SUBMITTED')
            GROUP BY fi.id, u.national_id, u.first_name, u.last_name,
                     u.org_position_desc, u.gender, uc.education, uc.yearsOfService,
                     reg.name, reg.ProvinceCode, prov.Name, fi.requestStatus, fi.create_date
            ORDER BY vote_count DESC, u.last_name ASC",
            new { p, r, allowedRegion })).AsList();

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

    // GET /api/getVotingCandidates
    [HttpGet("getVotingCandidates")]
    public async Task<IActionResult> GetVotingCandidates()
    {
        await using var conn = _db.CreateConnection();
        var voter = await conn.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT u.region_id AS regionId, r.ProvinceCode AS provinceCode,
                   COALESCE(m.maxVotes, CASE WHEN COUNT(v.national_id) < 1000 THEN 0
                   ELSE CEILING(COUNT(v.national_id)/1000.0) END) AS allowedVotes
            FROM dbo.users u
            JOIN dbo.region r ON r.id=u.region_id
            LEFT JOIN dbo.maxvotes m ON m.region_id=r.id
            LEFT JOIN dbo.users v ON v.region_id=r.id
                AND COALESCE(NULLIF(v.roles,''),'VOTER')='VOTER'
            WHERE u.national_id=@nid
            GROUP BY u.region_id,r.ProvinceCode,m.maxVotes",
            new { nid=NationalId });
        if (voter == null)
            return BadRequest(new { status=false, message="منطقه کاربر مشخص نیست." });

        int regionId=Convert.ToInt32(voter.regionId);
        int provinceCode=Convert.ToInt32(voter.provinceCode);
        bool provinceWideVoting=Convert.ToInt32(voter.allowedVotes)<=0;
        var candidates=await conn.QueryAsync<dynamic>(@"
            SELECT f.id, f.id AS codeentekhabati, u.first_name, u.last_name, u.gender,
                   u.org_position_desc, u.region_id, r.name AS regname,
                   r.ProvinceCode AS provinceCode
            FROM dbo.final_submissions f
            JOIN dbo.users u ON u.national_id=f.nationalId
            JOIN dbo.region r ON r.id=u.region_id
            WHERE f.requestStatus='SUPERVISION_APPROVED'
              AND ((@wide=0 AND u.region_id=@rid)
                OR (@wide=1 AND r.ProvinceCode=@province))
            ORDER BY u.last_name,u.first_name",
            new { wide=provinceWideVoting, rid=regionId, province=provinceCode });
        return Ok(new { status=true, data=candidates, provinceWideVoting });
    }

    // GET /api/getAvailableVoteRegions
    // مناطق مجاز برای کاربرانی که منطقه خودشان ظرفیت رأی ندارد
    [HttpGet("getAvailableVoteRegions")]
    public async Task<IActionResult> GetAvailableVoteRegions()
    {
        await using var conn = _db.CreateConnection();

        var province = await conn.QueryFirstOrDefaultAsync<int?>(
            """
            SELECT r.ProvinceCode
            FROM users u
            INNER JOIN region r ON r.id = u.region_id
            WHERE u.national_id=@nid
            """,
            new { nid = NationalId });

        if (!province.HasValue)
            return BadRequest(new { status = false, message = "منطقه کاربر یافت نشد" });

        var rows = await conn.QueryAsync<dynamic>(
            """
            SELECT
                r.id,
                r.Name AS name,
                COALESCE(m.maxVotes, CEILING(CAST(COUNT(u.id) AS decimal(18,2)) / 1000)) AS allowedVotes,
                COUNT(u.id) AS voterCount
            FROM region r
            LEFT JOIN maxvotes m ON m.region_id = r.id
            LEFT JOIN users u ON u.region_id = r.id
                AND COALESCE(NULLIF(u.roles,''),'VOTER') = 'VOTER'
            WHERE r.ProvinceCode=@province
              AND r.id % 100 <> 0
            GROUP BY r.id, r.Name, m.maxVotes
            HAVING COALESCE(m.maxVotes, CEILING(CAST(COUNT(u.id) AS decimal(18,2)) / 1000)) > 0
            ORDER BY r.Name
            """,
            new { province });

        return Ok(new
        {
            status = true,
            regions = rows
        });
    }

    private static string CsvCell(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}

public record InsertVoteRequest(int[] candidateIds, string vote_token, int? regionId = null);


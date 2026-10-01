using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly JalaliService _jalali;
    private readonly BaleService _bale;

    public UserController(DatabaseService db, JalaliService jalali, BaleService bale)
    {
        _db = db;
        _jalali = jalali;
        _bale = bale;
    }

    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";
    private string UserRole => User.Claims.FirstOrDefault(c => c.Type == "roles")?.Value ?? "";

    private static readonly string[] ValidDegrees = {
        "لیسانس", "کارشناسی", "فوق لیسانس", "کارشناسی ارشد",
        "دکتری", "دکترا", "فوق دکتری", "فوق دکترا", "پست دکترا"
    };

    // GET /api/user-status
    [HttpGet("user-status")]
    public async Task<IActionResult> UserStatus()
    {
        await using var conn = _db.CreateConnection();

        bool inFund = false;
        float yearsOfService = 0f;
        string education = "";
        try
        {
            var check = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT TOP (1) national_id, yearsOfService, education FROM dbo.userscheck WHERE national_id=@nid",
                new { nid = NationalId });
            inFund = check != null;
            yearsOfService = check != null ? Convert.ToSingle(check.yearsOfService ?? 0) : 0f;
            education = ((string?)check?.education)?.Trim() ?? "";
        }
        catch { }

        bool hasMinYears = yearsOfService >= 1f;
        bool hasDegree   = ValidDegrees.Contains(education);

        string? reg = null;
        try
        {
            reg = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT TOP (1) nationalId FROM dbo.final_submissions WHERE nationalId=@nid",
                new { nid = NationalId });
        }
        catch { }

        return Ok(new
        {
            status = true,
            data = new
            {
                membershipActive  = inFund,
                membershipYears   = hasMinYears,
                yearsOfService    = yearsOfService,
                education         = education,
                degree            = hasDegree,
                alreadyRegistered = reg != null
            }
        });
    }

    // GET /api/getUsers?page=1&limit=50&search=
    [HttpGet("getUsers")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int limit = 50, [FromQuery] string? search = null)
    {
        await using var conn = _db.CreateConnection();

        var currentUser = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT TOP (1) u.roles, u.region_id, r.ProvinceCode
              FROM dbo.users u LEFT JOIN dbo.region r ON r.id=u.region_id
              WHERE u.national_id=@nid", new { nid = NationalId });

        if (currentUser == null) return Forbid();

        string currentRole = Convert.ToString(currentUser.roles) ?? "";
        int currentRegionId = Convert.ToInt32(currentUser.region_id ?? 0);
        int? currentProvinceCode = currentUser.ProvinceCode == null ? null : Convert.ToInt32(currentUser.ProvinceCode);

        bool isAdmin = currentRole == "ADMIN";
        bool isProvinceSupervisor = currentRole == "SUPERVISOR"
            && currentRegionId > 0 && currentRegionId % 100 == 0
            && currentProvinceCode.HasValue;

        if (!isAdmin && !isProvinceSupervisor)
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });

        page  = Math.Max(1, page);
        limit = Math.Clamp(limit, 10, 200);
        int offset = (page - 1) * limit;

        var conditions = new List<string>();
        if (isProvinceSupervisor)
            conditions.Add("r.ProvinceCode = @provinceCode");
        if (!string.IsNullOrWhiteSpace(search))
            conditions.Add(@"(u.national_id LIKE @search
                               OR REPLACE(REPLACE(REPLACE(COALESCE(u.first_name, N''), N'ي', N'ی'), N'ى', N'ی'), N'ك', N'ک') LIKE @search
                               OR REPLACE(REPLACE(REPLACE(COALESCE(u.last_name, N''), N'ي', N'ی'), N'ى', N'ی'), N'ك', N'ک') LIKE @search
                               OR CONVERT(nvarchar(50), u.personnel_code) LIKE @search
                               OR REPLACE(REPLACE(REPLACE(COALESCE(r.Name, N''), N'ي', N'ی'), N'ى', N'ی'), N'ك', N'ک') LIKE @search)");

        string where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";
        var queryParams = new
        {
            provinceCode = currentProvinceCode,
            search = string.IsNullOrWhiteSpace(search) ? null : $"%{PersianText.NormalizeForSearch(search)}%",
            offset,
            limit
        };

        string passColumns = isAdmin
            ? @", CASE WHEN u.roles='EXECUTIVE' AND (u.region_id % 100) != 0 THEN fra.EXECUTIVEPass ELSE NULL END AS executivePass,
                 CASE WHEN u.roles='SUPERVISOR' AND (u.region_id % 100) != 0 THEN fra.SUPERVISORPass ELSE NULL END AS supervisorPass"
            : "";
        string passJoin = isAdmin
            ? "LEFT JOIN dbo.final_results_approvals fra ON fra.region_id = u.region_id"
            : "";

        var total = await conn.QueryFirstOrDefaultAsync<int>(
            $"SELECT COUNT(*) FROM dbo.users u JOIN dbo.region r ON r.id=u.region_id {where}", queryParams);
        int pages = limit > 0 ? (int)Math.Ceiling(total / (double)limit) : 1;

        // شمارش واجدین رأی هر منطقه باید مستقل از pagination/search باشد.
        // کاربران با roles خالی/NULL مطابق منطق سامانه VOTER در نظر گرفته می‌شوند.
        var voterCountConditions = new List<string>
        {
            "COALESCE(NULLIF(LTRIM(RTRIM(u.roles)), N''), N'VOTER') = N'VOTER'"
        };
        if (isProvinceSupervisor)
            voterCountConditions.Add("r.ProvinceCode = @provinceCode");

        string voterCountWhere = "WHERE " + string.Join(" AND ", voterCountConditions);
        var voterCountRows = await conn.QueryAsync<dynamic>(
            $@"SELECT
                    u.region_id AS regionId,
                    COUNT(*) AS voterCount,
                    MAX(mv.maxVotes) AS maxVotes
               FROM dbo.users u
               JOIN dbo.region r ON r.id = u.region_id
               LEFT JOIN dbo.maxvotes mv ON mv.region_id = u.region_id
               {voterCountWhere}
               GROUP BY u.region_id",
            new { provinceCode = currentProvinceCode });

        // تعداد رای مجاز: اگر maxvotes مقدار داشته باشد همان مقدار،
        // در غیر این صورت به ازای هر 1000 واجد رأی یک رأی (گرد رو به بالا)
        // کمتر از 1000 نفر = صفر
        var voterCountByRegion = voterCountRows.ToDictionary(
            x => Convert.ToString(x.regionId) ?? "0",
            x => Convert.ToInt32(x.voterCount));

        var allowedVotesByRegion = voterCountRows.ToDictionary(
            x => Convert.ToString(x.regionId) ?? "0",
            x =>
            {
                int? maxVotes = x.maxVotes == null ? null : Convert.ToInt32(x.maxVotes);
                if (maxVotes.HasValue)
                    return maxVotes.Value;

                int count = Convert.ToInt32(x.voterCount);
                return count < 1000 ? 0 : (int)Math.Ceiling(count / 1000.0);
            });

        var sql = $@"SELECT u.id, u.national_id, u.first_name, u.last_name,
                    u.personnel_code, u.region_id, u.roles, u.created_at,
                    r.name AS regionName, r.ProvinceCode AS provinceCode,
                    p.Name AS provinceName,
                    uc.education, uc.yearsOfService{passColumns}
                FROM dbo.users u
                JOIN dbo.region r ON r.id=u.region_id
                LEFT JOIN dbo.region p ON p.id=(r.ProvinceCode * 100)
                LEFT JOIN dbo.userscheck uc ON uc.national_id=u.national_id
                {passJoin}
                {where}
                ORDER BY u.id DESC OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

        var rows = (await conn.QueryAsync<dynamic>(sql, queryParams)).AsList();
        var list = rows.Select(r =>
        {
            var d = (IDictionary<string, object>)r;
            if (d["created_at"] is DateTime dt)
                d["created_at"] = _jalali.FormatShort(dt);
            return d;
        });

        return Ok(new
        {
            status = true,
            data = list,
            meta = new { total, page, limit, pages, voterCountByRegion, allowedVotesByRegion }
        });
    }

    // POST /api/updateUser  {national_id, region_id, roles}
    [HttpPost("updateUser")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.national_id) || req.region_id <= 0
            || !new[] { "ADMIN", "SUPERVISOR", "EXECUTIVE", "CANDIDATE", "VOTER" }.Contains(req.roles))
            return BadRequest(new { status = false, message = "اطلاعات ارسالی معتبر نیست." });

        await using var conn = _db.CreateConnection();

        var currentUser = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT TOP (1) u.roles, u.region_id, r.ProvinceCode
              FROM dbo.users u LEFT JOIN dbo.region r ON r.id=u.region_id
              WHERE u.national_id=@nid", new { nid = NationalId });

        string currentRole = currentUser == null ? "" : (Convert.ToString(currentUser.roles) ?? "");
        int currentRegionId = currentUser == null ? 0 : Convert.ToInt32(currentUser.region_id ?? 0);
        int? currentProvinceCode = currentUser?.ProvinceCode == null ? null : Convert.ToInt32(currentUser.ProvinceCode);

        bool isAdmin = currentRole == "ADMIN";
        bool isProvinceSupervisor = currentRole == "SUPERVISOR" && currentRegionId > 0 && currentRegionId % 100 == 0;

        if (!isAdmin && !isProvinceSupervisor)
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });

        var newRegion = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT TOP (1) id, ProvinceCode, Name FROM dbo.region WHERE id=@id", new { id = req.region_id });
        if (newRegion == null)
            return BadRequest(new { status = false, message = "منطقه انتخاب شده معتبر نیست." });

        if (isProvinceSupervisor)
        {
            int provincecode = currentProvinceCode ?? 0;
            var target = await conn.QueryFirstOrDefaultAsync<dynamic>(
                @"SELECT TOP (1) u.national_id, r.ProvinceCode FROM dbo.users u
                  JOIN dbo.region r ON r.id=u.region_id WHERE u.national_id=@nid",
                new { nid = req.national_id });

            if (target == null
                || Convert.ToInt32(target.ProvinceCode) != provincecode
                || Convert.ToInt32(newRegion.ProvinceCode) != provincecode)
                return StatusCode(403, new { status = false, message = "امکان ویرایش کاربران خارج از استان شما وجود ندارد." });
        }

        await conn.ExecuteAsync(
            "UPDATE dbo.users SET region_id=@rid, regionName=@rname, roles=@roles WHERE national_id=@nid",
            new { rid = req.region_id, rname = Convert.ToString(newRegion.Name) ?? "", roles = req.roles, nid = req.national_id });

        return Ok(new { status = true, message = "ویرایش با موفقیت انجام شد.", data = true });
    }

    // GET /api/getEXECUTIVEList
    [HttpGet("getEXECUTIVEList")]
    public async Task<IActionResult> GetExecutiveList()
    {
        await using var conn = _db.CreateConnection();

        var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT TOP (1) roles, region_id FROM dbo.users WHERE national_id=@nid", new { nid = NationalId });

        var myListRole = me == null ? "" : (Convert.ToString(me.roles) ?? "");
        if (me == null || (myListRole != "EXECUTIVE" && myListRole != "SUPERVISOR"))
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });

        int regionId = Convert.ToInt32(me.region_id);

        var rows = (await conn.QueryAsync<dynamic>(
            @"SELECT f.id AS codeentekhabati, tracking_code, requestStatus, f.create_date,
                     u.id, u.national_Id, u.first_name, u.last_name, u.persian_birth_date,
                     u.personnel_code, u.gender, u.father_name, u.org_position_desc,
                     uc.yearsOfService, uc.education, u.user_type, u.region_id,
                     re.name AS regname, u.roles,
                     ud.user_photo, ud.education_doc, ud.employment_cert,
                     ud.soPishine_cert, ud.ravan_cert, ud.document_reviews,
                     ud.updated_at AS datepic,
                     ua.post_code, ua.address, f.reson
              FROM dbo.final_submissions f
              JOIN dbo.users u ON u.national_id=f.nationalId
              LEFT JOIN dbo.region re ON re.id=u.region_id
              JOIN dbo.user_documents ud ON ud.nationalId=f.nationalId
              LEFT JOIN dbo.user_addresses ua ON ua.user_id=u.id
              LEFT JOIN dbo.userscheck uc ON uc.national_id=u.national_id
              WHERE u.region_id=@rid ORDER BY create_date DESC",
            new { rid = regionId })).AsList();

        var list = rows.Select(r =>
        {
            var d = (IDictionary<string, object>)r;
            if (d["create_date"] is DateTime cd)
                d["create_datesh"] = _jalali.FormatShort(cd);
            if (d["datepic"] is DateTime dp)
                d["datepic"] = _jalali.FormatShort(dp);
            return d;
        });

        return Ok(new { status = true, data = list });
    }

    // POST /api/ChangeState  {national_Id, requestStatus, reason}
    [HttpPost("ChangeState")]
    public async Task<IActionResult> ChangeState([FromBody] ChangeStateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.national_Id) || string.IsNullOrWhiteSpace(req.requestStatus))
            return BadRequest(new { status = false, message = "پارامتر کد و وضعیت الزامی است." });

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        var me = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT TOP (1) roles FROM dbo.users WHERE national_id=@nid", new { nid = NationalId });
        string myRole = me == null ? "" : (Convert.ToString(me.roles) ?? "");

        if (myRole != "EXECUTIVE" && myRole != "SUPERVISOR")
            return StatusCode(403, new { status = false, message = "شما دسترسی لازم را ندارید" });

        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            await conn.ExecuteAsync(
                @"UPDATE dbo.final_submissions SET requestStatus=@s, reson=@r, edited_at=GETDATE()
                  WHERE nationalId=@nid",
                new { s = req.requestStatus, r = req.reason ?? "", nid = req.national_Id }, tx);

            if (myRole == "SUPERVISOR" && (req.requestStatus == "SUPERVISION_APPROVED" || req.requestStatus == "SUPERVISION_REJECTED"))
            {
                await conn.ExecuteAsync(
                    @"UPDATE dbo.user_documents
                      SET supervision_status=@s, supervision_reason=@r,
                          supervision_reviewed_by=@by, supervision_reviewed_at=GETDATE()
                      WHERE nationalId=@nid",
                    new { s = req.requestStatus, r = req.reason ?? "", by = NationalId, nid = req.national_Id }, tx);
            }

            await conn.ExecuteAsync(
                "INSERT INTO dbo.logs (nationalId, action, description) VALUES (@nid,'تغییر وضعیت',@desc)",
                new { nid = NationalId, desc = $"تغییر کدملی {req.national_Id} به {req.requestStatus}" }, tx);

            await tx.CommitAsync();

            // پیام بله به کاندید
            var statusMessages = new Dictionary<string, string>
            {
                ["SUPERVISION_APPROVED"]  = "صلاحیت شما توسط ناظر تأیید شد.",
                ["SUPERVISION_REJECTED"]  = "صلاحیت شما توسط ناظر رد شد.",
                ["EXECUTIVE_APPROVED"]    = "مدارک شما توسط اجرایی تأیید شد.",
                ["EXECUTIVE_REJECTED"]    = "مدارک شما توسط اجرایی رد شد.",
            };
            if (statusMessages.TryGetValue(req.requestStatus, out var smsText))
            {
                var mobile = await conn.QueryFirstOrDefaultAsync<string>(
                    "SELECT TOP (1) mobile FROM dbo.users WHERE national_id=@nid", new { nid = req.national_Id });
                if (!string.IsNullOrWhiteSpace(mobile))
                    _bale.SendAsync(mobile, $"سامانه انتخابات: {smsText}");
            }

            return Ok(new
            {
                status = true,
                message = "با موفقیت انجام شد.",
                data = new { nationalId = req.national_Id, requestStatus = req.requestStatus }
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}

public record UpdateUserRequest(string national_id, int region_id, string roles);
public record ChangeStateRequest(string national_Id, string requestStatus, string? reason);

using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class RegionMappingController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly JalaliService _jalali;
    public RegionMappingController(DatabaseService db, JalaliService jalali) { _db = db; _jalali = jalali; }
    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";

    private const string SetupSql = @"
IF OBJECT_ID(N'dbo.election_region_mappings', N'U') IS NULL
CREATE TABLE dbo.election_region_mappings (
    source_region_id INT NOT NULL PRIMARY KEY,
    target_region_id INT NOT NULL,
    supervisor_national_id NVARCHAR(20) NOT NULL,
    mapped_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CK_election_region_mapping_distinct CHECK (source_region_id <> target_region_id)
);";

    private async Task<(bool Allowed, int Province)> Supervisor(System.Data.IDbConnection conn)
    {
        var user = await conn.QueryFirstOrDefaultAsync<dynamic>(@"
SELECT u.roles, r.ProvinceCode AS province FROM dbo.users u
JOIN dbo.region r ON r.id=u.region_id WHERE u.national_id=@nid", new { nid = NationalId });
        if (user == null) return (false, 0);
        var roles = Convert.ToString(user.roles)?.ToUpperInvariant()
            .Split(new[] { ',', ';', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        // Provincial supervisor must belong to the province-level region, not a local supervisor.
        int province = Convert.ToInt32(user.province);
        int ownRegion = await conn.ExecuteScalarAsync<int>(
            "SELECT region_id FROM dbo.users WHERE national_id=@nid", new { nid = NationalId });
        return (roles.Contains("SUPERVISOR") && ownRegion == province * 100, province);
    }

    private async Task<bool> MappingWindowOpen(System.Data.IDbConnection conn)
    {
        var rows = (await conn.QueryAsync<dynamic>(@"
SELECT event_key, start_date, end_date FROM dbo.election_schedule_events
WHERE event_key IN ('registration','candidate_registration','candidateRegistration','voting')")).ToList();
        var registration = rows.FirstOrDefault(x => Convert.ToString(x.event_key) != "voting");
        var voting = rows.FirstOrDefault(x => Convert.ToString(x.event_key) == "voting");
        if (registration == null || voting == null) return false;
        var registrationEnd = _jalali.NormalizeToGregorian(registration.end_date);
        var votingStart = _jalali.NormalizeToGregorian(voting.start_date);
        return registrationEnd.HasValue && votingStart.HasValue &&
            DateTime.Now > registrationEnd.Value && DateTime.Now < votingStart.Value;
    }

    [HttpGet("provincialRegionMappings")]
    public async Task<IActionResult> List()
    {
        await using var conn = _db.CreateConnection();
        var (allowed, province) = await Supervisor(conn);
        if (!allowed) return Forbid();
        await conn.ExecuteAsync(SetupSql);
        var rows = await conn.QueryAsync<dynamic>(@"
WITH members AS (
 SELECT r.id, r.name, r.ProvinceCode,
 COUNT(CASE WHEN COALESCE(NULLIF(LTRIM(RTRIM(u.roles)),''),'VOTER')='VOTER' THEN 1 END) AS memberCount
 FROM dbo.region r LEFT JOIN dbo.users u ON u.region_id=r.id
 WHERE r.ProvinceCode=@province AND r.id % 100 <> 0
 GROUP BY r.id,r.name,r.ProvinceCode
), candidates AS (
 SELECT u.region_id, COUNT(*) AS candidateCount FROM dbo.final_submissions f
 JOIN dbo.users u ON u.national_id=f.nationalId
 WHERE f.requestStatus='SUPERVISION_APPROVED' GROUP BY u.region_id
)
SELECT m.id AS regionId,m.name AS regionName,m.memberCount,
 CASE WHEN mv.maxVotes IS NOT NULL THEN mv.maxVotes
      WHEN m.memberCount=0 THEN 0 ELSE CEILING(m.memberCount/2000.0) END AS quota,
 COALESCE(c.candidateCount,0) AS candidateCount,
 map.target_region_id AS targetRegionId,
 dest.name AS targetRegionName
FROM members m
LEFT JOIN (SELECT region_id,MAX(maxVotes) AS maxVotes FROM dbo.maxvotes GROUP BY region_id) mv ON mv.region_id=m.id
LEFT JOIN candidates c ON c.region_id=m.id
LEFT JOIN dbo.election_region_mappings map ON map.source_region_id=m.id
LEFT JOIN dbo.region dest ON dest.id=map.target_region_id
ORDER BY m.name", new { province });
        return Ok(new { status = true, canMap = await MappingWindowOpen(conn), data = rows });
    }

    public record MappingRequest(int SourceRegionId, int TargetRegionId);

    [HttpPost("setProvincialRegionMapping")]
    public async Task<IActionResult> Set([FromBody] MappingRequest request)
    {
        await using var conn = _db.CreateConnection();
        var (allowed, province) = await Supervisor(conn);
        if (!allowed) return Forbid();
        if (!await MappingWindowOpen(conn))
            return BadRequest(new { status = false, message = "نگاشت فقط پس از پایان ثبت‌نام و پیش از شروع رأی‌گیری مجاز است." });
        await conn.ExecuteAsync(SetupSql);
        var regions = (await conn.QueryAsync<dynamic>(@"
SELECT id,ProvinceCode FROM dbo.region WHERE id IN (@source,@target)",
            new { source = request.SourceRegionId, target = request.TargetRegionId })).ToList();
        if (request.SourceRegionId == request.TargetRegionId || regions.Count != 2 ||
            regions.Any(r => Convert.ToInt32(r.ProvinceCode) != province || Convert.ToInt32(r.id) % 100 == 0))
            return BadRequest(new { status = false, message = "هر دو منطقه باید متفاوت و در استان ناظر باشند." });
        var stats = await conn.QueryFirstAsync<dynamic>(@"
SELECT (SELECT COUNT(*) FROM dbo.users WHERE region_id=@source AND COALESCE(NULLIF(LTRIM(RTRIM(roles)),''),'VOTER')='VOTER') AS members,
(SELECT COUNT(*) FROM dbo.final_submissions f JOIN dbo.users u ON u.national_id=f.nationalId
 WHERE u.region_id=@source AND f.requestStatus='SUPERVISION_APPROVED') AS candidates,
(SELECT MAX(maxVotes) FROM dbo.maxvotes WHERE region_id=@source) AS manualQuota",
            new { source = request.SourceRegionId });
        int members = Convert.ToInt32(stats.members);
        int quota = stats.manualQuota == null ? (int)Math.Ceiling(members / 2000.0) : Convert.ToInt32(stats.manualQuota);
        if (Convert.ToInt32(stats.candidates) >= 2 * quota)
            return BadRequest(new { status = false, message = "منطقه مبدأ حدنصاب نامزدها را دارد." });
        if (await conn.ExecuteScalarAsync<int>(@"
SELECT COUNT(*) FROM dbo.election_region_mappings
WHERE source_region_id=@target OR target_region_id=@source",
            new { source = request.SourceRegionId, target = request.TargetRegionId }) > 0)
            return BadRequest(new { status = false, message = "نگاشت زنجیره‌ای یا چرخه‌ای مجاز نیست." });
        await conn.ExecuteAsync(@"
UPDATE dbo.election_region_mappings SET target_region_id=@target,
supervisor_national_id=@nid,mapped_at=GETDATE() WHERE source_region_id=@source;
IF @@ROWCOUNT=0 INSERT INTO dbo.election_region_mappings
(source_region_id,target_region_id,supervisor_national_id)
VALUES (@source,@target,@nid);",
            new { source = request.SourceRegionId, target = request.TargetRegionId, nid = NationalId });
        return Ok(new { status = true });
    }
}

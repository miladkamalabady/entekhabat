using Dapper;
using System.Data;

namespace EntekhabatApi.Services;

/// <summary>Single authoritative resolver for a voter's effective district and seat limit.</summary>
public static class EffectiveVotingRegion
{
    public sealed record Result(int HomeRegionId, int RegionId, int ProvinceCode, int AllowedVotes, bool IsMapped);

    public static async Task<Result?> Resolve(IDbConnection conn, string nationalId, IDbTransaction? tx = null)
    {
        var home = await conn.QueryFirstOrDefaultAsync<(int RegionId, int ProvinceCode)>(
            @"SELECT u.region_id AS RegionId, r.ProvinceCode
              FROM dbo.users u JOIN dbo.region r ON r.id=u.region_id
              WHERE u.national_id=@nationalId",
            new { nationalId }, tx);
        if (home.RegionId == 0) return null;

        var target = await conn.QueryFirstOrDefaultAsync<int?>(@"
IF OBJECT_ID(N'dbo.election_region_mappings',N'U') IS NOT NULL
 SELECT m.target_region_id
 FROM dbo.election_region_mappings m
 JOIN dbo.region source ON source.id=m.source_region_id
 JOIN dbo.region destination ON destination.id=m.target_region_id
 WHERE m.source_region_id=@home AND source.ProvinceCode=destination.ProvinceCode
   AND source.ProvinceCode=@province;",
            new { home = home.RegionId, province = home.ProvinceCode }, tx);

        int effective = target ?? home.RegionId;
        int allowed = await conn.ExecuteScalarAsync<int>(@"
SELECT CAST(COALESCE(
  (SELECT MAX(maxVotes) FROM dbo.maxvotes WHERE region_id=@region),
  CEILING(COUNT(CASE WHEN COALESCE(NULLIF(LTRIM(RTRIM(roles)),''),'VOTER')='VOTER'
                      THEN 1 END)/2000.0)
) AS INT)
FROM dbo.users WHERE region_id=@region",
            new { region = effective }, tx);

        return new Result(home.RegionId, effective, home.ProvinceCode, allowed, target.HasValue);
    }
}

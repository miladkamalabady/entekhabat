using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ElectionMinutesController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly JalaliService _jalali;
    private readonly IWebHostEnvironment _environment;
    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";
    public ElectionMinutesController(DatabaseService db, JalaliService jalali, IWebHostEnvironment environment)
    { _db = db; _jalali = jalali; _environment = environment; }

    private const string TableSql = @"
IF OBJECT_ID(N'dbo.election_signed_minutes',N'U') IS NULL
CREATE TABLE dbo.election_signed_minutes(
 id INT IDENTITY(1,1) PRIMARY KEY,
 region_id INT NOT NULL,
 role NVARCHAR(30) NOT NULL,
 uploaded_by NVARCHAR(20) NOT NULL,
 file_path NVARCHAR(500) NOT NULL,
 original_name NVARCHAR(255) NOT NULL,
 uploaded_at DATETIME2 NOT NULL DEFAULT GETDATE()
);";

    private async Task<(bool Allowed, int Region, string Role)> AuthorizedRegion(System.Data.IDbConnection conn)
    {
        var u = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT roles,region_id FROM dbo.users WHERE national_id=@nid",new { nid=NationalId });
        if(u==null) return (false,0,"");
        var roles=(Convert.ToString(u.roles)??"").ToUpperInvariant()
            .Split(new[]{',',';','|',' '},StringSplitOptions.RemoveEmptyEntries);
        var role=roles.Contains("SUPERVISOR") ? "SUPERVISOR" : roles.Contains("EXECUTIVE") ? "EXECUTIVE" : "";
        return (!string.IsNullOrEmpty(role),Convert.ToInt32(u.region_id),role);
    }

    private async Task<bool> ElectionEnded(System.Data.IDbConnection conn)
    {
        var date=await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT TOP(1) end_date FROM dbo.election_schedule_events WHERE event_key='voting'");
        if (date == null) return false;
        DateTime? votingEnd = _jalali.NormalizeToGregorian((object?)date.end_date);
        return votingEnd.HasValue && DateTime.Now > votingEnd.Value;
    }

    [HttpGet("regionalElectionMinutes")]
    public async Task<IActionResult> Download()
    {
        await using var conn=_db.CreateConnection();
        var access=await AuthorizedRegion(conn);
        if(!access.Allowed) return Forbid();
        if(!await ElectionEnded(conn)) return BadRequest(new {status=false,message="صورتجلسه پس از پایان رأی‌گیری در دسترس است."});
        var region=await conn.QueryFirstOrDefaultAsync<string>(
            "SELECT name FROM dbo.region WHERE id=@rid",new {rid=access.Region});
        if(region==null) return NotFound();
        var stats=await conn.QueryFirstAsync<dynamic>(@"
SELECT (SELECT COUNT(*) FROM dbo.users WHERE region_id=@rid
AND COALESCE(NULLIF(LTRIM(RTRIM(roles)),''),'VOTER')='VOTER') AS eligible,
(SELECT COUNT(*) FROM dbo.election_participants p JOIN dbo.users u ON u.national_id=p.national_id
WHERE u.region_id=@rid) AS participants,
(SELECT COUNT(*) FROM dbo.votes v JOIN dbo.users u ON u.national_id=v.national_id
WHERE u.region_id=@rid) AS votes",new {rid=access.Region});
        // Votes received by candidates in this region, including voters from merged source regions.
        var candidates=(await conn.QueryAsync<dynamic>(@"
SELECT f.id AS code, u.first_name, u.last_name, COUNT(v.id) AS votes
FROM dbo.final_submissions f
JOIN dbo.users u ON u.national_id=f.nationalId
LEFT JOIN dbo.votes v ON v.candidate_id=f.id
WHERE u.region_id=@rid AND f.requestStatus='SUPERVISION_APPROVED'
GROUP BY f.id,u.first_name,u.last_name
ORDER BY COUNT(v.id) DESC,u.last_name,u.first_name",new {rid=access.Region})).ToList();
        QuestPDF.Settings.License=LicenseType.Community;
        var document=Document.Create(container=>container.Page(page=>
        {
            page.Size(PageSizes.A4);
            page.Margin(35);
            page.DefaultTextStyle(x=>x.FontSize(11));
            page.Content().Column(col=>
            {
                col.Spacing(12);
                col.Item().AlignCenter().Text("صورتجلسه آمار و نتایج انتخابات").Bold().FontSize(17);
                col.Item().AlignRight().Text($"منطقه: {region}  |  کد منطقه: {access.Region}");
                col.Item().AlignRight().Text($"تاریخ صدور: {DateTime.Now:yyyy/MM/dd HH:mm}");
                col.Item().AlignRight().Text($"تعداد واجدان شرایط منطقه: {stats.eligible}    |    شرکت‌کنندگان منطقه: {stats.participants}    |    آرای ثبت‌شده توسط اعضای منطقه: {stats.votes}");
                col.Item().AlignRight().Text("آرای نامزدهای این حوزه (شامل آرای رأی‌دهندگان مناطق نگاشت‌شده):").Bold();
                col.Item().Table(table=>
                {
                    table.ColumnsDefinition(columns=>{columns.ConstantColumn(45);columns.RelativeColumn(3);columns.RelativeColumn(1);});
                    table.Header(header=>
                    {
                        header.Cell().Text("ردیف").Bold();
                        header.Cell().Text("نام نامزد").Bold();
                        header.Cell().Text("آرا").Bold();
                    });
                    int n=0;
                    foreach(var candidate in candidates)
                    {
                        table.Cell().Text((++n).ToString());
                        table.Cell().Text($"{candidate.first_name} {candidate.last_name}");
                        table.Cell().Text(Convert.ToString(candidate.votes)??"0");
                    }
                });
                col.Item().PaddingTop(18).AlignRight().Text("این صورتجلسه را چاپ کرده، به امضای اعضای اجرایی و نظارت برسانید و سپس نسخه امضاشده PDF را در سامانه بارگذاری نمایید.").Bold();
                col.Item().PaddingTop(25).Row(row=>
                {
                    row.RelativeItem().Column(x=>{x.Item().Text("نام و امضای مسئول اجرایی");x.Item().PaddingTop(45).LineHorizontal(1);});
                    row.ConstantItem(20);
                    row.RelativeItem().Column(x=>{x.Item().Text("نام و امضای ناظر");x.Item().PaddingTop(45).LineHorizontal(1);});
                });
            });
        }));
        var bytes=document.GeneratePdf();
        return File(bytes,"application/pdf",$"election-minutes-{access.Region}.pdf");
    }

    [HttpPost("uploadSignedElectionMinutes")]
    [RequestSizeLimit(20*1024*1024)]
    public async Task<IActionResult> Upload([FromForm] IFormFile? document)
    {
        await using var conn=_db.CreateConnection();
        var access=await AuthorizedRegion(conn);
        if(!access.Allowed) return Forbid();
        if(!await ElectionEnded(conn)) return BadRequest(new {status=false,message="بارگذاری فقط پس از پایان انتخابات مجاز است."});
        if(document==null || document.Length==0 || document.Length>20*1024*1024 ||
           !string.Equals(Path.GetExtension(document.FileName),".pdf",StringComparison.OrdinalIgnoreCase))
            return BadRequest(new {status=false,message="فایل PDF حداکثر ۲۰ مگابایت الزامی است."});
        await using(var check=document.OpenReadStream())
        {
            var magic=new byte[5];
            if(await check.ReadAsync(magic)!=5 || System.Text.Encoding.ASCII.GetString(magic)!="%PDF-")
                return BadRequest(new {status=false,message="محتوای فایل PDF معتبر نیست."});
        }
        var folder=Path.Combine(_environment.ContentRootPath,"wwwroot","uploads","signed-election-minutes");
        Directory.CreateDirectory(folder);
        var filename=Guid.NewGuid().ToString("N")+".pdf";
        var fullPath=Path.Combine(folder,filename);
        await using(var output=System.IO.File.Create(fullPath)) await document.CopyToAsync(output);
        try
        {
            await conn.ExecuteAsync(TableSql);
            await conn.ExecuteAsync(@"INSERT INTO dbo.election_signed_minutes
(region_id,role,uploaded_by,file_path,original_name)
VALUES(@rid,@role,@nid,@path,@name)",new {
rid=access.Region,role=access.Role,nid=NationalId,
path="uploads/signed-election-minutes/"+filename,
name=Path.GetFileName(document.FileName)});
            return Ok(new {status=true,message="نسخه امضاشده صورتجلسه ثبت شد."});
        }
        catch {System.IO.File.Delete(fullPath);throw;}
    }
}

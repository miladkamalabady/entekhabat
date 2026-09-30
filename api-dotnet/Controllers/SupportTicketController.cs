using System.Text.Json;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class SupportTicketController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly JalaliService _jalali;

    public SupportTicketController(DatabaseService db, JalaliService jalali)
    {
        _db = db;
        _jalali = jalali;
    }

    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";

    private async Task EnsureTables(Microsoft.Data.SqlClient.SqlConnection conn)
    {
        await conn.ExecuteAsync(@"IF OBJECT_ID(N'dbo.support_tickets', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.support_tickets (id INT IDENTITY(1,1) PRIMARY KEY, ticket_code NVARCHAR(30) NOT NULL UNIQUE, requester_national_id NVARCHAR(20) NOT NULL, requester_name NVARCHAR(120) NULL, requester_region_id INT NULL, requester_region_name NVARCHAR(120) NULL, requester_province_code INT NULL, target_role NVARCHAR(20) NOT NULL DEFAULT N'EXECUTIVE', support_level NVARCHAR(20) NOT NULL DEFAULT N'region', subject NVARCHAR(255) NOT NULL, category NVARCHAR(60) NOT NULL, priority NVARCHAR(20) NOT NULL DEFAULT N'medium', status NVARCHAR(20) NOT NULL DEFAULT N'open', created_at DATETIME2 NOT NULL DEFAULT GETDATE(), updated_at DATETIME2 NOT NULL DEFAULT GETDATE(), closed_at DATETIME2 NULL, closed_by NVARCHAR(20) NULL);
END;
IF OBJECT_ID(N'dbo.support_ticket_messages', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.support_ticket_messages (id INT IDENTITY(1,1) PRIMARY KEY, ticket_id INT NOT NULL, sender_national_id NVARCHAR(20) NOT NULL, sender_name NVARCHAR(120) NULL, sender_role NVARCHAR(20) NOT NULL DEFAULT N'USER', message NVARCHAR(MAX) NOT NULL, attachments NVARCHAR(MAX) NULL, created_at DATETIME2 NOT NULL DEFAULT GETDATE());
END");
    }

    private static string NormalizeRole(string role)
    {
        role = role.ToUpper().Trim();
        return role is "ADMIN" or "SUPERVISOR" or "EXECUTIVE" ? role : "USER";
    }

    private static string GetSupportLevel(string role, int regionId)
    {
        if (role == "ADMIN") return "headquarters";
        if (regionId.ToString().EndsWith("00")) return "province";
        return "region";
    }

    private static bool CanAccess(dynamic ticket, dynamic user)
    {
        string role = NormalizeRole((string)(user.roles ?? ""));
        string nid = (string)(user.national_id ?? "");

        if ((string)ticket.requester_national_id == nid) return true;
        if (role == "ADMIN") return true;
        if (role != "SUPERVISOR" && role != "EXECUTIVE") return false;

        string userRegion = Convert.ToString(user.region_id ?? 0) ?? "0";
        string ticketRegion = Convert.ToString(ticket.requester_region_id ?? 0) ?? "0";
        int userProvinceCode = Convert.ToInt32(user.provinceCode ?? 0);
        int ticketProvinceCode = Convert.ToInt32(ticket.requester_province_code ?? 0);

        bool sameRegion = userRegion.EndsWith("00")
            ? userProvinceCode > 0 && ticketProvinceCode == userProvinceCode
            : ticketRegion != "0" && ticketRegion == userRegion;

        if (!sameRegion) return false;

        // SUPERVISOR می‌تواند به تیکت‌های هدایت‌شده به خودش پاسخ دهد
        // EXECUTIVE فقط به تیکت‌های خودش
        if (role == "SUPERVISOR") return (string)ticket.target_role == "SUPERVISOR";
        return (string)ticket.target_role == role;
    }

    private static string BuildWhere(dynamic user)
    {
        string role = NormalizeRole((string)(user.roles ?? ""));
        string nid = (string)(user.national_id ?? "");
        string own = $"t.requester_national_id='{nid}'";

        if (role == "ADMIN") return "1=1";
        if (role != "SUPERVISOR" && role != "EXECUTIVE") return own;

        int regionId = Convert.ToInt32(user.region_id ?? 0);
        string staff;
        if (regionId > 0 && regionId.ToString().EndsWith("00"))
        {
            int pc = Convert.ToInt32(user.provinceCode ?? 0);
            // SUPERVISOR: همه تیکت‌های استان (غیر از ADMIN) — نقش نظارتی
            // EXECUTIVE: فقط تیکت‌های هدایت‌شده به خودش
            staff = role == "SUPERVISOR"
                ? $"(t.requester_province_code={pc} AND t.target_role!='ADMIN')"
                : $"(t.target_role='{role}' AND t.requester_province_code={pc})";
        }
        else
        {
            // SUPERVISOR: همه تیکت‌های منطقه (غیر از ADMIN)
            // EXECUTIVE: فقط تیکت‌های هدایت‌شده به خودش
            staff = role == "SUPERVISOR"
                ? $"(t.requester_region_id={regionId} AND t.target_role!='ADMIN')"
                : $"(t.target_role='{role}' AND t.requester_region_id={regionId})";
        }

        return $"({own} OR {staff})";
    }

    private static string RoleLabel(string role) => role switch
    {
        "ADMIN" => "ستاد/ادمین", "SUPERVISOR" => "نظارت",
        "EXECUTIVE" => "اجرایی", _ => "کاربر"
    };
    private static string LevelLabel(string level) => level switch
    {
        "headquarters" => "ستاد", "province" => "استان", _ => "منطقه"
    };

    // GET /api/getSupportTickets?status=
    [HttpGet("getSupportTickets")]
    public async Task<IActionResult> GetSupportTickets([FromQuery] string? status)
    {
        await using var conn = _db.CreateConnection();
        await EnsureTables(conn);

        var user = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT u.national_id, u.first_name, u.last_name, u.roles, u.region_id,
                     r.Name AS regionName, r.ProvinceCode AS provinceCode
              FROM users u LEFT JOIN region r ON r.id=u.region_id WHERE u.national_id=@nid",
            new { nid = NationalId });
        if (user == null) return NotFound(new { status = false, message = "کاربر یافت نشد" });

        string where = BuildWhere(user);
        if (!string.IsNullOrWhiteSpace(status) && new[] { "open", "closed", "pending" }.Contains(status))
            where += $" AND t.status='{status}'";

        var tickets = (await conn.QueryAsync<dynamic>(
            $"SELECT t.* FROM support_tickets t WHERE {where} ORDER BY t.updated_at DESC, t.id DESC OFFSET 0 ROWS FETCH NEXT 300 ROWS ONLY")).AsList();

        var ids = tickets.Select(t => (int)t.id).ToList();
        var allMessages = new List<dynamic>();
        if (ids.Any())
        {
            allMessages = (await conn.QueryAsync<dynamic>(
                $"SELECT * FROM support_ticket_messages WHERE ticket_id IN ({string.Join(",", ids)}) ORDER BY id ASC")).AsList();
        }

        var ticketMap = new Dictionary<int, IDictionary<string, object>>();
        foreach (var t in tickets)
        {
            var d = (IDictionary<string, object>)t;
            int tid = (int)d["id"];
            string tRole = NormalizeRole((string)(user.roles ?? ""));
            string userLevel = GetSupportLevel(tRole, (int)(user.region_id ?? 0));

            if (d["created_at"] is DateTime ca) d["date"] = _jalali.Format(ca, "Y/m/d");
            if (d["updated_at"] is DateTime ua) d["lastReply"] = _jalali.Format(ua, "H:i Y/m/d");
            d["conversation"] = new List<object>();
            d["canReply"] = CanAccess(t, user);
            d["targetRoleLabel"] = RoleLabel((string)(d["target_role"] ?? ""));
            d["supportLevelLabel"] = LevelLabel((string)(d["support_level"] ?? ""));
            ticketMap[tid] = d;
        }

        foreach (var msg in allMessages)
        {
            var md = (IDictionary<string, object>)msg;
            int tid = (int)md["ticket_id"];
            if (!ticketMap.ContainsKey(tid)) continue;

            string senderNid = (string)(md["sender_national_id"] ?? "");
            md["text"] = md["message"];
            md["sender"] = senderNid == NationalId ? "user" : "support";
            md["senderLabel"] = (string)(md["sender_role"] ?? "") == "USER" ? "کاربر" : RoleLabel((string)(md["sender_role"] ?? ""));
            if (md["created_at"] is DateTime mt) md["time"] = _jalali.Format(mt, "H:i Y/m/d");
            if (md["attachments"] is string att && !string.IsNullOrWhiteSpace(att))
                try { md["attachments"] = JsonSerializer.Deserialize<object>(att); } catch { }

            ((List<object>)ticketMap[tid]["conversation"]!).Add(md);
        }

        var list = ticketMap.Values.ToList();
        var stats = new { total = list.Count, open = list.Count(t => (string)(t["status"] ?? "") == "open"), closed = list.Count(t => (string)(t["status"] ?? "") == "closed"), pending = list.Count(t => (string)(t["status"] ?? "") == "pending") };
        string userNormRole = NormalizeRole((string)(user.roles ?? ""));

        return Ok(new
        {
            status = true,
            data = list,
            stats,
            scope = new
            {
                role = userNormRole,
                roleLabel = RoleLabel(userNormRole),
                level = GetSupportLevel(userNormRole, (int)(user.region_id ?? 0)),
                levelLabel = LevelLabel(GetSupportLevel(userNormRole, (int)(user.region_id ?? 0))),
                regionId = user.region_id,
                regionName = user.regionName,
                provinceCode = user.provinceCode
            }
        });
    }

    // POST /api/saveSupportTicket  {subject, category, priority, description, targetRole}
    [HttpPost("saveSupportTicket")]
    public async Task<IActionResult> SaveSupportTicket([FromBody] SaveTicketRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.subject) || string.IsNullOrWhiteSpace(req.category) || string.IsNullOrWhiteSpace(req.description))
            return BadRequest(new { status = false, message = "موضوع، دسته‌بندی و شرح درخواست الزامی است." });

        string priority = new[] { "low", "medium", "high", "urgent" }.Contains(req.priority ?? "") ? req.priority! : "medium";
        string targetRole = NormalizeRole(req.targetRole ?? "EXECUTIVE");
        if (targetRole == "USER") targetRole = "EXECUTIVE";

        await using var conn = _db.CreateConnection();
        await EnsureTables(conn);

        var user = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT u.national_id, u.first_name, u.last_name, u.roles, u.region_id,
                     r.Name AS regionName, r.ProvinceCode AS provinceCode
              FROM users u LEFT JOIN region r ON r.id=u.region_id WHERE u.national_id=@nid",
            new { nid = NationalId });
        if (user == null) return NotFound(new { status = false, message = "کاربر یافت نشد" });

        int regionId = (int)(user.region_id ?? 0);
        int provinceCode = (int)(user.provinceCode ?? 0);
        string supportLevel = targetRole == "ADMIN" ? "headquarters" : GetSupportLevel(NormalizeRole((string)(user.roles ?? "")), regionId);
        string requesterName = $"{user.first_name} {user.last_name}".Trim();
        string ticketCode = "T" + DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture) + Random.Shared.Next(100, 999);

        int ticketId = await conn.QuerySingleAsync<int>(
            @"INSERT INTO dbo.support_tickets (ticket_code, requester_national_id, requester_name, requester_region_id, requester_region_name, requester_province_code, target_role, support_level, subject, category, priority, status)
              OUTPUT INSERTED.id
              VALUES (@code,@nid,@name,@rid,@rname,@pc,@trole,@level,@subj,@cat,@pri,'open')",
            new { code = ticketCode, nid = NationalId, name = requesterName, rid = regionId, rname = (string)(user.regionName ?? ""), pc = provinceCode, trole = targetRole, level = supportLevel, subj = req.subject, cat = req.category, pri = priority });

        await conn.ExecuteAsync(
            "INSERT INTO support_ticket_messages (ticket_id, sender_national_id, sender_name, sender_role, message, attachments) VALUES (@tid,@nid,@name,'USER',@msg,'[]')",
            new { tid = ticketId, nid = NationalId, name = requesterName, msg = req.description });

        return Ok(new { status = true, message = "تیکت با موفقیت ثبت شد.", ticketId, ticketCode });
    }

    // POST /api/replySupportTicket  {ticketId, message, closeTicket}
    [HttpPost("replySupportTicket")]
    public async Task<IActionResult> ReplySupportTicket([FromBody] ReplyTicketRequest req)
    {
        if (req.ticketId <= 0 || (string.IsNullOrWhiteSpace(req.message) && !req.closeTicket))
            return BadRequest(new { status = false, message = "شناسه تیکت یا متن پاسخ نامعتبر است." });

        await using var conn = _db.CreateConnection();
        await EnsureTables(conn);

        var user = await conn.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT u.national_id, u.first_name, u.last_name, u.roles, u.region_id,
                     r.Name AS regionName, r.ProvinceCode AS provinceCode
              FROM users u LEFT JOIN region r ON r.id=u.region_id WHERE u.national_id=@nid",
            new { nid = NationalId });
        if (user == null) return NotFound(new { status = false, message = "کاربر یافت نشد" });

        var ticket = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM support_tickets WHERE id=@id", new { id = req.ticketId });
        if (ticket == null || !CanAccess(ticket, user))
            return StatusCode(403, new { status = false, message = "شما دسترسی پاسخ به این تیکت را ندارید." });
        if ((string)ticket.status == "closed")
            return BadRequest(new { status = false, message = "این تیکت بسته شده است." });

        string senderRole = NormalizeRole((string)(user.roles ?? ""));
        if ((string)ticket.requester_national_id == NationalId && senderRole != "ADMIN")
            senderRole = "USER";

        string senderName = $"{user.first_name} {user.last_name}".Trim();

        if (!string.IsNullOrWhiteSpace(req.message))
            await conn.ExecuteAsync(
                "INSERT INTO support_ticket_messages (ticket_id, sender_national_id, sender_name, sender_role, message, attachments) VALUES (@tid,@nid,@name,@role,@msg,'[]')",
                new { tid = req.ticketId, nid = NationalId, name = senderName, role = senderRole, msg = req.message });

        string updateSql = req.closeTicket
            ? "UPDATE support_tickets SET updated_at=GETDATE(), status='closed', closed_at=GETDATE(), closed_by=@by WHERE id=@id"
            : "UPDATE support_tickets SET updated_at=GETDATE(), status='open' WHERE id=@id";
        await conn.ExecuteAsync(updateSql, new { by = NationalId, id = req.ticketId });

        return Ok(new { status = true, message = "پاسخ تیکت ثبت شد." });
    }
}

public record SaveTicketRequest(string? subject, string? category, string? priority, string? description, string? targetRole);
public record ReplyTicketRequest(int ticketId, string? message, bool closeTicket = false);

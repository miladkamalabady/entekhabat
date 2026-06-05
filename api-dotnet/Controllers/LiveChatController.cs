using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntekhabatApi.Services;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class LiveChatController : ControllerBase
{
    private readonly DatabaseService _db;

    public LiveChatController(DatabaseService db) => _db = db;

    private string NationalId => User.Claims.FirstOrDefault(c => c.Type == "national_id")?.Value ?? "";

    private async Task EnsureTables(MySqlConnector.MySqlConnection conn)
    {
        await conn.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS live_chat_sessions (
            id INT(11) NOT NULL AUTO_INCREMENT,
            session_code VARCHAR(32) NOT NULL,
            user_national_id VARCHAR(20) NOT NULL,
            user_name VARCHAR(255) DEFAULT NULL,
            user_role VARCHAR(50) DEFAULT NULL,
            user_region_id VARCHAR(20) DEFAULT NULL,
            assigned_agent_national_id VARCHAR(20) DEFAULT NULL,
            assigned_agent_name VARCHAR(255) DEFAULT NULL,
            status ENUM('waiting','active','closed') NOT NULL DEFAULT 'waiting',
            subject VARCHAR(255) DEFAULT NULL,
            created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            closed_at TIMESTAMP NULL DEFAULT NULL,
            PRIMARY KEY (id), UNIQUE KEY session_code (session_code)) ENGINE=InnoDB DEFAULT CHARSET=utf8");

        await conn.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS live_chat_messages (
            id INT(11) NOT NULL AUTO_INCREMENT,
            session_id INT(11) NOT NULL,
            sender_national_id VARCHAR(20) DEFAULT NULL,
            sender_name VARCHAR(255) DEFAULT NULL,
            sender_role VARCHAR(50) DEFAULT NULL,
            sender_type ENUM('user','support','system') NOT NULL DEFAULT 'user',
            message TEXT NOT NULL,
            created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            is_read TINYINT(1) NOT NULL DEFAULT 0,
            PRIMARY KEY (id),
            CONSTRAINT lc_msg_fk FOREIGN KEY (session_id) REFERENCES live_chat_sessions(id) ON DELETE CASCADE) ENGINE=InnoDB DEFAULT CHARSET=utf8");
    }

    private static bool IsSupportAgent(string role) =>
        new[] { "ADMIN", "SUPERVISOR", "EXECUTIVE" }.Contains(role);

    private static object FormatSession(IDictionary<string, object> r) => new
    {
        id = Convert.ToInt32(r["id"]),
        sessionCode = r["session_code"],
        userNationalId = r["user_national_id"],
        userName = r["user_name"],
        userRole = r["user_role"],
        userRegionId = r["user_region_id"],
        assignedAgentNationalId = r["assigned_agent_national_id"],
        assignedAgentName = r["assigned_agent_name"],
        status = r["status"],
        subject = r["subject"],
        createdAt = r["created_at"],
        updatedAt = r["updated_at"],
        closedAt = r["closed_at"]
    };

    private static object FormatMessage(IDictionary<string, object> r) => new
    {
        id = Convert.ToInt32(r["id"]),
        sessionId = Convert.ToInt32(r["session_id"]),
        senderNationalId = r["sender_national_id"],
        senderName = r["sender_name"],
        senderRole = r["sender_role"],
        senderType = r["sender_type"],
        text = r["message"],
        createdAt = r["created_at"],
        isRead = Convert.ToInt32(r["is_read"]) == 1
    };

    private async Task<IDictionary<string, object>?> GetUser(MySqlConnector.MySqlConnection conn, string nid)
    {
        var row = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT national_id, first_name, last_name, roles, region_id, regionName FROM users WHERE national_id=@nid LIMIT 1",
            new { nid });
        if (row == null) return null;
        var d = (IDictionary<string, object>)row;
        d["full_name"] = $"{d["first_name"]} {d["last_name"]}".Trim();
        return d;
    }

    private async Task<IDictionary<string, object>?> CanAccess(MySqlConnector.MySqlConnection conn, int sessionId, string nid, IDictionary<string, object> user)
    {
        var row = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM live_chat_sessions WHERE id=@id LIMIT 1", new { id = sessionId });
        if (row == null) return null;
        var s = (IDictionary<string, object>)row;
        if ((string)s["user_national_id"] == nid) return s;
        if (IsSupportAgent((string)user["roles"])) return s;
        if ((string?)s["assigned_agent_national_id"] == nid) return s;
        return null;
    }

    // POST /api/startLiveChat  {subject?}
    [HttpPost("startLiveChat")]
    public async Task<IActionResult> StartLiveChat([FromBody] StartChatRequest? req)
    {
        await using var conn = _db.CreateConnection();
        await EnsureTables(conn);

        var user = await GetUser(conn, NationalId);
        if (user == null) return NotFound(new { status = false, message = "کاربر یافت نشد" });

        string subject = string.IsNullOrWhiteSpace(req?.subject) ? "گفتگوی آنلاین پشتیبانی" : req.subject;
        subject = subject.Length > 250 ? subject[..250] : subject;

        // Check existing active session
        var existing = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM live_chat_sessions WHERE user_national_id=@nid AND status IN ('waiting','active') ORDER BY id DESC LIMIT 1",
            new { nid = NationalId });

        IDictionary<string, object> session;
        if (existing != null)
        {
            session = (IDictionary<string, object>)existing;
        }
        else
        {
            var code = "LC" + DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture) + Random.Shared.Next(100, 999);
            string fullName = (string)user["full_name"];
            string role = (string)user["roles"];
            string regionId = user["region_id"]?.ToString() ?? "";

            await conn.ExecuteAsync(
                "INSERT INTO live_chat_sessions (session_code, user_national_id, user_name, user_role, user_region_id, subject) VALUES (@code,@nid,@name,@role,@rid,@subj)",
                new { code, nid = NationalId, name = fullName, role, rid = regionId, subj = subject });

            int sessionId = (int)await conn.ExecuteScalarAsync("SELECT LAST_INSERT_ID()");
            string welcome = "گفتگوی آنلاین شما شروع شد. لطفاً پیام خود را بنویسید تا اولین پشتیبان آنلاین پاسخ دهد.";
            await conn.ExecuteAsync(
                "INSERT INTO live_chat_messages (session_id, sender_type, sender_name, sender_role, message, is_read) VALUES (@sid,'system','سامانه','SYSTEM',@msg,1)",
                new { sid = sessionId, msg = welcome });

            var newSession = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM live_chat_sessions WHERE id=@id LIMIT 1", new { id = sessionId });
            session = (IDictionary<string, object>)newSession!;
        }

        var agentCount = await conn.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(*) FROM users WHERE roles IN ('ADMIN','SUPERVISOR','EXECUTIVE')");

        return Ok(new { status = true, data = FormatSession(session), onlineAgents = agentCount });
    }

    // POST /api/closeLiveChat  {sessionId}
    [HttpPost("closeLiveChat")]
    public async Task<IActionResult> CloseLiveChat([FromBody] SessionRequest req)
    {
        await using var conn = _db.CreateConnection();
        await EnsureTables(conn);

        var user = await GetUser(conn, NationalId);
        var session = user != null ? await CanAccess(conn, req.sessionId, NationalId, user) : null;
        if (session == null)
            return StatusCode(403, new { status = false, message = "دسترسی به این گفتگو مجاز نیست" });

        await conn.ExecuteAsync(
            "UPDATE live_chat_sessions SET status='closed', closed_at=NOW(), updated_at=NOW() WHERE id=@id",
            new { id = req.sessionId });

        await conn.ExecuteAsync(
            "INSERT INTO live_chat_messages (session_id, sender_national_id, sender_name, sender_role, sender_type, message, is_read) VALUES (@sid,@nid,@name,@role,'system','گفتگو بسته شد.',0)",
            new { sid = req.sessionId, nid = NationalId, name = user!["full_name"], role = user["roles"] });

        return Ok(new { status = true, message = "گفتگو بسته شد" });
    }

    // GET /api/getLiveChatMessages?sessionId=&afterId=
    [HttpGet("getLiveChatMessages")]
    public async Task<IActionResult> GetLiveChatMessages([FromQuery] int sessionId, [FromQuery] int afterId = 0)
    {
        if (sessionId == 0)
            return BadRequest(new { status = false, message = "پارامترهای گفتگو کامل نیست" });

        await using var conn = _db.CreateConnection();
        await EnsureTables(conn);

        var user = await GetUser(conn, NationalId);
        var session = user != null ? await CanAccess(conn, sessionId, NationalId, user) : null;
        if (session == null)
            return StatusCode(403, new { status = false, message = "دسترسی به این گفتگو مجاز نیست" });

        string afterClause = afterId > 0 ? "AND id>@afterId" : "";
        var msgs = (await conn.QueryAsync<dynamic>(
            $"SELECT * FROM live_chat_messages WHERE session_id=@sid {afterClause} ORDER BY id ASC LIMIT 200",
            new { sid = sessionId, afterId })).AsList();

        await conn.ExecuteAsync(
            "UPDATE live_chat_messages SET is_read=1 WHERE session_id=@sid AND (sender_national_id IS NULL OR sender_national_id<>@nid)",
            new { sid = sessionId, nid = NationalId });

        var updatedSession = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM live_chat_sessions WHERE id=@id LIMIT 1", new { id = sessionId });

        return Ok(new
        {
            status = true,
            data = msgs.Select(r => FormatMessage((IDictionary<string, object>)r)),
            session = updatedSession != null ? FormatSession((IDictionary<string, object>)updatedSession) : null
        });
    }

    // GET /api/getLiveChatSessions?limit=30
    [HttpGet("getLiveChatSessions")]
    public async Task<IActionResult> GetLiveChatSessions([FromQuery] int limit = 30)
    {
        limit = Math.Clamp(limit, 1, 100);
        await using var conn = _db.CreateConnection();
        await EnsureTables(conn);

        var user = await GetUser(conn, NationalId);
        if (user == null) return NotFound(new { status = false, message = "کاربر یافت نشد" });

        bool isAgent = IsSupportAgent((string)user["roles"]);
        string where = isAgent
            ? $"(s.status IN ('waiting','active') OR s.assigned_agent_national_id='{NationalId}')"
            : $"s.user_national_id='{NationalId}'";

        var rows = (await conn.QueryAsync<dynamic>(
            $@"SELECT s.*,
                (SELECT message FROM live_chat_messages m WHERE m.session_id=s.id ORDER BY m.id DESC LIMIT 1) AS last_message,
                (SELECT COUNT(*) FROM live_chat_messages m WHERE m.session_id=s.id AND m.sender_national_id<>'{NationalId}' AND m.is_read=0) AS unread_count
               FROM live_chat_sessions s
               WHERE {where}
               ORDER BY FIELD(s.status,'waiting','active','closed'), s.updated_at DESC
               LIMIT {limit}")).AsList();

        var agentCount = await conn.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(*) FROM users WHERE roles IN ('ADMIN','SUPERVISOR','EXECUTIVE')");

        var sessions = rows.Select(r =>
        {
            var d = (IDictionary<string, object>)r;
            var s = FormatSession(d) as dynamic;
            return new
            {
                id = Convert.ToInt32(d["id"]),
                sessionCode = d["session_code"], userNationalId = d["user_national_id"],
                userName = d["user_name"], userRole = d["user_role"], userRegionId = d["user_region_id"],
                assignedAgentNationalId = d["assigned_agent_national_id"], assignedAgentName = d["assigned_agent_name"],
                status = d["status"], subject = d["subject"], createdAt = d["created_at"],
                updatedAt = d["updated_at"], closedAt = d["closed_at"],
                lastMessage = d["last_message"],
                unreadCount = Convert.ToInt32(d["unread_count"])
            };
        });

        return Ok(new { status = true, data = sessions, isSupportAgent = isAgent, onlineAgents = agentCount });
    }

    // POST /api/sendLiveChatMessage  {sessionId, message}
    [HttpPost("sendLiveChatMessage")]
    public async Task<IActionResult> SendLiveChatMessage([FromBody] SendMessageRequest req)
    {
        if (req.sessionId == 0 || string.IsNullOrWhiteSpace(req.message))
            return BadRequest(new { status = false, message = "پیام یا شناسه گفتگو معتبر نیست" });
        if (req.message.Length > 1000)
            return BadRequest(new { status = false, message = "حداکثر طول پیام ۱۰۰۰ کاراکتر است" });

        await using var conn = _db.CreateConnection();
        await EnsureTables(conn);

        var user = await GetUser(conn, NationalId);
        var session = user != null ? await CanAccess(conn, req.sessionId, NationalId, user) : null;
        if (session == null || (string)session["status"] == "closed")
            return StatusCode(403, new { status = false, message = "امکان ارسال پیام در این گفتگو وجود ندارد" });

        bool isAgent = IsSupportAgent((string)user!["roles"]) && (string)session["user_national_id"] != NationalId;
        string senderType = isAgent ? "support" : "user";

        if (isAgent)
            await conn.ExecuteAsync(
                "UPDATE live_chat_sessions SET status='active', assigned_agent_national_id=@nid, assigned_agent_name=@name, updated_at=NOW() WHERE id=@id",
                new { nid = NationalId, name = user["full_name"], id = req.sessionId });
        else
            await conn.ExecuteAsync("UPDATE live_chat_sessions SET updated_at=NOW() WHERE id=@id", new { id = req.sessionId });

        await conn.ExecuteAsync(
            "INSERT INTO live_chat_messages (session_id, sender_national_id, sender_name, sender_role, sender_type, message) VALUES (@sid,@nid,@name,@role,@type,@msg)",
            new { sid = req.sessionId, nid = NationalId, name = user["full_name"], role = user["roles"], type = senderType, msg = req.message });

        int msgId = (int)await conn.ExecuteScalarAsync("SELECT LAST_INSERT_ID()");
        var msgRow = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM live_chat_messages WHERE id=@id LIMIT 1", new { id = msgId });

        return Ok(new { status = true, data = FormatMessage((IDictionary<string, object>)msgRow!) });
    }
}

public record StartChatRequest(string? subject);
public record SessionRequest(int sessionId);
public record SendMessageRequest(int sessionId, string message);

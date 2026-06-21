using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MySqlConnector;

namespace EntekhabatApi.Controllers;

[ApiController]
[Route("")]
public class SendBaleSchoolController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _http;

    private const string BaleApiUrl    = "https://safir.bale.ai/api/v3/send_batch";
    private const string BaleUploadUrl = "https://safir.bale.ai/api/v3/upload_file";
    private const string BaleApiKey    = "KzptvJLxvJv40mtJ";
    private const int    BaleBotId     = 82365147;
    private const string BaleToken     = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJib3RfbmFtZSI6Itii2LLZhdmI2YYg2YbZh9in24zbjCDYotmF2YjYsti0INmIINm-2LHZiNix2LQiLCJvcmdfbmFtZSI6ImFtb296ZXNoMyIsImJvdF9pZCI6MTM5MTY2MDY1OCwiaXNfdmVyaWZpZWQiOnRydWUsImlzc3VlZF9hdCI6IjIwMjUtMDUtMTFUMTk6MzI6MjQuOTMwMjEwMTgzWiIsImV4cGlyZWRfYXQiOiIyMDI1LTA1LTEyVDE5OjMyOjI0LjkzMDIxMDMzWiJ9.cmYBT5bgk8fmWoGG_BOlZYDUlrNLTW0SrApwgfzrSks";

    private const string SqlSrvHost = "192.168.0.231";
    private const string SqlSrvDb   = "TotalSchool";
    private const string SqlSrvUser = "s.moslemi";
    private const string SqlSrvPass = "123456";

    public SendBaleSchoolController(IConfiguration config, IHttpClientFactory http)
    {
        _config = config;
        _http   = http;
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private MySqlConnection OpenMysql()
    {
        var cs = _config.GetConnectionString("BaleSchoolConnection")!;
        var conn = new MySqlConnection(cs);
        conn.Open();
        return conn;
    }

    private static string? NormalizePhone(string? p)
    {
        if (p == null) return null;
        p = System.Text.RegularExpressions.Regex.Replace(p, @"\D", "");
        if (p.Length == 10 && p[0] == '9') return "98" + p;
        if (p.Length == 11 && p[0] == '0') return "98" + p[1..];
        if (p.Length == 12 && p.StartsWith("98")) return p;
        if (p.Length == 14 && p.StartsWith("0098")) return p[2..];
        return null;
    }

    private static string? MaskPhone(string? p)
    {
        var n = NormalizePhone(p);
        if (n == null || n.Length < 8) return null;
        return n[..4] + "***" + n[^4..];
    }

    private static bool IsValidPhone(string? p) => NormalizePhone(p) != null;

    // ── ensure tables exist ──────────────────────────────────────────────────

    private void EnsureTables(MySqlConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS history (
    id INT AUTO_INCREMENT PRIMARY KEY,
    school_code VARCHAR(20) NOT NULL,
    school_name VARCHAR(100) NOT NULL,
    year INT NOT NULL DEFAULT 1404,
    class_name  VARCHAR(50),
    INDEX (year, school_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS student (
    id INT AUTO_INCREMENT PRIMARY KEY,
    student_code VARCHAR(15),
    first_name VARCHAR(50) NOT NULL,
    last_name  VARCHAR(50) NOT NULL,
    school_code VARCHAR(20) NOT NULL,
    class_name  VARCHAR(50),
    student_phone VARCHAR(15),
    father_phone  VARCHAR(15),
    mother_phone  VARCHAR(15),
    INDEX (school_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
        cmd.ExecuteNonQuery();
    }

    // ── seed ────────────────────────────────────────────────────────────────

    private void RunSeed(MySqlConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "TRUNCATE TABLE student; TRUNCATE TABLE history;";
        cmd.ExecuteNonQuery();

        var schools = new[] {
            ("12345", "دبستان شهید مطهری",    1404, new[] {"اول الف","اول ب","دوم الف","سوم الف"}),
            ("67890", "راهنمایی امام خمینی",  1404, new[] {"هفتم الف","هفتم ب","هشتم الف","نهم الف"}),
            ("11111", "دبیرستان شاهد",         1404, new[] {"دهم الف","یازدهم الف","دوازدهم الف","دوازدهم ب"}),
        };
        var fns  = new[] {"علی","محمد","زهرا","فاطمه","حسین","مریم","رضا","سارا","امیر","نگار","مهدی","لیلا","یوسف","آرش","نیلوفر","پریسا","کیانوش","شیما","امین","ندا"};
        var lns  = new[] {"احمدی","حسینی","محمدی","رضایی","کریمی","موسوی","نجفی","صادقی","قاسمی","ابراهیمی","اکبری","جعفری","حیدری","عباسی","زارع","مرادی"};
        var pxs  = new[] {"0912","0913","0914","0915","0911","0916","0935","0936","0910","0901","0902","0930"};
        var rng  = new Random();

        foreach (var (code, name, year, classes) in schools)
        {
            foreach (var cls in classes)
            {
                cmd.CommandText = "INSERT INTO history (school_code,school_name,year,class_name) VALUES (@c,@n,@y,@cl)";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@c",  code);
                cmd.Parameters.AddWithValue("@n",  name);
                cmd.Parameters.AddWithValue("@y",  year);
                cmd.Parameters.AddWithValue("@cl", cls);
                cmd.ExecuteNonQuery();
            }
            int seq = 1;
            foreach (var cls in classes)
            {
                for (int k = 1; k <= 5; k++, seq++)
                {
                    string sp = (seq % 6 == 0) ? "" : (seq % 6 == 3) ? "0123456" : pxs[rng.Next(pxs.Length)] + rng.Next(1000000, 9999999);
                    string fp = (seq % 5 == 1) ? "" : pxs[rng.Next(pxs.Length)] + rng.Next(1000000, 9999999);
                    string mp = (seq % 4 == 2) ? "" : pxs[rng.Next(pxs.Length)] + rng.Next(1000000, 9999999);
                    cmd.CommandText = "INSERT INTO student (student_code,first_name,last_name,school_code,class_name,student_phone,father_phone,mother_phone) VALUES (@sc,@fn,@ln,@cd,@cl,@sp,@fp,@mp)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@sc", code + "04" + seq.ToString("D3"));
                    cmd.Parameters.AddWithValue("@fn", fns[rng.Next(fns.Length)]);
                    cmd.Parameters.AddWithValue("@ln", lns[rng.Next(lns.Length)]);
                    cmd.Parameters.AddWithValue("@cd", code);
                    cmd.Parameters.AddWithValue("@cl", cls);
                    cmd.Parameters.AddWithValue("@sp", string.IsNullOrEmpty(sp) ? (object)DBNull.Value : sp);
                    cmd.Parameters.AddWithValue("@fp", string.IsNullOrEmpty(fp) ? (object)DBNull.Value : fp);
                    cmd.Parameters.AddWithValue("@mp", string.IsNullOrEmpty(mp) ? (object)DBNull.Value : mp);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    // ── dispatcher ───────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Dispatch()
    {
        var action = Request.Form["action"].ToString();
        return action switch
        {
            "reseed"              => await Reseed(),
            "get_students_sqlsrv" => await GetStudentsSqlSrv(),
            "get_schools"         => await GetSchools(),
            "get_students"        => await GetStudents(),
            "upload_file"         => await UploadFile(),
            "send_batch"          => await SendBatch(),
            _                     => Ok(new { error = "action نامعتبر" })
        };
    }

    // ── reseed ───────────────────────────────────────────────────────────────

    private Task<IActionResult> Reseed()
    {
        using var conn = OpenMysql();
        EnsureTables(conn);
        RunSeed(conn);
        return Task.FromResult<IActionResult>(Ok(new { ok = true }));
    }

    // ── get_students_sqlsrv ──────────────────────────────────────────────────

    private async Task<IActionResult> GetStudentsSqlSrv()
    {
        var sc = System.Text.RegularExpressions.Regex.Replace(Request.Form["school_code"].ToString(), @"[^0-9]", "");
        if (string.IsNullOrEmpty(sc))
            return Ok(new { error = "کد مدرسه وارد نشده" });

        var cs = $"Server={SqlSrvHost};Database={SqlSrvDb};User Id={SqlSrvUser};Password={SqlSrvPass};TrustServerCertificate=True;";
        try
        {
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var sql = $@"
SELECT h.Student_Id,
       s.FirstName, s.LastName,
       h.ClassRoom_Id, cl.ClassRoomTitle,
       h.School_Id, sch.Title AS SchoolTitle,
       s.FatherMobileNumber, s.MotherMobileNumber, s.StudentMobileNumber
FROM [TotalSchool].[dbo].[History]   AS h
JOIN [TotalSchool].[dbo].[Student]   AS s   ON s.id  = h.Student_Id
JOIN [TotalSchool].[dbo].[ClassRoom] AS cl  ON cl.id = h.ClassRoom_Id
JOIN [TotalSchool].[dbo].[School]    AS sch ON sch.id = h.School_Id
WHERE h.TimeYearType_Id = 1404 AND h.StudentStateType_Id IN (1,3) AND h.School_Id = {sc}";

            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            var students    = new List<object>();
            var classes     = new List<string>();
            var stats       = new Dictionary<string, int> { ["student"] = 0, ["father"] = 0, ["mother"] = 0 };
            var school_name = "";

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrEmpty(school_name))
                    school_name = reader["SchoolTitle"]?.ToString()?.Trim() ?? "";

                var spRaw = reader["StudentMobileNumber"]?.ToString();
                var fpRaw = reader["FatherMobileNumber"]?.ToString();
                var mpRaw = reader["MotherMobileNumber"]?.ToString();

                var vsp = IsValidPhone(spRaw);
                var vfp = IsValidPhone(fpRaw);
                var vmp = IsValidPhone(mpRaw);

                if (vsp) stats["student"]++;
                if (vfp) stats["father"]++;
                if (vmp) stats["mother"]++;

                var cls = reader["ClassRoomTitle"]?.ToString()?.Trim() ?? "نامشخص";
                if (!classes.Contains(cls)) classes.Add(cls);

                students.Add(new
                {
                    id           = reader["Student_Id"],
                    student_code = reader["Student_Id"]?.ToString() ?? "",
                    school_code  = reader["School_Id"]?.ToString() ?? sc,
                    class_name   = cls,
                    first_name   = reader["FirstName"]?.ToString()?.Trim() ?? "",
                    last_name    = reader["LastName"]?.ToString()?.Trim() ?? "",
                    name         = (reader["FirstName"]?.ToString()?.Trim() + " " + reader["LastName"]?.ToString()?.Trim()).Trim(),
                    sp_masked    = MaskPhone(spRaw),
                    fp_masked    = MaskPhone(fpRaw),
                    mp_masked    = MaskPhone(mpRaw),
                    sp           = vsp ? NormalizePhone(spRaw) : null,
                    fp           = vfp ? NormalizePhone(fpRaw) : null,
                    mp           = vmp ? NormalizePhone(mpRaw) : null,
                });
            }

            return Ok(new { students, stats, classes, school_name });
        }
        catch (Exception ex)
        {
            return Ok(new { error = ex.Message });
        }
    }

    // ── get_schools (MySQL) ──────────────────────────────────────────────────

    private Task<IActionResult> GetSchools()
    {
        using var conn = OpenMysql();
        EnsureTables(conn);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT DISTINCT school_code, school_name FROM history WHERE year = 1404 ORDER BY school_name";
        using var reader = cmd.ExecuteReader();

        var rows = new List<object>();
        while (reader.Read())
            rows.Add(new { school_code = reader["school_code"].ToString(), school_name = reader["school_name"].ToString() });

        return Task.FromResult<IActionResult>(Ok(rows));
    }

    // ── get_students (MySQL) ─────────────────────────────────────────────────

    private Task<IActionResult> GetStudents()
    {
        var sc = Request.Form["school_code"].ToString();
        using var conn = OpenMysql();
        EnsureTables(conn);

        // auto-seed if empty
        using (var chk = conn.CreateCommand()) {
            chk.CommandText = "SELECT COUNT(*) FROM student";
            if (Convert.ToInt32(chk.ExecuteScalar()) == 0) RunSeed(conn);
        }

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM student WHERE school_code = @sc ORDER BY class_name, last_name, first_name";
        cmd.Parameters.AddWithValue("@sc", sc);
        using var reader = cmd.ExecuteReader();

        var students = new List<object>();
        var classes  = new List<string>();
        var stats    = new Dictionary<string, int> { ["student"] = 0, ["father"] = 0, ["mother"] = 0 };

        while (reader.Read())
        {
            var spRaw = reader["student_phone"] as string;
            var fpRaw = reader["father_phone"]  as string;
            var mpRaw = reader["mother_phone"]  as string;

            var vsp = IsValidPhone(spRaw);
            var vfp = IsValidPhone(fpRaw);
            var vmp = IsValidPhone(mpRaw);

            if (vsp) stats["student"]++;
            if (vfp) stats["father"]++;
            if (vmp) stats["mother"]++;

            var cls = reader["class_name"]?.ToString() ?? "نامشخص";
            if (!classes.Contains(cls)) classes.Add(cls);

            students.Add(new
            {
                id           = reader["id"],
                student_code = reader["student_code"]?.ToString() ?? "",
                school_code  = reader["school_code"]?.ToString() ?? "",
                class_name   = cls,
                first_name   = reader["first_name"]?.ToString() ?? "",
                last_name    = reader["last_name"]?.ToString() ?? "",
                name         = (reader["first_name"]?.ToString() + " " + reader["last_name"]?.ToString()).Trim(),
                sp_masked    = MaskPhone(spRaw),
                fp_masked    = MaskPhone(fpRaw),
                mp_masked    = MaskPhone(mpRaw),
                sp           = vsp ? NormalizePhone(spRaw) : null,
                fp           = vfp ? NormalizePhone(fpRaw) : null,
                mp           = vmp ? NormalizePhone(mpRaw) : null,
            });
        }

        return Task.FromResult<IActionResult>(Ok(new { students, stats, classes }));
    }

    // ── upload_file ──────────────────────────────────────────────────────────

    private async Task<IActionResult> UploadFile()
    {
        var file = Request.Form.Files.GetFile("file");
        if (file == null) return Ok(new { error = "فایلی دریافت نشد" });

        using var client = _http.CreateClient();
        using var form   = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        var fileContent  = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        form.Add(fileContent, "file", file.FileName);

        client.DefaultRequestHeaders.Add("api-access-key", BaleApiKey);
        client.DefaultRequestHeaders.Add("Authorization",  "Bearer " + BaleToken);

        var res      = await client.PostAsync(BaleUploadUrl, form);
        var body     = await res.Content.ReadAsStringAsync();
        var httpCode = (int)res.StatusCode;

        string? fileId = null;
        try
        {
            var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("file_id", out var fid)) fileId = fid.GetString();
            else if (doc.RootElement.TryGetProperty("result", out var result) && result.TryGetProperty("file_id", out var fid2)) fileId = fid2.GetString();
        }
        catch { }

        return Ok(new { http_code = httpCode, file_id = fileId, raw_response = body });
    }

    // ── send_batch ───────────────────────────────────────────────────────────

    private async Task<IActionResult> SendBatch()
    {
        var form      = Request.Form;
        var sendType  = form["send_type"].ToString();
        var fileId    = form["file_id"].ToString().Trim();
        var btnTexts  = form["btn_text[]"].ToArray();
        var btnUrls   = form["btn_url[]"].ToArray();
        var keyboard  = new List<object>();

        for (int i = 0; i < btnTexts.Length; i++)
        {
            var t = btnTexts[i]?.Trim() ?? "";
            var u = (i < btnUrls.Length ? btnUrls[i] : "")?.Trim() ?? "";
            if (t.Length > 0 && u.Length > 0)
                keyboard.Add(new[] { new { text = t, url = u } });
        }

        var entriesJson = form["entries_json"].ToString().Trim();
        int sentCount;
        List<object> messages;

        if (!string.IsNullOrEmpty(entriesJson))
        {
            var entries = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(entriesJson) ?? new();
            if (entries.Count == 0) return Ok(new { error = "داده‌ای دریافت نشد" });

            messages = entries.Select(e =>
            {
                var text = (e.GetValueOrDefault("text") ?? "").Replace("\r\n", "\n").Replace("\r", "\n").Trim();
                object msg = sendType == "file"
                    ? new { file_id = fileId, text = text.Length > 0 ? (object)text : null }
                    : (object)new { text };
                if (keyboard.Count > 0)
                    msg = AddKeyboard(msg, keyboard);
                return (object)new { phone_numbers = new[] { e.GetValueOrDefault("phone") }, message_data = new { message = msg } };
            }).ToList();

            sentCount = entries.Count;
        }
        else
        {
            var phones = form["phones[]"].Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!.Trim()).ToArray();
            if (phones.Length == 0) return Ok(new { error = "شماره‌ای دریافت نشد" });

            var messageText = form["message_text"].ToString().Replace("\r\n", "\n").Replace("\r", "\n").Trim();
            object msg = sendType == "file"
                ? new { file_id = fileId, text = messageText.Length > 0 ? (object)messageText : null }
                : (object)new { text = messageText };
            if (keyboard.Count > 0) msg = AddKeyboard(msg, keyboard);

            messages  = new List<object> { new { phone_numbers = phones, message_data = new { message = msg } } };
            sentCount = phones.Length;
        }

        var payload = JsonSerializer.Serialize(new
        {
            request_id = $"sch_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Random.Shared.Next(1000, 9999)}",
            bot_id     = BaleBotId,
            messages
        }, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

        using var client  = _http.CreateClient();
        client.DefaultRequestHeaders.Add("api-access-key", BaleApiKey);
        client.DefaultRequestHeaders.Add("Authorization",  "Bearer " + BaleToken);
        var content       = new StringContent(payload, Encoding.UTF8, "application/json");
        var res           = await client.PostAsync(BaleApiUrl, content);
        var baleResp      = await res.Content.ReadAsStringAsync();
        var baleHttp      = (int)res.StatusCode;

        return Ok(new { sent_count = sentCount, bale_http = baleHttp, bale_response = baleResp });
    }

    private static object AddKeyboard(object msg, List<object> keyboard)
    {
        var json   = JsonSerializer.Serialize(msg);
        var dict   = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? new();
        var newDict = new Dictionary<string, object?>();
        foreach (var kv in dict) newDict[kv.Key] = kv.Value;
        newDict["reply_markup"] = new { inline_keyboard = keyboard };
        return newDict;
    }
}

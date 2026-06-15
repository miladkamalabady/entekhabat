using System.Text;
using System.Text.Json;

namespace EntekhabatApi.Services;

public class BaleService
{
    private readonly IHttpClientFactory _http;
    private readonly ILogger<BaleService> _logger;

    private const string BaseUrl  = "http://192.168.0.96:5800";
    private const string Username = "m1";
    private const string Password = "M2";

    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public BaleService(IHttpClientFactory http, ILogger<BaleService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    // توکن را با کش ۵۰ دقیقه‌ای برمی‌گرداند
    private async Task<string?> GetTokenAsync()
    {
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken;

        await _lock.WaitAsync();
        try
        {
            if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry)
                return _cachedToken;

            using var client = _http.CreateClient();
            using var form   = new MultipartFormDataContent();
            form.Add(new StringContent(Username), "username");
            form.Add(new StringContent(Password), "password");

            var res = await client.PostAsync($"{BaseUrl}/user/login/GetToken", form);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("Bale: GetToken failed with {Status}", res.StatusCode);
                return null;
            }

            var body = await res.Content.ReadAsStringAsync();
            // انتظار داریم بدنه خودش توکن باشد یا در فیلد token/Token باشد
            string? token = null;
            try
            {
                var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("token", out var t))
                    token = t.GetString();
                else if (doc.RootElement.TryGetProperty("Token", out var t2))
                    token = t2.GetString();
            }
            catch
            {
                // بدنه مستقیماً توکن رشته‌ای است
                token = body.Trim().Trim('"');
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Bale: Token parse failed. Body: {Body}", body);
                return null;
            }

            _cachedToken  = token;
            _tokenExpiry  = DateTime.UtcNow.AddMinutes(50);
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bale: Exception while getting token");
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    // ارسال پیام به شماره موبایل — fire-and-forget ایمن
    public void SendAsync(string phone, string message)
    {
        if (string.IsNullOrWhiteSpace(phone)) return;
        _ = Task.Run(() => SendInternalAsync(phone, message));
    }

    private async Task SendInternalAsync(string phone, string message)
    {
        try
        {
            var token = await GetTokenAsync();
            if (token == null) return;

            using var client = _http.CreateClient();
            client.DefaultRequestHeaders.Add("Token", token);

            var payload = new[]
            {
                new { Phone = phone, message, sendType = "1" }
            };

            var json    = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PostAsync($"{BaseUrl}/api/BaleMessenger/sendMessage", content);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("Bale: SendMessage to {Phone} failed: {Status}", phone, res.StatusCode);
                // توکن ممکن است منقضی شده باشد — پاک می‌کنیم تا دفعه بعد دوباره بگیریم
                _cachedToken = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bale: Exception while sending to {Phone}", phone);
        }
    }
}

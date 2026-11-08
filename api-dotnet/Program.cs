using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using EntekhabatApi.Services;

var builder = WebApplication.CreateBuilder(args);

// JSON with full Unicode (no \uXXXX escaping for Persian text)
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    opts.JsonSerializerOptions.PropertyNamingPolicy = null; // keep original casing
});

// DI Services
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<JalaliService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<BaleService>();

// JWT
var jwt = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwt["Secret"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.MapInboundClaims = false;
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// CORS - same origins as PHP
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:2000" };

builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyMethod()
         .AllowAnyHeader()
         .AllowCredentials()
         .SetPreflightMaxAge(TimeSpan.FromMinutes(10))));

var app = builder.Build();

// نمایش جزئیات خطا در response (برای debug)
app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    ctx.Response.StatusCode = 500;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsync(
        System.Text.Json.JsonSerializer.Serialize(new
        {
            status = false,
            error  = ex?.Message,
            trace  = ex?.StackTrace
        }));
}));

// CORS باید قبل از routing و authentication باشد
app.UseRouting();
app.UseCors();

// Serve uploaded files at /uploads/...
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Security audit: log mutations and denied API requests without credentials, tokens or payloads.
// Configure a persistent structured log sink in production for retention and review.
app.Use(async (ctx, next) =>
{
    await next();
    if (!ctx.Request.Path.StartsWithSegments("/api")) return;
    bool mutation = !HttpMethods.IsGet(ctx.Request.Method)
        && !HttpMethods.IsHead(ctx.Request.Method)
        && !HttpMethods.IsOptions(ctx.Request.Method);
    bool denied = ctx.Response.StatusCode is 401 or 403;
    if (!mutation && !denied) return;
    var logger = ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("SecurityAudit");
    var nationalId = ctx.User.FindFirst("national_id")?.Value ?? "anonymous";
    logger.LogInformation("API_AUDIT method={Method} path={Path} user={User} status={Status} trace={Trace}",
        ctx.Request.Method, ctx.Request.Path.Value, nationalId,
        ctx.Response.StatusCode, ctx.TraceIdentifier);
});
app.MapControllers();

app.Run();

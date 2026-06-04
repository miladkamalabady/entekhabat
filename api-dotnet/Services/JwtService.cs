using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EntekhabatApi.Services;

public class JwtService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly int _expireHours;

    public JwtService(IConfiguration config)
    {
        _secret = config["Jwt:Secret"]!;
        _issuer = config["Jwt:Issuer"]!;
        _expireHours = int.Parse(config["Jwt:ExpireHours"] ?? "8");
    }

    public string GenerateToken(string nationalId, string roles, string regionName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("national_id", nationalId),
            new Claim("roles", roles),
            new Claim("regionName", regionName ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_expireHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

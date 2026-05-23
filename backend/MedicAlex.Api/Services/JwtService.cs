using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MedicAlex.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace MedicAlex.Api.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    int GetUserIdFromClaims(ClaimsPrincipal user);
    string GetRoleFromClaims(ClaimsPrincipal user);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiresHours;

    public JwtService(IConfiguration config)
    {
        _config = config;
        _secret       = Environment.GetEnvironmentVariable("JWT_SECRET")
                        ?? throw new InvalidOperationException("JWT_SECRET lipsește din .env");
        _issuer       = config["Jwt:Issuer"]   ?? "MedicAlex";
        _audience     = config["Jwt:Audience"] ?? "MedicAlex";
        _expiresHours = int.Parse(config["Jwt:ExpiresHours"] ?? "24");
    }

    public string GenerateToken(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Role,           user.Role),
            new Claim("firstName",               user.FirstName),
            new Claim("lastName",                user.LastName),
        };

        var token = new JwtSecurityToken(
            issuer:             _issuer,
            audience:           _audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(_expiresHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int GetUserIdFromClaims(ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : 0;
    }

    public string GetRoleFromClaims(ClaimsPrincipal principal)
        => principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
}

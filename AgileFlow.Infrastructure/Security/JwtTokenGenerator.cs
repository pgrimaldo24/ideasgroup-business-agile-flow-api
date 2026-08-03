using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AgileFlow.Application.Ports;
using AgileFlow.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AgileFlow.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration) => _configuration = configuration;

    public (string Token, DateTime ExpiresAtUtc) GenerateToken(User user)
    {
        var secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Falta configurar Jwt:Secret (variable de entorno JWT_SECRET).");
        var issuer = _configuration["Jwt:Issuer"] ?? "AgileFlow.Api";
        var audience = _configuration["Jwt:Audience"] ?? "AgileFlow.Client";
        var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var m) ? m : 60;

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}

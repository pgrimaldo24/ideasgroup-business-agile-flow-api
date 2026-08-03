using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.Ports;

public interface IJwtTokenGenerator
{
    /// <summary>Genera el JWT firmado y su fecha de expiración UTC.</summary>
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}

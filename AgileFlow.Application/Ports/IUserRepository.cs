using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.Ports;

/// <summary>
/// Contrato que Application necesita para
/// persistir/consultar usuarios, sin conocer la tecnología concreta.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
}

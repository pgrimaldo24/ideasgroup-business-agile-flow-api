using AgileFlow.Domain.Common;

namespace AgileFlow.Domain.Entities;

/// <summary>
/// Usuario del sistema. El hash de contraseña se calcula en una capa
/// superior (Application/Infrastructure) a través del puerto IPasswordHasher;
/// el dominio solo conoce y transporta el resultado (hash + salt).
/// </summary>
public class User : Entity
{
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string PasswordSalt { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }

    private User() { } // EF Core

    public User(string fullName, string email, string passwordHash, string passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("El nombre del usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("El correo electrónico es obligatorio.");

        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        CreatedAtUtc = DateTime.UtcNow;
    }
}

using System.Security.Cryptography;
using AgileFlow.Application.Ports;
using Microsoft.Extensions.Configuration;

namespace AgileFlow.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int KeySizeBytes = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    private readonly string _pepper;

    public PasswordHasher(IConfiguration configuration)
    {
        _pepper = configuration["Security:PasswordPepper"]
            ?? throw new InvalidOperationException(
                "Falta configurar Security:PasswordPepper (variable de entorno PASSWORD_PEPPER).");
    }

    public (string Hash, string Salt) HashPassword(string plainPassword)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hashBytes = DeriveKey(plainPassword, saltBytes);

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    public bool VerifyPassword(string plainPassword, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var expectedHashBytes = Convert.FromBase64String(hash);
        var actualHashBytes = DeriveKey(plainPassword, saltBytes);

        return CryptographicOperations.FixedTimeEquals(expectedHashBytes, actualHashBytes);
    }

    private byte[] DeriveKey(string plainPassword, byte[] saltBytes)
    {
        var pepperedPassword = plainPassword + _pepper;

        return Rfc2898DeriveBytes.Pbkdf2(pepperedPassword, saltBytes, Iterations, Algorithm, KeySizeBytes);
    }
}

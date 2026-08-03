namespace AgileFlow.Application.Ports;

/// <summary>
/// Puerto de hashing de contraseñas. La implementación (Infrastructure) usa
/// PBKDF2 con salt aleatorio por usuario (guardado en BD) + pepper fijo
/// leído de variable de entorno.
/// </summary>
public interface IPasswordHasher
{
    (string Hash, string Salt) HashPassword(string plainPassword);
    bool VerifyPassword(string plainPassword, string hash, string salt);
}

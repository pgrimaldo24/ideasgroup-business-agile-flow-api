using AgileFlow.Application.Common;
using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;

namespace AgileFlow.Application.UseCases.Auth;

/// <summary>
/// Caso de uso de inicio de sesión.
/// </summary>
public class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> ExecuteAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);

        // Mensaje deliberadamente genérico: no revelar si el email existe o no.
        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            throw new UnauthorizedAppException();

        var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse(token, expiresAtUtc, user.FullName, user.Email);
    }
}

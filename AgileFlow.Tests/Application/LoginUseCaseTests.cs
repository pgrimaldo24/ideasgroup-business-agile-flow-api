using AgileFlow.Application.Common;
using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using AgileFlow.Application.UseCases.Auth;
using AgileFlow.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AgileFlow.Tests.Application;

public class LoginUseCaseTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly LoginUseCase _sut;

    public LoginUseCaseTests()
    {
        _sut = new LoginUseCase(_userRepository, _passwordHasher, _tokenGenerator);
    }

    [Fact]
    public async Task ExecuteAsync_ConCredencialesValidas_DevuelveTokenYDatosDelUsuario()
    {
        var user = new User("Administrador Demo", "admin@ideasgroup.demo", "hash", "salt");
        var expiracion = DateTime.UtcNow.AddHours(1);

        _userRepository.GetByEmailAsync("admin@ideasgroup.demo", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("Kanban#2026", "hash", "salt").Returns(true);
        _tokenGenerator.GenerateToken(user).Returns(("jwt-firmado", expiracion));

        var response = await _sut.ExecuteAsync(new LoginRequest("admin@ideasgroup.demo", "Kanban#2026"));

        response.Token.Should().Be("jwt-firmado");
        response.ExpiresAtUtc.Should().Be(expiracion);
        response.FullName.Should().Be("Administrador Demo");
        response.Email.Should().Be("admin@ideasgroup.demo");
    }

    [Theory]
    [InlineData("  ADMIN@IdeasGroup.Demo  ")]
    [InlineData("Admin@IdeasGroup.Demo")]
    public async Task ExecuteAsync_NormalizaElEmailAntesDeConsultar(string emailIngresado)
    {
        var user = new User("Administrador Demo", "admin@ideasgroup.demo", "hash", "salt");

        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _tokenGenerator.GenerateToken(user).Returns(("token", DateTime.UtcNow));

        await _sut.ExecuteAsync(new LoginRequest(emailIngresado, "Kanban#2026"));

        await _userRepository.Received(1)
            .GetByEmailAsync("admin@ideasgroup.demo", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CuandoElUsuarioNoExiste_LanzaUnauthorized()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = () => _sut.ExecuteAsync(new LoginRequest("nadie@ideasgroup.demo", "x"));

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }

    [Fact]
    public async Task ExecuteAsync_ConPasswordIncorrecta_LanzaUnauthorized()
    {
        var user = new User("Administrador Demo", "admin@ideasgroup.demo", "hash", "salt");

        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var act = () => _sut.ExecuteAsync(new LoginRequest("admin@ideasgroup.demo", "incorrecta"));

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }

    [Fact]
    public async Task ExecuteAsync_UsuarioInexistenteYPasswordIncorrecta_DevuelvenElMismoMensaje()
    {
        // No debe revelarse si el email existe: ambos caminos comparten mensaje.
        var user = new User("Administrador Demo", "admin@ideasgroup.demo", "hash", "salt");

        _userRepository.GetByEmailAsync("admin@ideasgroup.demo", Arg.Any<CancellationToken>()).Returns(user);
        _userRepository.GetByEmailAsync("nadie@ideasgroup.demo", Arg.Any<CancellationToken>()).Returns((User?)null);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var porUsuario = await Capturar(() => _sut.ExecuteAsync(new LoginRequest("nadie@ideasgroup.demo", "x")));
        var porPassword = await Capturar(() => _sut.ExecuteAsync(new LoginRequest("admin@ideasgroup.demo", "x")));

        porUsuario.Should().Be(porPassword);
    }

    [Fact]
    public async Task ExecuteAsync_ConCredencialesInvalidas_NoGeneraToken()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = () => _sut.ExecuteAsync(new LoginRequest("nadie@ideasgroup.demo", "x"));

        await act.Should().ThrowAsync<UnauthorizedAppException>();
        _tokenGenerator.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

    private static async Task<string> Capturar(Func<Task> accion)
    {
        try
        {
            await accion();
            throw new InvalidOperationException("Se esperaba una excepción y no se lanzó.");
        }
        catch (UnauthorizedAppException ex)
        {
            return ex.Message;
        }
    }
}

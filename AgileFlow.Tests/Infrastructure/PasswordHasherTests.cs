using AgileFlow.Infrastructure.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AgileFlow.Tests.Infrastructure;

public class PasswordHasherTests
{
    private const string Pepper = "pepper-de-pruebas";

    [Fact]
    public void VerifyPassword_ConLaMismaContraseña_DevuelveTrue()
    {
        var hasher = CrearHasher(Pepper);

        var (hash, salt) = hasher.HashPassword("Kanban#2026");

        hasher.VerifyPassword("Kanban#2026", hash, salt).Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ConContraseñaDistinta_DevuelveFalse()
    {
        var hasher = CrearHasher(Pepper);

        var (hash, salt) = hasher.HashPassword("Kanban#2026");

        hasher.VerifyPassword("otra-contraseña", hash, salt).Should().BeFalse();
    }

    [Fact]
    public void HashPassword_LaMismaContraseñaDosVeces_ProduceSaltYHashDistintos()
    {
        var hasher = CrearHasher(Pepper);

        var primero = hasher.HashPassword("Kanban#2026");
        var segundo = hasher.HashPassword("Kanban#2026");

        segundo.Salt.Should().NotBe(primero.Salt);
        segundo.Hash.Should().NotBe(primero.Hash);
    }

    [Fact]
    public void HashPassword_NoAlmacenaLaContraseñaEnClaro()
    {
        var hasher = CrearHasher(Pepper);

        var (hash, salt) = hasher.HashPassword("Kanban#2026");

        hash.Should().NotContain("Kanban#2026");
        salt.Should().NotContain("Kanban#2026");
    }

    [Fact]
    public void VerifyPassword_ConOtroPepper_NoValidaElHash()
    {
        // El pepper vive solo en configuración del servidor: si se filtra la
        // base de datos pero no el pepper, los hashes no son verificables.
        var original = CrearHasher(Pepper);
        var conOtroPepper = CrearHasher("pepper-diferente");

        var (hash, salt) = original.HashPassword("Kanban#2026");

        conOtroPepper.VerifyPassword("Kanban#2026", hash, salt).Should().BeFalse();
    }

    [Fact]
    public void Constructor_SinPepperConfigurado_LanzaInvalidOperationException()
    {
        var configuracionVacia = new ConfigurationBuilder().Build();

        var act = () => new PasswordHasher(configuracionVacia);

        act.Should().Throw<InvalidOperationException>().WithMessage("*PasswordPepper*");
    }

    private static PasswordHasher CrearHasher(string pepper)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:PasswordPepper"] = pepper
            })
            .Build();

        return new PasswordHasher(configuration);
    }
}

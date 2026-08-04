using AgileFlow.Application.Ports;
using AgileFlow.Infrastructure.Persistence;
using AgileFlow.Infrastructure.Persistence.Seed;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace AgileFlow.Tests.Infrastructure;

public class DbSeederTests
{
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();

    public DbSeederTests()
    {
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns(("hash", "salt"));
    }

    [Fact]
    public async Task SeedAsync_SinUsuariosConfigurados_NoSiembraNada()
    {
        await using var context = CrearContexto();

        await DbSeeder.SeedAsync(context, _passwordHasher, new SeedOptions());

        (await context.Users.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task SeedAsync_CreaLosUsuariosConfigurados()
    {
        await using var context = CrearContexto();

        await DbSeeder.SeedAsync(context, _passwordHasher, OpcionesCon(
            ("Administrador Demo", "admin@ideasgroup.demo", "Kanban#2026"),
            ("Usuario Demo", "usuario@ideasgroup.demo", "Kanban#2026")));

        var usuarios = await context.Users.OrderBy(u => u.Email).ToListAsync();
        usuarios.Should().HaveCount(2);
        usuarios.Select(u => u.Email).Should()
            .Equal("admin@ideasgroup.demo", "usuario@ideasgroup.demo");
    }

    [Fact]
    public async Task SeedAsync_EjecutadoDosVeces_NoDuplicaUsuarios()
    {
        await using var context = CrearContexto();
        var opciones = OpcionesCon(("Administrador Demo", "admin@ideasgroup.demo", "Kanban#2026"));

        await DbSeeder.SeedAsync(context, _passwordHasher, opciones);
        await DbSeeder.SeedAsync(context, _passwordHasher, opciones);

        (await context.Users.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SeedAsync_ConUnUsuarioNuevoEnLaConfiguracion_SoloCreaElQueFalta()
    {
        await using var context = CrearContexto();

        await DbSeeder.SeedAsync(context, _passwordHasher, OpcionesCon(
            ("Administrador Demo", "admin@ideasgroup.demo", "Kanban#2026")));

        await DbSeeder.SeedAsync(context, _passwordHasher, OpcionesCon(
            ("Administrador Demo", "admin@ideasgroup.demo", "Kanban#2026"),
            ("Tercer Usuario", "tercero@ideasgroup.demo", "Kanban#2026")));

        (await context.Users.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task SeedAsync_ConEmailEnMayusculas_NoDuplicaAlUsuarioExistente()
    {
        await using var context = CrearContexto();

        await DbSeeder.SeedAsync(context, _passwordHasher, OpcionesCon(
            ("Administrador Demo", "admin@ideasgroup.demo", "Kanban#2026")));

        await DbSeeder.SeedAsync(context, _passwordHasher, OpcionesCon(
            ("Administrador Demo", "  ADMIN@IdeasGroup.Demo  ", "Kanban#2026")));

        (await context.Users.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SeedAsync_UsaElHasherParaCadaUsuarioNuevo()
    {
        await using var context = CrearContexto();

        await DbSeeder.SeedAsync(context, _passwordHasher, OpcionesCon(
            ("Administrador Demo", "admin@ideasgroup.demo", "Kanban#2026"),
            ("Usuario Demo", "usuario@ideasgroup.demo", "otra-clave")));

        _passwordHasher.Received(1).HashPassword("Kanban#2026");
        _passwordHasher.Received(1).HashPassword("otra-clave");

        var usuario = await context.Users.FirstAsync();
        usuario.PasswordHash.Should().Be("hash");
        usuario.PasswordSalt.Should().Be("salt");
    }

    [Theory]
    [InlineData("", "Kanban#2026")]
    [InlineData("   ", "Kanban#2026")]
    [InlineData("admin@ideasgroup.demo", "")]
    [InlineData("admin@ideasgroup.demo", "   ")]
    public async Task SeedAsync_ConUsuarioIncompleto_LanzaInvalidOperationException(string email, string password)
    {
        await using var context = CrearContexto();

        var act = () => DbSeeder.SeedAsync(context, _passwordHasher, OpcionesCon(("Demo", email, password)));

        await act.Should().ThrowAsync<InvalidOperationException>();
        (await context.Users.CountAsync()).Should().Be(0);
    }

    private static SeedOptions OpcionesCon(params (string FullName, string Email, string Password)[] usuarios) =>
        new()
        {
            Users = usuarios
                .Select(u => new SeedUserOptions
                {
                    FullName = u.FullName,
                    Email = u.Email,
                    Password = u.Password
                })
                .ToList()
        };

    private static KanbanDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<KanbanDbContext>()
            .UseInMemoryDatabase($"agileflow-tests-{Guid.NewGuid()}")
            .Options;

        return new KanbanDbContext(options);
    }
}

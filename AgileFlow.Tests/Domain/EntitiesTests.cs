using AgileFlow.Domain.Common;
using AgileFlow.Domain.Entities;
using AgileFlow.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AgileFlow.Tests.Domain;

public class KanbanTaskTests
{
    private static readonly Guid ColumnId = Guid.NewGuid();

    [Fact]
    public void Constructor_ConDatosValidos_NormalizaTextoYAsignaEstado()
    {
        var task = new KanbanTask("  Diseñar API  ", "  Detalle  ", TaskPriority.Alta, "  Ana  ", ColumnId, 1024m);

        task.Title.Should().Be("Diseñar API");
        task.Description.Should().Be("Detalle");
        task.AssigneeName.Should().Be("Ana");
        task.Priority.Should().Be(TaskPriority.Alta);
        task.ColumnId.Should().Be(ColumnId);
        task.Position.Should().Be(1024m);
        task.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_SinTitulo_LanzaDomainException(string? title)
    {
        var act = () => new KanbanTask(title!, null, TaskPriority.Baja, null, ColumnId, 1m);

        act.Should().Throw<DomainException>().WithMessage("*título*");
    }

    [Fact]
    public void Constructor_SinColumna_LanzaDomainException()
    {
        var act = () => new KanbanTask("Tarea", null, TaskPriority.Baja, null, Guid.Empty, 1m);

        act.Should().Throw<DomainException>().WithMessage("*columna*");
    }

    [Fact]
    public void MoveTo_ActualizaColumnaYPosicion()
    {
        var task = new KanbanTask("Tarea", null, TaskPriority.Baja, null, ColumnId, 1024m);
        var destino = Guid.NewGuid();

        task.MoveTo(destino, 512m);

        task.ColumnId.Should().Be(destino);
        task.Position.Should().Be(512m);
    }

    [Fact]
    public void MoveTo_ConColumnaInvalida_LanzaDomainExceptionYNoMuta()
    {
        var task = new KanbanTask("Tarea", null, TaskPriority.Baja, null, ColumnId, 1024m);

        var act = () => task.MoveTo(Guid.Empty, 512m);

        act.Should().Throw<DomainException>();
        task.ColumnId.Should().Be(ColumnId);
        task.Position.Should().Be(1024m);
    }

    [Fact]
    public void Update_CambiaDatosPeroConservaColumnaYPosicion()
    {
        var task = new KanbanTask("Original", "desc", TaskPriority.Baja, "Ana", ColumnId, 1024m);

        task.Update("Actualizado", "nueva desc", TaskPriority.Alta, "Luis");

        task.Title.Should().Be("Actualizado");
        task.Description.Should().Be("nueva desc");
        task.Priority.Should().Be(TaskPriority.Alta);
        task.AssigneeName.Should().Be("Luis");
        task.ColumnId.Should().Be(ColumnId);
        task.Position.Should().Be(1024m);
    }
}

public class BoardColumnTests
{
    [Fact]
    public void Constructor_ConDatosValidos_RecortaElNombre()
    {
        var column = new BoardColumn("  Por hacer  ", 1024m, Guid.NewGuid());

        column.Name.Should().Be("Por hacer");
        column.Position.Should().Be(1024m);
    }

    [Fact]
    public void Constructor_SinProyecto_LanzaDomainException()
    {
        var act = () => new BoardColumn("Por hacer", 1m, Guid.Empty);

        act.Should().Throw<DomainException>().WithMessage("*proyecto*");
    }

    [Fact]
    public void Rename_SinNombre_LanzaDomainException()
    {
        var column = new BoardColumn("Por hacer", 1m, Guid.NewGuid());

        var act = () => column.Rename("   ");

        act.Should().Throw<DomainException>();
        column.Name.Should().Be("Por hacer");
    }
}

public class ProjectTests
{
    [Fact]
    public void Constructor_ConFechaFinAnteriorAInicio_LanzaDomainException()
    {
        var inicio = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var fin = inicio.AddDays(-1);

        var act = () => new Project("AgileFlow", null, inicio, fin, ProjectStatus.EnProgreso);

        act.Should().Throw<DomainException>().WithMessage("*fin prevista*");
    }

    [Fact]
    public void Constructor_ConMismaFechaInicioYFin_EsValido()
    {
        var fecha = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        var act = () => new Project("AgileFlow", null, fecha, fecha, ProjectStatus.EnProgreso);

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_NuevoProyecto_NoTieneColumnas()
    {
        var project = CrearProyecto();

        project.Columns.Should().BeEmpty();
    }

    [Fact]
    public void Update_ConFechasInvalidas_NoModificaElProyecto()
    {
        var project = CrearProyecto();
        var inicio = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        var act = () => project.Update("Otro", null, inicio, inicio.AddDays(-5), ProjectStatus.Completado);

        act.Should().Throw<DomainException>();
        project.Name.Should().Be("AgileFlow");
    }

    private static Project CrearProyecto()
    {
        var inicio = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        return new Project("AgileFlow", "desc", inicio, inicio.AddMonths(2), ProjectStatus.EnProgreso);
    }
}

public class UserTests
{
    [Fact]
    public void Constructor_NormalizaElEmailAMinusculasYRecortaEspacios()
    {
        var user = new User("  Ana Pérez  ", "  ADMIN@IdeasGroup.Demo  ", "hash", "salt");

        user.FullName.Should().Be("Ana Pérez");
        user.Email.Should().Be("admin@ideasgroup.demo");
    }

    [Theory]
    [InlineData("", "a@b.com")]
    [InlineData("   ", "a@b.com")]
    [InlineData("Ana", "")]
    [InlineData("Ana", "   ")]
    public void Constructor_ConDatosObligatoriosVacios_LanzaDomainException(string fullName, string email)
    {
        var act = () => new User(fullName, email, "hash", "salt");

        act.Should().Throw<DomainException>();
    }
}

public class EntityTests
{
    [Fact]
    public void Equals_MismoIdYMismoTipo_SonIguales()
    {
        var column = new BoardColumn("Por hacer", 1m, Guid.NewGuid());
        var mismaReferencia = column;

        column.Equals(mismaReferencia).Should().BeTrue();
        column.GetHashCode().Should().Be(mismaReferencia.GetHashCode());
    }

    [Fact]
    public void Equals_DistintasInstancias_TienenIdentidadPropia()
    {
        var a = new BoardColumn("Por hacer", 1m, Guid.NewGuid());
        var b = new BoardColumn("Por hacer", 1m, Guid.NewGuid());

        a.Id.Should().NotBe(b.Id);
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_ComparadoConOtroTipo_DevuelveFalse()
    {
        var column = new BoardColumn("Por hacer", 1m, Guid.NewGuid());

        column.Equals("no es una entidad").Should().BeFalse();
    }
}

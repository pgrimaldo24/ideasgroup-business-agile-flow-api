using AgileFlow.Domain.Common;
using AgileFlow.Domain.Services;
using FluentAssertions;
using Xunit;

namespace AgileFlow.Tests.Domain;

public class TaskOrderingServiceTests
{
    [Fact]
    public void CalculateNewPosition_ColumnaVacia_DevuelveGapStep()
    {
        var position = TaskOrderingService.CalculateNewPosition(null, null);

        position.Should().Be(TaskOrderingService.GapStep);
    }

    [Fact]
    public void CalculateNewPosition_AlInicio_DevuelveLaMitadDelPrimero()
    {
        var position = TaskOrderingService.CalculateNewPosition(null, 1024m);

        position.Should().Be(512m);
    }

    [Fact]
    public void CalculateNewPosition_AlFinal_SumaGapStepAlUltimo()
    {
        var position = TaskOrderingService.CalculateNewPosition(2048m, null);

        position.Should().Be(2048m + TaskOrderingService.GapStep);
    }

    [Theory]
    [InlineData(1024, 2048, 1536)]
    [InlineData(0, 10, 5)]
    [InlineData(512, 513, 512.5)]
    public void CalculateNewPosition_EntreDosVecinos_DevuelveElPuntoMedio(
        decimal previous, decimal next, decimal expected)
    {
        var position = TaskOrderingService.CalculateNewPosition(previous, next);

        position.Should().Be(expected);
    }

    [Fact]
    public void CalculateNewPosition_EntreDosVecinos_QuedaEstrictamenteEnMedio()
    {
        var position = TaskOrderingService.CalculateNewPosition(100m, 200m);

        position.Should().BeGreaterThan(100m).And.BeLessThan(200m);
    }

    [Theory]
    [InlineData(2048, 1024)]
    [InlineData(500, 500)]
    public void CalculateNewPosition_VecinosEnOrdenInvalido_LanzaDomainException(
        decimal previous, decimal next)
    {
        var act = () => TaskOrderingService.CalculateNewPosition(previous, next);

        act.Should().Throw<DomainException>()
            .WithMessage("*menor que*");
    }

    [Fact]
    public void CalculateNewPosition_HuecoAgotado_ExigeRebalanceo()
    {
        // Diferencia igual al mínimo tolerado: ya no hay espacio para partir.
        var act = () => TaskOrderingService.CalculateNewPosition(1m, 1.0001m);

        act.Should().Throw<DomainException>()
            .WithMessage("*rebalancear*");
    }

    [Fact]
    public void CalculateNewPosition_InsercionesRepetidasAlInicio_MantienenElOrden()
    {
        // Simula arrastrar tareas sucesivamente a la primera posición: cada
        // nueva posición debe seguir siendo menor que la anterior y positiva.
        var current = TaskOrderingService.CalculateNewPosition(null, null);

        for (var i = 0; i < 10; i++)
        {
            var next = TaskOrderingService.CalculateNewPosition(null, current);

            next.Should().BeLessThan(current);
            next.Should().BeGreaterThan(0m);

            current = next;
        }
    }

    [Fact]
    public void CalculateNewPosition_InsercionesRepetidasAlFinal_SonSiempreCrecientes()
    {
        var current = TaskOrderingService.CalculateNewPosition(null, null);

        for (var i = 0; i < 10; i++)
        {
            var next = TaskOrderingService.CalculateNewPosition(current, null);

            next.Should().BeGreaterThan(current);

            current = next;
        }
    }
}

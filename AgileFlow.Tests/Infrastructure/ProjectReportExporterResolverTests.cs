using AgileFlow.Application.Common;
using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using AgileFlow.Infrastructure.Reports;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AgileFlow.Tests.Infrastructure;

public class ProjectReportExporterResolverTests
{
    [Theory]
    [InlineData("pdf")]
    [InlineData("PDF")]
    [InlineData("Pdf")]
    public void Resolve_IgnoraMayusculasYMinusculas(string formato)
    {
        var pdf = Exportador("pdf");
        var sut = new ProjectReportExporterResolver(new[] { pdf, Exportador("xlsx") });

        sut.Resolve(formato).Should().BeSameAs(pdf);
    }

    [Fact]
    public void Resolve_DevuelveElExportadorDeCadaFormatoRegistrado()
    {
        var pdf = Exportador("pdf");
        var xlsx = Exportador("xlsx");
        var sut = new ProjectReportExporterResolver(new[] { pdf, xlsx });

        sut.Resolve("pdf").Should().BeSameAs(pdf);
        sut.Resolve("xlsx").Should().BeSameAs(xlsx);
    }

    [Fact]
    public void Resolve_ConFormatoDesconocido_LanzaAppException()
    {
        var sut = new ProjectReportExporterResolver(new[] { Exportador("pdf") });

        var act = () => sut.Resolve("csv");

        act.Should().Throw<AppException>().WithMessage("*csv*");
    }

    [Fact]
    public void Resolve_ConUnFormatoNuevo_NoRequiereCambiosEnElResolver()
    {
        // Extensibilidad del patrón Strategy: basta registrar un exportador
        // adicional en DI para que quede disponible.
        var csv = Exportador("csv");
        var sut = new ProjectReportExporterResolver(new[] { Exportador("pdf"), Exportador("xlsx"), csv });

        sut.Resolve("csv").Should().BeSameAs(csv);
    }

    [Fact]
    public void Constructor_SinExportadores_HaceQueCualquierFormatoFalle()
    {
        var sut = new ProjectReportExporterResolver(Array.Empty<IProjectReportExporter>());

        var act = () => sut.Resolve("pdf");

        act.Should().Throw<AppException>();
    }

    private static IProjectReportExporter Exportador(string formato)
    {
        var exportador = Substitute.For<IProjectReportExporter>();
        exportador.Format.Returns(formato);
        exportador.FileExtension.Returns(formato);
        exportador.Export(Arg.Any<ProjectReportDto>()).Returns(Array.Empty<byte>());
        return exportador;
    }
}

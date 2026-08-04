using AgileFlow.Application.Common;
using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using AgileFlow.Application.UseCases.Reports;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace AgileFlow.Tests.Application;

public class GenerateProjectReportUseCaseTests
{
    private readonly IProjectReportQuery _reportQuery = Substitute.For<IProjectReportQuery>();
    private readonly IProjectReportExporterResolver _resolver = Substitute.For<IProjectReportExporterResolver>();
    private readonly GenerateProjectReportUseCase _sut;

    public GenerateProjectReportUseCaseTests()
    {
        _sut = new GenerateProjectReportUseCase(_reportQuery, _resolver);
    }

    [Fact]
    public async Task ExecuteAsync_DevuelveElContenidoYMetadatosDelExportador()
    {
        var contenido = new byte[] { 1, 2, 3 };
        ConfigurarReporte("AgileFlow", ConfigurarExportador("pdf", "application/pdf", "pdf", contenido));

        var archivo = await _sut.ExecuteAsync(Guid.NewGuid(), "pdf");

        archivo.Content.Should().BeEquivalentTo(contenido);
        archivo.ContentType.Should().Be("application/pdf");
        archivo.FileName.Should().EndWith(".pdf");
    }

    [Fact]
    public async Task ExecuteAsync_ReemplazaLosEspaciosDelNombreDelProyectoEnElArchivo()
    {
        ConfigurarReporte("Proyecto De Prueba", ConfigurarExportador("xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx", new byte[] { 9 }));

        var archivo = await _sut.ExecuteAsync(Guid.NewGuid(), "xlsx");

        archivo.FileName.Should().StartWith("reporte_Proyecto_De_Prueba_");
        archivo.FileName.Should().NotContain(" ");
    }

    [Fact]
    public async Task ExecuteAsync_ConsultaLosDatosUnaSolaVez()
    {
        var projectId = Guid.NewGuid();
        ConfigurarReporte("AgileFlow", ConfigurarExportador("pdf", "application/pdf", "pdf", new byte[] { 1 }));

        await _sut.ExecuteAsync(projectId, "pdf");

        await _reportQuery.Received(1).GetReportDataAsync(projectId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CuandoElProyectoNoExiste_LanzaNotFoundYNoResuelveExportador()
    {
        _reportQuery.GetReportDataAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((ProjectReportDto?)null);

        var act = () => _sut.ExecuteAsync(Guid.NewGuid(), "pdf");

        await act.Should().ThrowAsync<NotFoundException>();
        _resolver.DidNotReceive().Resolve(Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_ConFormatoNoSoportado_PropagaElErrorDelResolver()
    {
        ConfigurarReporte("AgileFlow", exportador: null);
        _resolver.Resolve("csv").Throws(new AppException("Formato de reporte no soportado: 'csv'."));

        var act = () => _sut.ExecuteAsync(Guid.NewGuid(), "csv");

        await act.Should().ThrowAsync<AppException>().WithMessage("*csv*");
    }

    private void ConfigurarReporte(string projectName, IProjectReportExporter? exportador)
    {
        var reporte = new ProjectReportDto(
            projectName,
            "descripción",
            new DateTime(2026, 8, 3, 10, 30, 0, DateTimeKind.Utc),
            new List<ProjectReportTaskRow>
            {
                new("Diseñar API", "En progreso", "Ana", "Alta")
            });

        _reportQuery.GetReportDataAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(reporte);

        if (exportador is not null)
            _resolver.Resolve(exportador.Format).Returns(exportador);
    }

    private static IProjectReportExporter ConfigurarExportador(
        string format, string contentType, string extension, byte[] contenido)
    {
        var exportador = Substitute.For<IProjectReportExporter>();
        exportador.Format.Returns(format);
        exportador.ContentType.Returns(contentType);
        exportador.FileExtension.Returns(extension);
        exportador.Export(Arg.Any<ProjectReportDto>()).Returns(contenido);
        return exportador;
    }
}

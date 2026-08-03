using AgileFlow.Application.Common;
using AgileFlow.Application.Ports;

namespace AgileFlow.Application.UseCases.Reports;

public record ProjectReportFile(byte[] Content, string ContentType, string FileName);

/// <summary>
/// Un solo caso de uso para ambos formatos: pide el DTO con UNA consulta
/// (IProjectReportQuery), y delega la transformación a bytes en el
/// exportador que corresponda.
/// </summary>
public class GenerateProjectReportUseCase
{
    private readonly IProjectReportQuery _reportQuery;
    private readonly IProjectReportExporterResolver _exporterResolver;

    public GenerateProjectReportUseCase(
        IProjectReportQuery reportQuery,
        IProjectReportExporterResolver exporterResolver)
    {
        _reportQuery = reportQuery;
        _exporterResolver = exporterResolver;
    }

    public async Task<ProjectReportFile> ExecuteAsync(Guid projectId, string format, CancellationToken ct = default)
    {
        var reportData = await _reportQuery.GetReportDataAsync(projectId, ct)
            ?? throw new NotFoundException("Proyecto", projectId);

        var exporter = _exporterResolver.Resolve(format);
        var content = exporter.Export(reportData);

        var safeName = reportData.ProjectName.Replace(' ', '_');
        var fileName = $"reporte_{safeName}_{reportData.GeneratedAtUtc:yyyyMMdd_HHmmss}.{exporter.FileExtension}";

        return new ProjectReportFile(content, exporter.ContentType, fileName);
    }
}

using AgileFlow.Application.Common;
using AgileFlow.Application.Ports;

namespace AgileFlow.Infrastructure.Reports;

public class ProjectReportExporterResolver : IProjectReportExporterResolver
{
    private readonly IReadOnlyDictionary<string, IProjectReportExporter> _exportersByFormat;

    public ProjectReportExporterResolver(IEnumerable<IProjectReportExporter> exporters)
    {
        _exportersByFormat = exporters.ToDictionary(e => e.Format, StringComparer.OrdinalIgnoreCase);
    }

    public IProjectReportExporter Resolve(string format)
    {
        if (_exportersByFormat.TryGetValue(format, out var exporter))
            return exporter;

        throw new AppException($"Formato de reporte no soportado: '{format}'.");
    }
}

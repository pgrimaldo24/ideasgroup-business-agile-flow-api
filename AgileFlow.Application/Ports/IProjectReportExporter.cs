using AgileFlow.Application.Dtos;

namespace AgileFlow.Application.Ports;

/// <summary>
/// Patrón Strategy: cada formato de salida (PDF, Excel, y cualquier futuro
/// formato) implementa este puerto de forma independiente. El caso de uso
/// que genera el reporte (GenerateProjectReportUseCase) no conoce QuestPDF
/// ni ClosedXML; solo pide "el exportador para el formato X" a
/// IProjectReportExporterResolver y le entrega el mismo ProjectReportDto.
///
/// Extensibilidad comprobable (req. 6.8): agregar un tercer formato (ej.
/// CSV) implica crear una nueva clase CsvProjectReportExporter : 
/// IProjectReportExporter y registrarla en DI — cero cambios en
/// PdfProjectReportExporter, ExcelProjectReportExporter, en el resolver
/// genérico, ni en el caso de uso.
/// </summary>
public interface IProjectReportExporter
{
    /// <summary>Identificador corto del formato: "pdf", "xlsx", etc.</summary>
    string Format { get; }
    string ContentType { get; }
    string FileExtension { get; }

    byte[] Export(ProjectReportDto report);
}

/// <summary>
/// Resuelve el exportador correcto en tiempo de ejecución a partir de la
/// colección de IProjectReportExporter registrada en DI (uno por formato).
/// El resolver es genérico y no necesita tocarse al agregar formatos nuevos.
/// </summary>
public interface IProjectReportExporterResolver
{
    IProjectReportExporter Resolve(string format);
}

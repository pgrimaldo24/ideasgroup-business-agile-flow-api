namespace AgileFlow.Application.Dtos;

/// <summary>
/// Estructura de transferencia tanto el exportador PDF como el Excel 
/// reciben exactamente este mismo objeto, construido a partir de una sola
/// consulta a base de datos.
/// </summary>
public record ProjectReportDto(
    string ProjectName,
    string? ProjectDescription,
    DateTime GeneratedAtUtc,
    IReadOnlyList<ProjectReportTaskRow> Tasks);

public record ProjectReportTaskRow(
    string TaskTitle,
    string ColumnName,
    string? AssigneeName,
    string Priority);

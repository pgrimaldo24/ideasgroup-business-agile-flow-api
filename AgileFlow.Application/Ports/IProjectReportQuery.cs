using AgileFlow.Application.Dtos;

namespace AgileFlow.Application.Ports;

/// <summary>
/// Puerto de UNA sola consulta a base de datos que arma el ProjectReportDto
/// completo (proyecto + tareas con columna/responsable/prioridad) via un
/// único JOIN proyectado directamente a DTO (sin N+1, sin traer entidades
/// de más). La implementación EF Core vive en Infrastructure. Tanto el
/// exportador PDF como el Excel consumen el resultado de esta única
/// consulta
/// </summary>
public interface IProjectReportQuery
{
    Task<ProjectReportDto?> GetReportDataAsync(Guid projectId, CancellationToken ct = default);
}

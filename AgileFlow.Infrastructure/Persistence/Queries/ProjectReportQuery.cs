using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using Microsoft.EntityFrameworkCore;

namespace AgileFlow.Infrastructure.Persistence.Queries;

/// <summary>
/// Implementa IProjectReportQuery con UNA sola consulta LINQ (un JOIN
/// proyecto → columnas → tareas, resuelto por EF Core en un solo SELECT con
/// JOINs en PostgreSQL) proyectada directamente a ProjectReportTaskRow, sin
/// materializar entidades completas. El mismo resultado alimenta tanto al
/// exportador PDF (QuestPDF) como al Excel (ClosedXML) — ver
/// GenerateProjectReportUseCase.
/// </summary>
public class ProjectReportQuery : IProjectReportQuery
{
    private readonly KanbanDbContext _context;

    public ProjectReportQuery(KanbanDbContext context) => _context = context;

    public async Task<ProjectReportDto?> GetReportDataAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await _context.Projects.AsNoTracking()
            .Where(p => p.Id == projectId)
            .Select(p => new { p.Name, p.Description })
            .FirstOrDefaultAsync(ct);

        if (project is null) return null;

        var rows = await (
            from task in _context.KanbanTasks.AsNoTracking()
            join column in _context.BoardColumns.AsNoTracking() on task.ColumnId equals column.Id
            where column.ProjectId == projectId
            orderby column.Position, task.Position
            select new ProjectReportTaskRow(
                task.Title,
                column.Name,
                task.AssigneeName,
                task.Priority.ToString())
        ).ToListAsync(ct);

        return new ProjectReportDto(project.Name, project.Description, DateTime.UtcNow, rows);
    }
}

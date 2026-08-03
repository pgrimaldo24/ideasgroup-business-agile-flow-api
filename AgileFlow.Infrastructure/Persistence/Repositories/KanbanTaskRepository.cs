using AgileFlow.Application.Ports;
using AgileFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgileFlow.Infrastructure.Persistence.Repositories;

public class KanbanTaskRepository : IKanbanTaskRepository
{
    private readonly KanbanDbContext _context;

    public KanbanTaskRepository(KanbanDbContext context) => _context = context;

    public Task<KanbanTask?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.KanbanTasks.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<KanbanTask>> GetByColumnIdAsync(Guid columnId, CancellationToken ct = default) =>
        await _context.KanbanTasks.AsNoTracking()
            .Where(t => t.ColumnId == columnId)
            .OrderBy(t => t.Position)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<KanbanTask>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        // Una sola consulta con JOIN a board_columns por project_id; evita
        // N+1 al pedir tareas columna por columna.
        var tasks = await _context.KanbanTasks.AsNoTracking()
            .Where(t => _context.BoardColumns.Any(c => c.Id == t.ColumnId && c.ProjectId == projectId))
            .OrderBy(t => t.ColumnId).ThenBy(t => t.Position)
            .ToListAsync(ct);

        return tasks;
    }

    /// <summary>
    /// Trae, en UNA sola consulta (LIMIT 2 con OFFSET), la tarea inmediatamente
    /// anterior y la inmediatamente siguiente a la posición destino dentro de
    /// una columna, excluyendo la propia tarea que se está moviendo. Se apoya
    /// en el índice (column_id, position) — ver KanbanTaskConfiguration.
    /// </summary>
    public async Task<(KanbanTask? Previous, KanbanTask? Next)> GetNeighborsAsync(
        Guid columnId, int targetIndex, Guid? excludeTaskId, CancellationToken ct = default)
    {
        var orderedQuery = _context.KanbanTasks.AsNoTracking()
            .Where(t => t.ColumnId == columnId && (excludeTaskId == null || t.Id != excludeTaskId))
            .OrderBy(t => t.Position);

        var skip = Math.Max(targetIndex - 1, 0);
        var window = await orderedQuery.Skip(skip).Take(2).ToListAsync(ct);

        if (targetIndex <= 0)
            return (null, window.ElementAtOrDefault(0));

        return (window.ElementAtOrDefault(0), window.ElementAtOrDefault(1));
    }

    public async Task AddAsync(KanbanTask task, CancellationToken ct = default) =>
        await _context.KanbanTasks.AddAsync(task, ct);

    public void Remove(KanbanTask task) => _context.KanbanTasks.Remove(task);
}

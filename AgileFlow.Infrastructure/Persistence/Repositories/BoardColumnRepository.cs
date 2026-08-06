using AgileFlow.Application.Ports;
using AgileFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgileFlow.Infrastructure.Persistence.Repositories;

public class BoardColumnRepository : IBoardColumnRepository
{
    private readonly KanbanDbContext _context;

    public BoardColumnRepository(KanbanDbContext context) => _context = context;

    public Task<BoardColumn?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.BoardColumns.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<BoardColumn>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default) =>
        await _context.BoardColumns.AsNoTracking()
            .Where(c => c.ProjectId == projectId)
            .OrderBy(c => c.Position)
            .ToListAsync(ct);

    public Task<bool> HasTasksAsync(Guid columnId, CancellationToken ct = default) =>
        _context.KanbanTasks.AnyAsync(t => t.ColumnId == columnId, ct);

    public Task<decimal?> GetLastPositionAsync(Guid projectId, CancellationToken ct = default) =>
        _context.BoardColumns.AsNoTracking()
            .Where(c => c.ProjectId == projectId)
            .OrderByDescending(c => c.Position)
            .Select(c => (decimal?)c.Position)
            .FirstOrDefaultAsync(ct);

    public async Task<(BoardColumn? Previous, BoardColumn? Next)> GetNeighborsAsync(
        Guid projectId, int targetIndex, Guid excludeColumnId, CancellationToken ct = default)
    {
        var orderedQuery = _context.BoardColumns.AsNoTracking()
            .Where(c => c.ProjectId == projectId && c.Id != excludeColumnId)
            .OrderBy(c => c.Position);

        var skip = Math.Max(targetIndex - 1, 0);
        var window = await orderedQuery.Skip(skip).Take(2).ToListAsync(ct);

        if (targetIndex <= 0)
            return (null, window.ElementAtOrDefault(0));

        return (window.ElementAtOrDefault(0), window.ElementAtOrDefault(1));
    }

    public async Task AddAsync(BoardColumn column, CancellationToken ct = default) =>
        await _context.BoardColumns.AddAsync(column, ct);

    public void Remove(BoardColumn column) => _context.BoardColumns.Remove(column);
}

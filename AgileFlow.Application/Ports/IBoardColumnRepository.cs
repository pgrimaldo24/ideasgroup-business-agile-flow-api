using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.Ports;

public interface IBoardColumnRepository
{
    Task<BoardColumn?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<BoardColumn>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task<bool> HasTasksAsync(Guid columnId, CancellationToken ct = default);
    Task<decimal?> GetLastPositionAsync(Guid projectId, CancellationToken ct = default);
    Task<(BoardColumn? Previous, BoardColumn? Next)> GetNeighborsAsync(
        Guid projectId, int targetIndex, Guid excludeColumnId, CancellationToken ct = default);
    Task AddAsync(BoardColumn column, CancellationToken ct = default);
    void Remove(BoardColumn column);
}

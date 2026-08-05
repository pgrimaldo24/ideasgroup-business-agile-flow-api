using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.Ports;

public interface IKanbanTaskRepository
{
    Task<KanbanTask?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<KanbanTask>> GetByColumnIdAsync(Guid columnId, CancellationToken ct = default);
    Task<IReadOnlyList<KanbanTask>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task<(KanbanTask? Previous, KanbanTask? Next)> GetNeighborsAsync(
        Guid columnId, int targetIndex, Guid? excludeTaskId, CancellationToken ct = default);
    Task<decimal?> GetLastPositionAsync(Guid columnId, CancellationToken ct = default);
    Task AddAsync(KanbanTask task, CancellationToken ct = default);
    void Remove(KanbanTask task);
}

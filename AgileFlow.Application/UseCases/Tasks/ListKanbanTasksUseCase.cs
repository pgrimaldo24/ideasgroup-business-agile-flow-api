using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.UseCases.Tasks;

public class ListKanbanTasksUseCase
{
    private readonly IKanbanTaskRepository _taskRepository;

    public ListKanbanTasksUseCase(IKanbanTaskRepository taskRepository) => _taskRepository = taskRepository;

    public async Task<IReadOnlyList<KanbanTaskDto>> ExecuteAsync(Guid columnId, CancellationToken ct = default)
    {
        var tasks = await _taskRepository.GetByColumnIdAsync(columnId, ct);

        return tasks.Select(ToDto).ToList();
    }

    private static KanbanTaskDto ToDto(KanbanTask task) => new(
        task.Id, task.Title, task.Description, task.Priority.ToString(),
        task.AssigneeName, task.ColumnId, task.Position, task.CreatedAtUtc);
}

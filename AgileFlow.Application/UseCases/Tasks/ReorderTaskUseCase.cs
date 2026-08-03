using AgileFlow.Application.Common;
using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using AgileFlow.Domain.Services;

namespace AgileFlow.Application.UseCases.Tasks;

public record ReorderTaskCommand(Guid TaskId, Guid TargetColumnId, int TargetIndex);

/// <summary>
/// Orquesta el reordenamiento de una tarea: pide a IKanbanTaskRepository
/// los vecinos en la posición destino, delega en el servicio de dominio
/// TaskOrderingService el cálculo de la nueva posición fraccional, aplica
/// el cambio sobre la entidad, persiste y notifica por el canal de tiempo
/// real.
/// </summary>
public class ReorderTaskUseCase
{
    private readonly IKanbanTaskRepository _taskRepository;
    private readonly IBoardColumnRepository _columnRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBoardRealtimeNotifier _realtimeNotifier;

    public ReorderTaskUseCase(
        IKanbanTaskRepository taskRepository,
        IBoardColumnRepository columnRepository,
        IUnitOfWork unitOfWork,
        IBoardRealtimeNotifier realtimeNotifier)
    {
        _taskRepository = taskRepository;
        _columnRepository = columnRepository;
        _unitOfWork = unitOfWork;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<KanbanTaskDto> ExecuteAsync(ReorderTaskCommand command, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(command.TaskId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.KanbanTask), command.TaskId);

        var targetColumn = await _columnRepository.GetByIdAsync(command.TargetColumnId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.BoardColumn), command.TargetColumnId);

        var (previous, next) = await _taskRepository.GetNeighborsAsync(
            targetColumn.Id, command.TargetIndex, excludeTaskId: task.Id, ct);

        var newPosition = TaskOrderingService.CalculateNewPosition(previous?.Position, next?.Position);

        task.MoveTo(targetColumn.Id, newPosition);

        await _unitOfWork.CommitAsync(ct);

        var dto = ToDto(task);
        await _realtimeNotifier.TaskMovedAsync(targetColumn.ProjectId, dto, ct);

        return dto;
    }

    private static KanbanTaskDto ToDto(Domain.Entities.KanbanTask task) => new(
        task.Id, task.Title, task.Description, task.Priority.ToString(),
        task.AssigneeName, task.ColumnId, task.Position, task.CreatedAtUtc);
}

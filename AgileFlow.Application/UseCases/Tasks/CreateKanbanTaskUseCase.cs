using AgileFlow.Application.Common;
using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using AgileFlow.Domain.Entities;
using AgileFlow.Domain.Enums;
using AgileFlow.Domain.Services;

namespace AgileFlow.Application.UseCases.Tasks;

public record CreateKanbanTaskCommand(
    Guid ColumnId,
    string Title,
    string? Description,
    string Priority,
    string? AssigneeName);

public class CreateKanbanTaskUseCase
{
    private readonly IKanbanTaskRepository _taskRepository;
    private readonly IBoardColumnRepository _columnRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBoardRealtimeNotifier _realtimeNotifier;

    public CreateKanbanTaskUseCase(
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

    public async Task<KanbanTaskDto> ExecuteAsync(CreateKanbanTaskCommand command, CancellationToken ct = default)
    {
        var column = await _columnRepository.GetByIdAsync(command.ColumnId, ct)
            ?? throw new NotFoundException(nameof(BoardColumn), command.ColumnId);

        if (!Enum.TryParse<TaskPriority>(command.Priority, ignoreCase: true, out var priority))
        {
            throw new AppException(
                $"Prioridad no válida: '{command.Priority}'. Valores admitidos: " +
                string.Join(", ", Enum.GetNames<TaskPriority>()) + ".");
        }

        var lastPosition = await _taskRepository.GetLastPositionAsync(column.Id, ct);
        var position = TaskOrderingService.CalculateNewPosition(lastPosition, null);

        var task = new KanbanTask(
            command.Title, command.Description, priority, command.AssigneeName, column.Id, position);

        await _taskRepository.AddAsync(task, ct);
        await _unitOfWork.CommitAsync(ct);

        var dto = ToDto(task);
        await _realtimeNotifier.TaskCreatedAsync(column.ProjectId, dto, ct);

        return dto;
    }

    private static KanbanTaskDto ToDto(KanbanTask task) => new(
        task.Id, task.Title, task.Description, task.Priority.ToString(),
        task.AssigneeName, task.ColumnId, task.Position, task.CreatedAtUtc);
}

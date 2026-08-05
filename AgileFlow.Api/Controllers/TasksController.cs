using AgileFlow.Application.Dtos;
using AgileFlow.Application.UseCases.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileFlow.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ReorderTaskUseCase _reorderTaskUseCase;
    private readonly ListKanbanTasksUseCase _listKanbanTasksUseCase;
    private readonly CreateKanbanTaskUseCase _createKanbanTaskUseCase;

    public TasksController(
        ReorderTaskUseCase reorderTaskUseCase,
        ListKanbanTasksUseCase listKanbanTasksUseCase,
        CreateKanbanTaskUseCase createKanbanTaskUseCase)
    {
        _reorderTaskUseCase = reorderTaskUseCase;
        _listKanbanTasksUseCase = listKanbanTasksUseCase;
        _createKanbanTaskUseCase = createKanbanTaskUseCase;
    }

    [HttpGet("/api/columns/{columnId:guid}/tasks")]
    public async Task<ActionResult<IReadOnlyList<KanbanTaskDto>>> ListByColumn(Guid columnId, CancellationToken ct)
    {
        var tasks = await _listKanbanTasksUseCase.ExecuteAsync(columnId, ct);
        return Ok(tasks);
    }

    [HttpPost("/api/columns/{columnId:guid}/tasks")]
    public async Task<ActionResult<KanbanTaskDto>> Create(
        Guid columnId, [FromBody] CreateKanbanTaskRequest request, CancellationToken ct)
    {
        var command = new CreateKanbanTaskCommand(
            columnId, request.Title, request.Description, request.Priority, request.AssigneeName);

        var task = await _createKanbanTaskUseCase.ExecuteAsync(command, ct);

        return StatusCode(StatusCodes.Status201Created, task);
    }

    /// <summary>
    /// Mueve/reordena una tarea (drag&drop, dentro de la misma columna o hacia otra).hacia otra).
    /// </summary>
    [HttpPatch("{taskId:guid}/reorder")]
    public async Task<IActionResult> Reorder(Guid taskId, [FromBody] ReorderTaskRequest request, CancellationToken ct)
    {
        var command = new ReorderTaskCommand(taskId, request.TargetColumnId, request.TargetIndex);
        var result = await _reorderTaskUseCase.ExecuteAsync(command, ct);
        return Ok(result);
    }
}

public record ReorderTaskRequest(Guid TargetColumnId, int TargetIndex);

public record CreateKanbanTaskRequest(
    string Title,
    string? Description,
    string Priority,
    string? AssigneeName);

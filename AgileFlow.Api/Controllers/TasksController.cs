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

    public TasksController(ReorderTaskUseCase reorderTaskUseCase) => _reorderTaskUseCase = reorderTaskUseCase;

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

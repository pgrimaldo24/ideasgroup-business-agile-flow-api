using AgileFlow.Application.Dtos;
using AgileFlow.Application.UseCases.Columns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileFlow.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/columns")]
[Authorize]
public class ColumnsController : ControllerBase
{
    private readonly ListBoardColumnsUseCase _listBoardColumnsUseCase;
    private readonly CreateBoardColumnUseCase _createBoardColumnUseCase;
    private readonly UpdateBoardColumnUseCase _updateBoardColumnUseCase;
    private readonly DeleteBoardColumnUseCase _deleteBoardColumnUseCase;
    private readonly ReorderBoardColumnUseCase _reorderBoardColumnUseCase;

    public ColumnsController(
        ListBoardColumnsUseCase listBoardColumnsUseCase,
        CreateBoardColumnUseCase createBoardColumnUseCase,
        UpdateBoardColumnUseCase updateBoardColumnUseCase,
        DeleteBoardColumnUseCase deleteBoardColumnUseCase,
        ReorderBoardColumnUseCase reorderBoardColumnUseCase)
    {
        _listBoardColumnsUseCase = listBoardColumnsUseCase;
        _createBoardColumnUseCase = createBoardColumnUseCase;
        _updateBoardColumnUseCase = updateBoardColumnUseCase;
        _deleteBoardColumnUseCase = deleteBoardColumnUseCase;
        _reorderBoardColumnUseCase = reorderBoardColumnUseCase;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BoardColumnDto>>> List(Guid projectId, CancellationToken ct)
    {
        var columns = await _listBoardColumnsUseCase.ExecuteAsync(projectId, ct);
        return Ok(columns);
    }

    [HttpPost]
    public async Task<ActionResult<BoardColumnDto>> Create(
        Guid projectId, [FromBody] CreateBoardColumnRequest request, CancellationToken ct)
    {
        var command = new CreateBoardColumnCommand(projectId, request.Name);
        var column = await _createBoardColumnUseCase.ExecuteAsync(command, ct);

        return StatusCode(StatusCodes.Status201Created, column);
    }

    [HttpPut("/api/columns/{columnId:guid}")]
    public async Task<ActionResult<BoardColumnDto>> Update(
        Guid columnId, [FromBody] UpdateBoardColumnRequest request, CancellationToken ct)
    {
        var command = new UpdateBoardColumnCommand(columnId, request.Name);
        var column = await _updateBoardColumnUseCase.ExecuteAsync(command, ct);

        return Ok(column);
    }

    [HttpDelete("/api/columns/{columnId:guid}")]
    public async Task<IActionResult> Delete(Guid columnId, CancellationToken ct)
    {
        await _deleteBoardColumnUseCase.ExecuteAsync(new DeleteBoardColumnCommand(columnId), ct);

        return NoContent();
    }

    [HttpPatch("/api/columns/{columnId:guid}/reorder")]
    public async Task<ActionResult<BoardColumnDto>> Reorder(
        Guid columnId, [FromBody] ReorderBoardColumnRequest request, CancellationToken ct)
    {
        var command = new ReorderBoardColumnCommand(columnId, request.TargetIndex);
        var column = await _reorderBoardColumnUseCase.ExecuteAsync(command, ct);

        return Ok(column);
    }
}

public record CreateBoardColumnRequest(string Name);
public record UpdateBoardColumnRequest(string Name);
public record ReorderBoardColumnRequest(int TargetIndex);

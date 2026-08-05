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

    public ColumnsController(
        ListBoardColumnsUseCase listBoardColumnsUseCase,
        CreateBoardColumnUseCase createBoardColumnUseCase)
    {
        _listBoardColumnsUseCase = listBoardColumnsUseCase;
        _createBoardColumnUseCase = createBoardColumnUseCase;
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
}

public record CreateBoardColumnRequest(string Name);

using AgileFlow.Application.Dtos;
using AgileFlow.Application.UseCases.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileFlow.Api.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly ListProjectsUseCase _listProjectsUseCase;
    private readonly CreateProjectUseCase _createProjectUseCase;

    public ProjectsController(
        ListProjectsUseCase listProjectsUseCase,
        CreateProjectUseCase createProjectUseCase)
    {
        _listProjectsUseCase = listProjectsUseCase;
        _createProjectUseCase = createProjectUseCase;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProjectDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? name = null,
        CancellationToken ct = default)
    {
        var result = await _listProjectsUseCase.ExecuteAsync(new ListProjectsQuery(page, pageSize, name), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(
        [FromBody] CreateProjectRequest request, CancellationToken ct)
    {
        var command = new CreateProjectCommand(
            request.Name, request.Description, request.StartDate, request.ExpectedEndDate, request.Status);

        var project = await _createProjectUseCase.ExecuteAsync(command, ct);

        return Created($"/api/projects/{project.Id}", project);
    }
}

public record CreateProjectRequest(
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime ExpectedEndDate,
    string Status);

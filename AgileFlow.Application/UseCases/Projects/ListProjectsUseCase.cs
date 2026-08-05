using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.UseCases.Projects;

public record ListProjectsQuery(int Page, int PageSize, string? Name);

public class ListProjectsUseCase
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IProjectRepository _projectRepository;

    public ListProjectsUseCase(IProjectRepository projectRepository) => _projectRepository = projectRepository;

    public async Task<PagedResult<ProjectDto>> ExecuteAsync(ListProjectsQuery query, CancellationToken ct = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);

        var (items, totalCount) = await _projectRepository.GetPagedAsync(page, pageSize, query.Name, ct);

        return new PagedResult<ProjectDto>(items.Select(ToDto).ToList(), page, pageSize, totalCount);
    }

    private static ProjectDto ToDto(Project project) => new(
        project.Id, project.Name, project.Description, project.StartDate,
        project.ExpectedEndDate, project.Status.ToString());
}

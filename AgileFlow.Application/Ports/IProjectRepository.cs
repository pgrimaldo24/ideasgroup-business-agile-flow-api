using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.Ports;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Project> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? nameFilter, CancellationToken ct = default);

    Task AddAsync(Project project, CancellationToken ct = default);
    void Remove(Project project);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}

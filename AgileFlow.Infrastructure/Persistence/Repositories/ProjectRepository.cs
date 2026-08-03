using AgileFlow.Application.Ports;
using AgileFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgileFlow.Infrastructure.Persistence.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly KanbanDbContext _context;

    public ProjectRepository(KanbanDbContext context) => _context = context;

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Projects.Include(p => p.Columns).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(IReadOnlyList<Project> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? nameFilter, CancellationToken ct = default)
    {
        var query = _context.Projects.AsNoTracking().AsQueryable();

        // Filtro por coincidencia parcial resuelto en el servidor (traducido
        // a ILIKE '%...%' por el proveedor de PostgreSQL, no en memoria).
        if (!string.IsNullOrWhiteSpace(nameFilter))
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{nameFilter.Trim()}%"));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(Project project, CancellationToken ct = default) =>
        await _context.Projects.AddAsync(project, ct);

    public void Remove(Project project) => _context.Projects.Remove(project);

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        _context.Projects.AnyAsync(p => p.Id == id, ct);
}

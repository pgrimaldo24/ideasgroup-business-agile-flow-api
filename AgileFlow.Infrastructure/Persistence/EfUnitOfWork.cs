using AgileFlow.Application.Ports;

namespace AgileFlow.Infrastructure.Persistence;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly KanbanDbContext _context;

    public EfUnitOfWork(KanbanDbContext context) => _context = context;

    public Task<int> CommitAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}

using AgileFlow.Application.Ports;
using AgileFlow.Domain.Entities;
using AgileFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileFlow.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly KanbanDbContext _context;

    public UserRepository(KanbanDbContext context) => _context = context;

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
}

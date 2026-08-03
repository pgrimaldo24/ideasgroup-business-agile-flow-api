using AgileFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgileFlow.Infrastructure.Persistence;

public class KanbanDbContext : DbContext
{
    public KanbanDbContext(DbContextOptions<KanbanDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<BoardColumn> BoardColumns => Set<BoardColumn>();
    public DbSet<KanbanTask> KanbanTasks => Set<KanbanTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KanbanDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

using AgileFlow.Application.Ports;
using AgileFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgileFlow.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(
        KanbanDbContext context,
        IPasswordHasher passwordHasher,
        SeedOptions options,
        CancellationToken ct = default)
    {
        if (options.Users.Count == 0) return;

        var added = false;

        foreach (var configuredUser in options.Users)
        {
            if (string.IsNullOrWhiteSpace(configuredUser.Email) ||
                string.IsNullOrWhiteSpace(configuredUser.Password))
            {
                throw new InvalidOperationException("Configuración de usuarios semilla inválida.");
            }
             
            var email = configuredUser.Email.Trim().ToLowerInvariant();
             
            if (await context.Users.AnyAsync(u => u.Email == email, ct)) continue;

            var (hash, salt) = passwordHasher.HashPassword(configuredUser.Password);

            context.Users.Add(new User(configuredUser.FullName, configuredUser.Email, hash, salt));
            added = true;
        }

        if (added) await context.SaveChangesAsync(ct);
    }
}

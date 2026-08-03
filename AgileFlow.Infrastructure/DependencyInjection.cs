using AgileFlow.Application.Ports;
using AgileFlow.Application.UseCases.Auth;
using AgileFlow.Application.UseCases.Reports;
using AgileFlow.Application.UseCases.Tasks;
using AgileFlow.Infrastructure.Persistence;
using AgileFlow.Infrastructure.Persistence.Queries;
using AgileFlow.Infrastructure.Persistence.Repositories;
using AgileFlow.Infrastructure.Realtime;
using AgileFlow.Infrastructure.Reports;
using AgileFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgileFlow.Infrastructure;

/// <summary>
/// Punto único donde se conectan los puertos (Application) con sus
/// adaptadores concretos (Infrastructure). AgileFlow.Api solo llama a
/// AddInfrastructure(configuration) — no conoce EF Core, Npgsql, SignalR,
/// QuestPDF ni ClosedXML directamente.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ConnectionStrings:Default"]
            ?? throw new InvalidOperationException(
                "Falta configurar ConnectionStrings:Default (variable de entorno CONNECTION_STRING).");

        services.AddDbContext<KanbanDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IBoardColumnRepository, BoardColumnRepository>();
        services.AddScoped<IKanbanTaskRepository, KanbanTaskRepository>();
        services.AddScoped<IProjectReportQuery, ProjectReportQuery>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IBoardRealtimeNotifier, SignalRBoardRealtimeNotifier>();

        // Exportadores de reporte: cada uno se registra como IProjectReportExporter;
        // el resolver los recibe todos vía IEnumerable<IProjectReportExporter>.
        services.AddScoped<IProjectReportExporter, PdfProjectReportExporter>();
        services.AddScoped<IProjectReportExporter, ExcelProjectReportExporter>();
        services.AddScoped<IProjectReportExporterResolver, ProjectReportExporterResolver>();

        // Casos de uso de Application (se registran aquí para mantener
        // AgileFlow.Api libre de referencias a Application más allá de los DTOs).
        services.AddScoped<LoginUseCase>();
        services.AddScoped<ReorderTaskUseCase>();
        services.AddScoped<GenerateProjectReportUseCase>();

        return services;
    }
}

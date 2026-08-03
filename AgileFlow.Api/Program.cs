using System.Text;
using AgileFlow.Api.Middleware;
using AgileFlow.Infrastructure;
using AgileFlow.Infrastructure.Persistence;
using AgileFlow.Infrastructure.Persistence.Seed;
using AgileFlow.Infrastructure.Realtime;
using AgileFlow.Application.Ports;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// Configuración externa: todos los valores sensibles llegan por variables
// de entorno (convención ASP.NET Core: "__" = separador de sección, ej.
// ConnectionStrings__Default, Jwt__Secret, Security__PasswordPepper).
// Ver docker-compose.yml / .env.example en la raíz del repositorio.
// Nada de esto está hardcodeado ni versionado (req. 6.1).
// -----------------------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Kanban API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresar: Bearer {token}"
    });

    options.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSignalR();

var frontendOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:4200";
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(frontendOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); // necesario para SignalR
});

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Falta Jwt__Secret (variable de entorno JWT_SECRET).");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Kanban.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Kanban.Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // SignalR no puede enviar el header Authorization en la conexión
        // WebSocket del navegador: el cliente manda el JWT como querystring
        // (?access_token=...) y aquí se reubica al pipeline de auth estándar.
        // Así el hub queda "autenticado con el mismo token de sesión" (req. 6.2)
        // sin duplicar lógica de validación.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<BoardHub>("/hubs/board");

// Construir la base de datos desde cero ejecutando las migraciones en orden
// (req. 6.1), y sembrar los usuarios precargados justo después.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<KanbanDbContext>();
    await dbContext.Database.MigrateAsync();

    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbSeeder.SeedAsync(dbContext, passwordHasher);
}

app.Run();

// Necesario para que WebApplicationFactory<Program> funcione en pruebas de integración.
public partial class Program { }

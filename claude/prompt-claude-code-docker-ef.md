# Contexto del proyecto

Es una solución .NET 8 con arquitectura hexagonal para una prueba técnica
(gestión de proyectos ágiles / tablero Kanban). Estructura de carpetas:

```
AgileFlow/
├── backend/
│   ├── AgileFlow.sln
│   ├── src/
│   │   ├── AgileFlow.Domain/          (sin dependencias externas)
│   │   ├── AgileFlow.Application/      (puertos + casos de uso, referencia Domain)
│   │   ├── AgileFlow.Infrastructure/   (EF Core, PostgreSQL, JWT, SignalR, QuestPDF, ClosedXML)
│   │   └── AgileFlow.Api/              (ASP.NET Core, referencia Application + Infrastructure)
│   └── tests/
│       └── AgileFlow.Application.Tests/
└── frontend/            (Angular 17 + PrimeNG/Sakai — aún no configurado, ignorar por ahora)
```

- `AgileFlow.Infrastructure/Persistence/KanbanDbContext.cs` (o el nombre
  equivalente tras el rename) ya tiene el `DbContext` con `DbSet` de Users,
  Projects, BoardColumns, KanbanTasks, y sus `IEntityTypeConfiguration<T>`
  en `Persistence/Configurations/`.
- `AgileFlow.Infrastructure/DependencyInjection.cs` ya registra
  `AddDbContext<...>(options => options.UseNpgsql(connectionString))`
  leyendo `ConnectionStrings:Default` desde `IConfiguration`.
- `Program.cs` ya llama a `dbContext.Database.MigrateAsync()` al arrancar,
  seguido de un seed de usuarios (`DbSeeder.SeedAsync`).
- Todavía **no existe ninguna migración de EF Core generada** (la carpeta
  `Migrations/` no existe aún dentro de Infrastructure).
- No hay `docker-compose.yml` en la raíz del repo todavía.

# Lo que necesito que hagas

## 1. Generar la migración inicial de EF Core
- Verifica que `AgileFlow.Infrastructure.csproj` tenga los paquetes
  `Npgsql.EntityFrameworkCore.PostgreSQL` y `Microsoft.EntityFrameworkCore.Design`.
- Corre `dotnet ef migrations add InitialCreate` apuntando
  `--project src/AgileFlow.Infrastructure --startup-project src/AgileFlow.Api`.
- Si falla por falta de `ConnectionStrings:Default` en tiempo de diseño,
  agrega un `IDesignTimeDbContextFactory<TuDbContext>` en Infrastructure
  que use una cadena de conexión de PostgreSQL local por defecto SOLO para
  el diseño de migraciones (no debe usarse en runtime).
- Confirma que la migración generada crea correctamente las tablas
  `users`, `projects`, `board_columns`, `kanban_tasks` con sus índices
  (unique en `users.email`, compuesto en `(project_id, position)` de
  columnas y `(column_id, position)` de tareas).

## 2. Crear `docker-compose.yml` en la raíz del repositorio
Debe levantar 3 servicios:
- **postgres**: imagen `postgres:16-alpine`, con volumen nombrado para
  persistencia, variables `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`
  leídas desde `.env` (no hardcodeadas en el compose), healthcheck con
  `pg_isready`.
- **api**: build desde `backend/src/AgileFlow.Api/Dockerfile` (ya existe,
  revísalo y ajústalo si el rename de Kanban→AgileFlow dejó algo
  desactualizado), variables de entorno usando la convención de ASP.NET
  Core con doble guion bajo:
  - `ConnectionStrings__Default=Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}`
  - `Jwt__Secret=${JWT_SECRET}`
  - `Jwt__Issuer=${JWT_ISSUER}`
  - `Jwt__Audience=${JWT_AUDIENCE}`
  - `Security__PasswordPepper=${PASSWORD_PEPPER}`
  - `Cors__AllowedOrigin=${FRONTEND_URL}`
  `depends_on: postgres` con `condition: service_healthy`, puerto expuesto
  `8080:8080` (o el que use el Dockerfile).
- **frontend**: si ya existe `frontend/Dockerfile` (nginx sirviendo el
  build de Angular), agrégalo también; si no existe todavía, deja el
  servicio comentado con un TODO y avísame — no lo inventes sin ver el
  proyecto Angular real.

## 3. Crear `.env.example` en la raíz, con TODAS las variables anteriores
llenas por defecto con valores de desarrollo funcionales (no vacíos), para
que el proyecto levante con `docker compose up` sin configuración manual
adicional. Ejemplo del tipo de valores que espero (ajusta a lo real):
```
POSTGRES_DB=agileflow
POSTGRES_USER=agileflow_user
POSTGRES_PASSWORD=agileflow_dev_password
JWT_SECRET=una-clave-larga-aleatoria-de-desarrollo-cambiar-en-produccion
JWT_ISSUER=AgileFlow.Api
JWT_AUDIENCE=AgileFlow.Client
PASSWORD_PEPPER=otro-secreto-de-desarrollo-cambiar-en-produccion
FRONTEND_URL=http://localhost:4200
```

## 4. Verificar `.gitignore`
Confirma que `.env` (sin `.example`), `bin/`, `obj/`, `.vs/` y cualquier
`appsettings.*.Local.json` estén ignorados — no debe quedar ningún secreto
versionado (requisito explícito de la prueba técnica).

## 5. Probar que todo levanta
- Copia `.env.example` a `.env`.
- Corre `docker compose up --build`.
- Confirma en los logs que: Postgres pasa el healthcheck, la API aplica
  las migraciones automáticamente al arrancar (`MigrateAsync`), el seed
  de usuarios se ejecuta sin duplicarse en reinicios, y la API queda
  escuchando y responde en `http://localhost:8080/swagger` (si Swagger
  sigue habilitado) o al menos en un endpoint de salud.
- Si algo falla, arréglalo iterando — no me devuelvas el prompt sin haber
  verificado que `docker compose up` realmente deja el sistema arriba.

## Restricciones importantes
- Mantén la arquitectura hexagonal: Infrastructure no debe filtrar
  detalles de EF Core hacia Application ni Domain.
- No hardcodees ninguna cadena de conexión ni secreto fuera de `.env` /
  `.env.example`.
- No modifiques la lógica de negocio existente (entidades, casos de uso,
  `TaskOrderingService`) — este trabajo es solo de infraestructura/DevOps.
- Al terminar, dime exactamente qué comandos debo correr yo para repetir
  el levantamiento desde cero en otra máquina (para poner en el README).

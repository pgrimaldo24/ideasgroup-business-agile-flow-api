FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AgileFlowApi.sln .
COPY src/AgileFlow.Domain/Kanban.Domain.csproj src/Kanban.Domain/
COPY src/AgileFlow.Application/Kanban.Application.csproj src/Kanban.Application/
COPY src/AgileFlow.Infrastructure/Kanban.Infrastructure.csproj src/Kanban.Infrastructure/
COPY src/AgileFlow.Api/Kanban.Api.csproj src/Kanban.Api/
COPY tests/AgileFlow.Application.Tests/Kanban.Application.Tests.csproj tests/Kanban.Application.Tests/

RUN dotnet restore src/Kanban.Api/Kanban.Api.csproj

COPY . .
RUN dotnet publish src/Kanban.Api/Kanban.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AgileFlow.Api.dll"]

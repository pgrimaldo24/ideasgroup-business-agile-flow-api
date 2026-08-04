# Build desde la RAÍZ del repositorio:
#   docker build -f AgileFlow.Api/Dockerfile .
# (docker-compose.yml ya lo hace con context: . )
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar primero solo los .csproj para aprovechar la caché de capas en restore.
COPY AgileFlowApi.sln .
COPY AgileFlow.Domain/AgileFlow.Domain.csproj AgileFlow.Domain/
COPY AgileFlow.Application/AgileFlow.Application.csproj AgileFlow.Application/
COPY AgileFlow.Infrastructure/AgileFlow.Infrastructure.csproj AgileFlow.Infrastructure/
COPY AgileFlow.Api/AgileFlow.Api.csproj AgileFlow.Api/

RUN dotnet restore AgileFlow.Api/AgileFlow.Api.csproj

COPY . .
RUN dotnet publish AgileFlow.Api/AgileFlow.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AgileFlow.Api.dll"]

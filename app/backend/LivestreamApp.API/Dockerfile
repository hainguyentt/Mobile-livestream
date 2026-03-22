# Multi-stage build for LivestreamApp.API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY app/backend/Directory.Build.props app/backend/
COPY app/backend/LivestreamApp.Shared/LivestreamApp.Shared.csproj app/backend/LivestreamApp.Shared/
COPY app/backend/LivestreamApp.Auth/LivestreamApp.Auth.csproj app/backend/LivestreamApp.Auth/
COPY app/backend/LivestreamApp.Profiles/LivestreamApp.Profiles.csproj app/backend/LivestreamApp.Profiles/
COPY app/backend/LivestreamApp.API/LivestreamApp.API.csproj app/backend/LivestreamApp.API/
RUN dotnet restore app/backend/LivestreamApp.API/LivestreamApp.API.csproj

# Copy source and build
COPY app/backend/ app/backend/
RUN dotnet publish app/backend/LivestreamApp.API/LivestreamApp.API.csproj \
    -c Release -o /app/publish --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

# Run as non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LivestreamApp.API.dll"]

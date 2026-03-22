# Multi-stage build for LivestreamApp.MockServices
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY app/mock/LivestreamApp.MockServices/LivestreamApp.MockServices.csproj app/mock/LivestreamApp.MockServices/
RUN dotnet restore app/mock/LivestreamApp.MockServices/LivestreamApp.MockServices.csproj

COPY app/mock/ app/mock/
RUN dotnet publish app/mock/LivestreamApp.MockServices/LivestreamApp.MockServices.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LivestreamApp.MockServices.dll"]

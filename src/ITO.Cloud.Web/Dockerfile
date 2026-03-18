# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ITO.Cloud.Web/ITO.Cloud.Web.csproj",           "src/ITO.Cloud.Web/"]
COPY ["src/ITO.Cloud.Application/ITO.Cloud.Application.csproj", "src/ITO.Cloud.Application/"]
COPY ["src/ITO.Cloud.Infrastructure/ITO.Cloud.Infrastructure.csproj", "src/ITO.Cloud.Infrastructure/"]
COPY ["src/ITO.Cloud.Domain/ITO.Cloud.Domain.csproj",     "src/ITO.Cloud.Domain/"]

RUN dotnet restore "src/ITO.Cloud.Web/ITO.Cloud.Web.csproj"

COPY . .

RUN dotnet publish "src/ITO.Cloud.Web/ITO.Cloud.Web.csproj" \
    -c Release -o /app/publish --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8081
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8081

ENTRYPOINT ["dotnet", "ITO.Cloud.Web.dll"]

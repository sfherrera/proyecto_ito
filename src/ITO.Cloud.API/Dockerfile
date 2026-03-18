# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos de proyecto primero (mejor cache de capas)
COPY ["src/ITO.Cloud.API/ITO.Cloud.API.csproj",           "src/ITO.Cloud.API/"]
COPY ["src/ITO.Cloud.Application/ITO.Cloud.Application.csproj", "src/ITO.Cloud.Application/"]
COPY ["src/ITO.Cloud.Infrastructure/ITO.Cloud.Infrastructure.csproj", "src/ITO.Cloud.Infrastructure/"]
COPY ["src/ITO.Cloud.Domain/ITO.Cloud.Domain.csproj",     "src/ITO.Cloud.Domain/"]

RUN dotnet restore "src/ITO.Cloud.API/ITO.Cloud.API.csproj"

# Copiar código fuente
COPY . .

RUN dotnet publish "src/ITO.Cloud.API/ITO.Cloud.API.csproj" \
    -c Release -o /app/publish --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Instalar fuentes para QuestPDF (reportes PDF)
RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 fonts-liberation && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "ITO.Cloud.API.dll"]

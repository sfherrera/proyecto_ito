# ITO Cloud — Plataforma de Inspección Técnica de Obras

Plataforma SaaS B2B multiempresa para la gestión digital de inspecciones técnicas en obras de construcción e inmobiliarias.

## Stack tecnológico

| Capa | Tecnología |
|------|-----------|
| Backend API | ASP.NET Core 8, Clean Architecture, CQRS (MediatR) |
| ORM | Entity Framework Core 8 |
| Base de datos | PostgreSQL 16 |
| Frontend web | Blazor Server + MudBlazor |
| App móvil | Android nativo (Kotlin, Jetpack Compose, Room) |
| Autenticación | ASP.NET Identity + JWT |
| Archivos | MinIO (S3 compatible) |
| Reportes PDF | QuestPDF |
| Reportes Excel | ClosedXML |
| Infraestructura | Docker + docker-compose |

## Estructura del proyecto

```
src/
  ITO.Cloud.Domain/          ← Entidades, interfaces, enums
  ITO.Cloud.Application/     ← CQRS, DTOs, validaciones, casos de uso
  ITO.Cloud.Infrastructure/  ← EF Core, MinIO, email, JWT
  ITO.Cloud.API/             ← ASP.NET Core Web API (REST + JWT)
  ITO.Cloud.Web/             ← Blazor Server
mobile/
  ITO.Cloud.Android/         ← App Android Kotlin
tests/
  ITO.Cloud.Domain.Tests/
  ITO.Cloud.Application.Tests/
  ITO.Cloud.API.Tests/
docker/
  docker-compose.yml
docs/
  ETAPA1_ARQUITECTURA.md
  adr/
```

## Levantar entorno local

```bash
docker-compose -f docker/docker-compose.yml up -d
```

## Documentación

- [Etapa 1: Arquitectura](docs/ETAPA1_ARQUITECTURA.md)
- [ADR-001: Blazor Server](docs/adr/ADR-001-blazor-server.md)
- [ADR-002: Multi-tenant lógico](docs/adr/ADR-002-multitenant-logico.md)

## Usuarios de prueba

> Contraseña de todos los usuarios: `Test1234!`

### Tenant 1 — ITO Pacífico SpA (`ito-pacifico`)

| Nombre | Email | Rol | Cargo |
|--------|-------|-----|-------|
| Rodrigo Fuentes Vidal | admin@itopacifico.cl | Administrador | Administrador ITO |
| Carlos Méndez Torres | carlos.mendez@itopacifico.cl | Supervisor | Supervisor de Obra |
| Pablo Rojas Soto | pablo.rojas@itopacifico.cl | Inspector | Inspector ITO Senior |
| Ana González Muñoz | ana.gonzalez@itopacifico.cl | Inspector | Inspector ITO |
| Juan Herrera Lagos | juan.herrera@estructurassur.cl | Contratista | Jefe de Obra |

### Tenant 2 — Consultores Sur Ltda (`consultores-sur`)

| Nombre | Email | Rol | Cargo |
|--------|-------|-----|-------|
| Marcela Ríos Pinto | admin@consultores-sur.cl | Administrador | Directora |
| Diego Silva Araya | diego.silva@consultores-sur.cl | Inspector | Inspector |

---

## Estado del proyecto

| Etapa | Estado |
|-------|--------|
| Etapa 1: Arquitectura | ✅ Completa |
| Etapa 2: Dominio y BD | 🔄 Siguiente |
| Etapa 3: Backend API | ⏳ Pendiente |
| Etapa 4: Frontend Web | ⏳ Pendiente |
| Etapa 5: App Android | ⏳ Pendiente |
| Etapa 6: Reportes | ⏳ Pendiente |
| Etapa 7: Infraestructura | ⏳ Pendiente |
| Etapa 8: IA (futuro) | ⏳ Pendiente |

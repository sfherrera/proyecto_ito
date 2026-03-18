# ITO Cloud — Plataforma de Inspección Técnica de Obras
## ETAPA 1: Descubrimiento y Arquitectura
**Versión:** 1.0 | **Fecha:** 2026-03-17 | **Arquitecto:** ITO Cloud Team

---

## 1. VISIÓN GENERAL DEL PRODUCTO

### ¿Qué es ITO Cloud?
ITO Cloud es una plataforma SaaS B2B multiempresa para la gestión digital del proceso de Inspección Técnica de Obras (ITO) en proyectos de construcción e inmobiliarias.

### Problema que resuelve
Hoy las inspecciones de obra se gestionan con:
- Formularios en papel o Excel
- WhatsApp para evidencia fotográfica
- Correos sueltos para observaciones
- Sin trazabilidad ni historial confiable
- Sin KPIs de calidad ni cumplimiento

### Propuesta de valor
| Actor | Beneficio |
|-------|-----------|
| ITO / Inspector | App móvil offline, registro ágil en terreno |
| Supervisor de obra | Trazabilidad de hallazgos en tiempo real |
| Constructora | Dashboard ejecutivo y reportes automáticos |
| Mandante / cliente | Transparencia y evidencia del proceso |
| Contratista | Seguimiento de sus observaciones asignadas |

---

## 2. DECISIÓN TÉCNICA: BLAZOR SERVER vs BLAZOR WEBASSEMBLY

### Análisis comparativo

| Criterio | Blazor Server | Blazor WebAssembly |
|----------|--------------|-------------------|
| Tiempo de carga inicial | Rápido (~50KB) | Lento (>10MB descarga inicial) |
| Latencia UI | Depende de red (SignalR) | Sin latencia, todo en cliente |
| Acceso a recursos servidor | Directo | Requiere HTTP |
| SEO | Mejor (SSR) | Limitado |
| Offline | No | Parcialmente (PWA) |
| Integración con Identity | Nativa y simple | Compleja (cookie vs JWT) |
| Dashboards en tiempo real | Excelente (SignalR nativo) | Requiere WebSockets manual |
| Escalabilidad | Stateful, requiere sticky sessions | Stateless, más escalable |
| Complejidad de despliegue | Simple | Moderada |
| Costo infra inicial | Menor | Mayor (CDN + API separada) |

### **DECISIÓN: Blazor Server**

**Justificación:**
1. El frontend web es una herramienta de **gestión y supervisión**, no un cliente que deba funcionar offline — ese rol lo cubre la app Android.
2. Los dashboards ejecutivos requieren datos en tiempo real → SignalR nativo de Blazor Server es ideal.
3. La integración con ASP.NET Identity es directa, sin el overhead de autenticación JWT en cliente que requiere WASM.
4. El ciclo de desarrollo es más rápido al compartir código directamente con el backend sin capa HTTP intermedia.
5. El usuario objetivo usa el web en oficina con conexión estable.
6. La escalabilidad horizontal se resuelve con Redis para sesiones distribuidas — un problema bien conocido y solucionado.

---

## 3. ARQUITECTURA DEL SISTEMA

### Vista de alto nivel

```
┌─────────────────────────────────────────────────────────────┐
│                        CLIENTES                             │
│  [Navegador Web]     [App Android]     [Integraciones]      │
└──────┬──────────────────┬─────────────────┬────────────────-┘
       │ HTTPS/SignalR    │ HTTPS/REST      │ HTTPS/REST
       ▼                  ▼                 ▼
┌─────────────────────────────────────────────────────────────┐
│                    CAPA DE PRESENTACIÓN                     │
│  ┌─────────────────┐    ┌──────────────────────────────┐   │
│  │  Blazor Server  │    │     ASP.NET Core Web API      │   │
│  │  (ITO.Web)      │    │     (ITO.API) - REST + JWT    │   │
│  └────────┬────────┘    └──────────────┬───────────────┘   │
└───────────┼────────────────────────────┼───────────────────-┘
            │                            │
            ▼                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   CAPA DE APLICACIÓN                        │
│              ITO.Application (CQRS + MediatR)               │
│   Commands │ Queries │ Validators │ DTOs │ Mappers          │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                    CAPA DE DOMINIO                          │
│                    ITO.Domain                               │
│   Entities │ Interfaces │ Enums │ ValueObjects │ Rules      │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                 CAPA DE INFRAESTRUCTURA                     │
│                  ITO.Infrastructure                         │
│  EF Core │ PostgreSQL │ MinIO │ Email │ JWT │ Audit        │
└─────────────────────────────────────────────────────────────┘
```

### Arquitectura de despliegue (Docker)

```
docker-compose
├── ito-api          → ASP.NET Core API  (puerto 5000)
├── ito-web          → Blazor Server     (puerto 5001)
├── postgres         → PostgreSQL 16     (puerto 5432)
├── minio            → MinIO S3          (puerto 9000/9001)
└── redis            → Redis (sesiones)  (puerto 6379)
```

---

## 4. ESTRUCTURA DE SOLUCIÓN

```
ITO.Cloud.sln
│
├── src/
│   ├── ITO.Cloud.Domain/
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── AuditableEntity.cs
│   │   │   └── ITenantEntity.cs
│   │   ├── Entities/
│   │   │   ├── Identity/
│   │   │   │   ├── Tenant.cs
│   │   │   │   ├── ApplicationUser.cs
│   │   │   │   └── UserRole.cs
│   │   │   ├── Projects/
│   │   │   │   ├── Company.cs
│   │   │   │   ├── Project.cs
│   │   │   │   ├── ProjectStage.cs
│   │   │   │   ├── ProjectSector.cs
│   │   │   │   ├── ProjectUnit.cs
│   │   │   │   └── Contractor.cs
│   │   │   ├── Templates/
│   │   │   │   ├── InspectionTemplate.cs
│   │   │   │   ├── TemplateSection.cs
│   │   │   │   ├── TemplateQuestion.cs
│   │   │   │   └── TemplateVersion.cs
│   │   │   ├── Inspections/
│   │   │   │   ├── Inspection.cs
│   │   │   │   ├── InspectionAnswer.cs
│   │   │   │   ├── InspectionEvidence.cs
│   │   │   │   └── InspectionAssignment.cs
│   │   │   ├── Observations/
│   │   │   │   ├── Observation.cs
│   │   │   │   ├── ObservationHistory.cs
│   │   │   │   └── Reinspection.cs
│   │   │   └── Documents/
│   │   │       └── ProjectDocument.cs
│   │   ├── Enums/
│   │   │   ├── InspectionStatus.cs
│   │   │   ├── ObservationStatus.cs
│   │   │   ├── ObservationSeverity.cs
│   │   │   └── QuestionType.cs
│   │   ├── Interfaces/
│   │   │   ├── Repositories/
│   │   │   │   ├── IProjectRepository.cs
│   │   │   │   └── IInspectionRepository.cs
│   │   │   ├── Services/
│   │   │   │   ├── IFileStorageService.cs
│   │   │   │   ├── IEmailService.cs
│   │   │   │   └── IAIService.cs          ← interfaz IA (no implementada aún)
│   │   │   └── ITenantContext.cs
│   │   └── ValueObjects/
│   │       ├── Address.cs
│   │       └── GeoLocation.cs
│   │
│   ├── ITO.Cloud.Application/
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   └── AuditBehavior.cs
│   │   │   ├── Exceptions/
│   │   │   │   ├── ValidationException.cs
│   │   │   │   ├── NotFoundException.cs
│   │   │   │   └── ForbiddenException.cs
│   │   │   └── Mappings/
│   │   │       └── MappingProfile.cs
│   │   ├── Features/
│   │   │   ├── Companies/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateCompanyCommand.cs
│   │   │   │   │   └── UpdateCompanyCommand.cs
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── GetCompaniesQuery.cs
│   │   │   │   │   └── GetCompanyByIdQuery.cs
│   │   │   │   └── DTOs/
│   │   │   │       └── CompanyDto.cs
│   │   │   ├── Projects/
│   │   │   ├── Inspections/
│   │   │   ├── Templates/
│   │   │   ├── Observations/
│   │   │   └── Dashboard/
│   │   └── DependencyInjection.cs
│   │
│   ├── ITO.Cloud.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/           ← IEntityTypeConfiguration por entidad
│   │   │   ├── Migrations/
│   │   │   ├── Interceptors/
│   │   │   │   └── AuditInterceptor.cs
│   │   │   └── Repositories/
│   │   ├── Identity/
│   │   │   ├── JwtTokenService.cs
│   │   │   └── CurrentUserService.cs
│   │   ├── Storage/
│   │   │   └── MinIOStorageService.cs
│   │   ├── Email/
│   │   │   └── SmtpEmailService.cs
│   │   ├── AI/
│   │   │   └── AIServiceStub.cs          ← stub preparado para IA futura
│   │   └── DependencyInjection.cs
│   │
│   ├── ITO.Cloud.API/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── CompaniesController.cs
│   │   │   ├── ProjectsController.cs
│   │   │   ├── TemplatesController.cs
│   │   │   ├── InspectionsController.cs
│   │   │   ├── ObservationsController.cs
│   │   │   └── FilesController.cs
│   │   ├── Middleware/
│   │   │   ├── TenantMiddleware.cs
│   │   │   └── ExceptionMiddleware.cs
│   │   ├── Filters/
│   │   │   └── ApiExceptionFilter.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   └── ITO.Cloud.Web/
│       ├── Pages/
│       │   ├── Auth/
│       │   ├── Dashboard/
│       │   ├── Companies/
│       │   ├── Projects/
│       │   ├── Templates/
│       │   ├── Inspections/
│       │   ├── Observations/
│       │   └── Reports/
│       ├── Shared/
│       │   ├── MainLayout.razor
│       │   └── NavMenu.razor
│       ├── Services/                     ← servicios para consumir la API
│       └── Program.cs
│
├── mobile/
│   └── ITO.Cloud.Android/               ← Kotlin, módulo separado
│       ├── app/src/main/
│       │   ├── java/cl/itocloud/
│       │   │   ├── ui/
│       │   │   ├── data/
│       │   │   │   ├── local/           ← Room Database (offline)
│       │   │   │   ├── remote/          ← Retrofit API client
│       │   │   │   └── repository/
│       │   │   ├── domain/
│       │   │   └── sync/                ← SyncWorker (WorkManager)
│       │   └── res/
│       └── build.gradle.kts
│
├── tests/
│   ├── ITO.Cloud.Domain.Tests/
│   ├── ITO.Cloud.Application.Tests/
│   └── ITO.Cloud.API.Tests/
│
├── docker/
│   ├── Dockerfile.api
│   ├── Dockerfile.web
│   └── docker-compose.yml
│
└── docs/
    ├── ETAPA1_ARQUITECTURA.md            ← este archivo
    ├── ETAPA2_DOMINIO.md
    └── adr/                             ← Architecture Decision Records
```

---

## 5. MODELO MULTIEMPRESA (MULTI-TENANT)

### Estrategia: Tenancy lógico por TenantId

Se utiliza **multi-tenant lógico** (base de datos compartida, esquema compartido con discriminador `TenantId`) porque:
- Simplicidad operacional
- Menor costo de infraestructura inicial
- Migración de BD centralizada
- Escalable a futuro (puede evolucionar a schema-per-tenant)

### Implementación

```
Tabla: tenants
  id, slug, nombre, plan, activo, created_at

Todas las tablas de negocio tienen:
  tenant_id UUID NOT NULL (FK → tenants.id, indexado)
```

### Resolución del Tenant

```
Flujo de resolución del TenantId:
1. Usuario hace login → JWT incluye claim "tenant_id"
2. Cada request → TenantMiddleware extrae tenant_id del JWT
3. TenantContext (scoped) expone el TenantId actual
4. ApplicationDbContext aplica QueryFilter global:
   modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId)
5. Todos los repositorios heredan este filtro automáticamente
```

### Aislamiento de datos (seguridad)
- Global Query Filters en EF Core → automático en todas las queries
- Validación explícita en comandos críticos (doble verificación)
- Tenant en JWT firmado → no manipulable por cliente

---

## 6. ESTRATEGIA OFFLINE PARA ANDROID

### Problema
El inspector trabaja en obras donde puede haber mala o nula conectividad.

### Arquitectura Offline-First

```
App Android (Offline-First con Room + WorkManager)

┌─────────────────────────────────────────────────┐
│              Capa de UI (Jetpack Compose)        │
└────────────────────┬────────────────────────────┘
                     │ observa StateFlow
                     ▼
┌─────────────────────────────────────────────────┐
│              ViewModel (MVVM)                   │
└────────────────────┬────────────────────────────┘
                     │ llama Repository
                     ▼
┌─────────────────────────────────────────────────┐
│              Repository (Source of Truth)       │
│  → Siempre lee desde Room (local)               │
│  → Escribe en Room + encola sync pendiente      │
└──────┬──────────────────────┬───────────────────┘
       │                      │
       ▼                      ▼
┌──────────────┐    ┌─────────────────────────────┐
│  Room DB     │    │  SyncQueue (tabla local)    │
│  (SQLite)    │    │  operaciones pendientes     │
└──────────────┘    └──────────────┬──────────────┘
                                   │ cuando hay red
                                   ▼
                    ┌─────────────────────────────┐
                    │  SyncWorker (WorkManager)   │
                    │  → envía a ITO.API          │
                    │  → maneja conflictos        │
                    │  → retry automático         │
                    └─────────────────────────────┘
```

### Reglas de sincronización
- `last_modified_at` en cada entidad para resolución de conflictos (server wins)
- Fotos: se suben primero con referencia temporal local, se reemplazan al sincronizar
- UUID como PK en todas las entidades → permite creación offline sin colisión
- WorkManager garantiza ejecución aunque la app se cierre

---

## 7. MÓDULOS DEL SISTEMA

| # | Módulo | Descripción | MVP |
|---|--------|-------------|-----|
| 1 | Identity & Tenants | Login, roles, multiempresa | ✅ |
| 2 | Empresas & Proyectos | CRUD de empresa, obra, etapas, sectores, unidades | ✅ |
| 3 | Usuarios & Permisos | Gestión de usuarios por tenant, roles por proyecto | ✅ |
| 4 | Plantillas de Inspección | Builder dinámico de formularios | ✅ |
| 5 | Programación | Agenda de inspecciones, asignación a ITO | ✅ |
| 6 | Ejecución | App Android, checklist, evidencia, offline | ✅ |
| 7 | Observaciones / NC | Registro, seguimiento, estados, responsables | ✅ |
| 8 | Reinspecciones | Validación de correcciones, trazabilidad | ✅ |
| 9 | Documentos | Adjuntos a proyectos, planos, contratos | ✅ |
| 10 | Reportes | PDF, Excel, informe de inspección | ✅ |
| 11 | Dashboard | KPIs ejecutivos, gráficos | ✅ |
| 12 | Notificaciones | Email interno, notificaciones del sistema | ✅ |
| 13 | Módulo IA | Stub/interfaces preparadas, sin implementar | ⏳ |
| 14 | Auditoría | Log de acciones, historial de cambios | ✅ |

---

## 8. ROADMAP DEL MVP

### Sprint 0 — Fundación (1-2 semanas)
- Crear solución .NET con Clean Architecture
- Configurar PostgreSQL + EF Core + Migrations
- Configurar Identity + JWT
- Configurar MinIO
- Docker-compose local funcional
- CI básico

### Sprint 1 — Núcleo de negocio (2-3 semanas)
- CRUD: Tenants, Empresas, Proyectos, Etapas, Sectores, Unidades
- CRUD: Usuarios, Roles, Permisos
- Blazor Web: Layout, Login, Dashboard vacío, CRUD Empresas y Proyectos

### Sprint 2 — Plantillas de inspección (2 semanas)
- Motor de plantillas dinámicas
- Builder de secciones y preguntas
- Versionado de plantillas
- Frontend: editor de plantillas

### Sprint 3 — Inspecciones web (2 semanas)
- Programación de inspecciones
- Asignación a ITO
- Registro básico desde web
- Estados y flujo

### Sprint 4 — App Android (3 semanas)
- Login + sync inicial
- Lista de inspecciones asignadas
- Ejecución offline de checklist
- Captura de fotos
- Sincronización

### Sprint 5 — Observaciones y reinspecciones (2 semanas)
- Módulo completo de NC
- Flujo de corrección y reinspección
- Trazabilidad

### Sprint 6 — Reportes y Dashboard (2 semanas)
- PDF de inspección con QuestPDF
- Excel con ClosedXML
- Dashboard KPIs con gráficos (MudBlazor Charts)

### Sprint 7 — Pulido MVP (1-2 semanas)
- Notificaciones email
- Validaciones finales
- Documentación API (Swagger)
- Testing crítico
- Deploy en servidor

**Total estimado MVP completo: ~15-18 semanas con 1 equipo full stack**

---

## 9. DECISIONES TÉCNICAS CLAVE

| Decisión | Opción elegida | Alternativa descartada | Razón |
|----------|---------------|----------------------|-------|
| ORM | EF Core 8 | Dapper | Productividad + migraciones + Global Filters para multi-tenant |
| CQRS | MediatR | Servicio directo | Separación de responsabilidades, mejor testabilidad |
| Validación | FluentValidation | DataAnnotations | Más expresivo, separado del modelo |
| Mapeo | AutoMapper | Mapeo manual | Reduce boilerplate en DTOs |
| PDF | QuestPDF | iTextSharp | Licencia MIT, API moderna, mejor DX |
| Excel | ClosedXML | EPPlus | Licencia MIT, sin restricción comercial |
| Storage | MinIO | Azure Blob | Auto-hosteable, compatible S3, sin vendor lock-in |
| Auth | ASP.NET Identity + JWT | Keycloak | Simplicidad, control total, no dependencia externa |
| Logging | Serilog | ILogger nativo | Sinks configurables (consola, archivo, Seq) |
| Paginación | Cursor-based (con fallback offset) | Solo offset | Mejor rendimiento en listas grandes |

---

## 10. RIESGOS TÉCNICOS

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|-------------|---------|------------|
| Conflictos de sync offline Android | Alta | Alto | Estrategia server-wins + UUID + timestamps |
| Rendimiento Blazor Server con muchos usuarios concurrentes | Media | Alto | Redis para state distribuido + sticky sessions bien configuradas |
| Tamaño de archivos multimedia (fotos HD en obra) | Alta | Medio | Compresión en app Android antes de upload, limits en API |
| Complejidad del motor de plantillas dinámicas | Alta | Alto | Diseñar modelo de datos flexible desde el inicio (Sprint 0) |
| Multi-tenant: filtros que se olvidan en queries | Media | Crítico | EF Global Query Filters + tests de seguridad de tenant |
| Migración de esquema con clientes en producción | Media | Alto | Estrategia blue-green + migraciones no destructivas |
| Latencia en generación de PDFs grandes | Baja | Medio | Generación asíncrona con cola de trabajos |

---

## 11. PATRONES Y CONVENCIONES

### Nomenclatura
- **Tablas BD:** snake_case, plural (`inspection_templates`, `project_units`)
- **Entidades C#:** PascalCase, singular (`InspectionTemplate`, `ProjectUnit`)
- **DTOs:** `{Entidad}Dto`, `Create{Entidad}Dto`, `Update{Entidad}Dto`
- **Commands:** `Create{Entidad}Command`, `Update{Entidad}Command`
- **Queries:** `Get{Entidades}Query`, `Get{Entidad}ByIdQuery`
- **Handlers:** `{Command/Query}Handler`
- **Controllers:** plural (`/api/companies`, `/api/projects`)

### Auditoría estándar (todas las tablas)
```sql
created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
created_by     UUID        NOT NULL
updated_at     TIMESTAMPTZ
updated_by     UUID
deleted_at     TIMESTAMPTZ          -- soft delete
deleted_by     UUID
is_deleted     BOOLEAN NOT NULL DEFAULT FALSE
tenant_id      UUID        NOT NULL
```

### Respuesta API estándar
```json
{
  "success": true,
  "data": { ... },
  "message": null,
  "errors": [],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}
```

---

## 12. PREPARACIÓN PARA IA FUTURA

Se definen interfaces vacías desde el inicio para no acoplar a ningún proveedor:

```csharp
// ITO.Cloud.Domain/Interfaces/Services/IAIService.cs
public interface IAIService
{
    Task<string> GenerateObservationDescriptionAsync(ObservationContext context);
    Task<string> SummarizeInspectionAsync(InspectionSummaryContext context);
    Task<ObservationSeverity> ClassifySeverityAsync(string description);
    Task<IEnumerable<string>> SemanticSearchAsync(string query, string context);
}

// En Infrastructure: AIServiceStub.cs implementa la interfaz
// retornando NotImplementedException o respuestas vacías
// → el DI container inyecta el stub hasta que se implemente OpenAI
```

---

## SIGUIENTE PASO EXACTO

**Comenzar ETAPA 2: Modelo de dominio y base de datos**

Acción inmediata:
1. Crear la solución `.NET` con todos los proyectos de capas
2. Definir todas las entidades del dominio con sus relaciones
3. Crear el `ApplicationDbContext` con configuraciones EF Core
4. Generar el script SQL de creación de tablas PostgreSQL
5. Configurar el `docker-compose.yml` base

**Ejecutar en siguiente sesión:** "Avanza con la Etapa 2: crea la solución .NET, las entidades de dominio y el modelo de base de datos completo"

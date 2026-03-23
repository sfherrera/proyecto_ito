# ITO Cloud — Plataforma de Inspección Técnica de Obras

Plataforma SaaS B2B multiempresa para la gestión digital de inspecciones técnicas en obras de construcción e inmobiliarias.

## Stack tecnológico

| Capa | Tecnología |
|------|-----------|
| Backend API | ASP.NET Core 10, Clean Architecture, CQRS (MediatR) |
| ORM | Entity Framework Core 10 |
| Base de datos | PostgreSQL 18 |
| Frontend web | Blazor Server + MudBlazor 9 |
| App móvil | Android nativo (Kotlin, Jetpack Compose, Room, Hilt, Retrofit) |
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
android/
  app/                       ← App Android Kotlin (Compose + Room + Retrofit)
docs/
  *.docx                     ← Documentación ejecutiva y diagramas
```

## Levantar entorno local

### Requisitos previos

- .NET 10 SDK
- PostgreSQL 18 corriendo en `localhost:5432`
- Base de datos `ito_cloud` creada

### 1. Backend API

```bash
cd src/ITO.Cloud.API
dotnet run
# API disponible en http://localhost:5095
```

### 2. Frontend Web

```bash
cd src/ITO.Cloud.Web
dotnet run
# Web disponible en http://localhost:5047
```

### 3. Seed de datos de prueba

```bash
# Crear / resetear usuarios
POST http://localhost:5095/dev/seed-users

# Poblar datos de prueba (proyectos, inspecciones, etc.)
POST http://localhost:5095/dev/seed-data
```

### 4. App Android (desarrollo local)

El proyecto Android se encuentra en `/android`. Para compilar apuntando al API local:

```bash
cd android
./gradlew assembleDebug
# El APK debug usa http://10.0.2.2:5095/api/ (localhost del emulador)
# El APK release usa https://ito-cloud-api.azurewebsites.net/api/
```

Instalar en emulador:
```bash
adb -s emulator-5554 install app/build/outputs/apk/debug/app-debug.apk
```

## Usuarios de prueba

> Contraseña de todos los usuarios: `Test1234!`

### Tenant 1 — ITO Pacífico SpA (`ito-pacifico`)

| Nombre | Email | Rol | Cargo |
|--------|-------|-----|-------|
| Rodrigo Fuentes Vidal | admin@itopacifico.cl | Administrador, SuperAdmin | Administrador ITO |
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
| Etapa 1: Arquitectura y scaffolding | ✅ Completa |
| Etapa 2: Dominio, BD y migraciones | ✅ Completa |
| Etapa 3: Backend API (CQRS + REST) | ✅ Completa |
| Etapa 4: Frontend Web (Blazor) | ✅ Completa |
| Etapa 5: App Android (Kotlin/Compose) | 🔄 En progreso |
| Etapa 6: Reportes PDF/Excel | ⏳ Pendiente |
| Etapa 7: Infraestructura / Docker | ⏳ Pendiente |
| Etapa 8: IA (predicciones, anomalías) | ⏳ Pendiente |

## Módulos implementados

### Backend API

- **Autenticación**: Login JWT, roles por tenant (SuperAdmin, AdminTenant, Supervisor, Inspector, Contratista)
- **Empresas**: CRUD completo con lógica multitenant
- **Proyectos**: CRUD + estructura de etapas/sectores/unidades
- **Inspecciones**: Crear, iniciar, enviar, filtrar por proyecto/estado/inspector
- **Observaciones**: Crear, actualizar estado, asociar a inspección
- **Plantillas**: Crear/editar plantillas de inspección con preguntas y secciones
- **Especialidades y Contratistas**: Catálogos por tenant
- **Usuarios**: Gestión de usuarios del tenant

### Frontend Web (Blazor Server)

- Login y autenticación por rol
- Dashboard con estadísticas y gráficas
- Gestión de empresas, proyectos, inspecciones, observaciones
- Gestión de plantillas con editor visual
- Gestión de usuarios, especialidades, contratistas
- Estructura de proyecto (etapas, sectores, unidades)
- Soporte i18n (español)
- Sistema de ayuda contextual por módulo

### App Android

- Arquitectura: MVVM + Clean Architecture + Hilt DI
- Pantallas: Login, Dashboard, Lista de inspecciones, Detalle, Ejecución de inspección
- Observaciones: Lista y creación
- Sincronización offline con Room + WorkManager
- Soporte de ubicación GPS y captura de fotos

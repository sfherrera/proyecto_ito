# ADR-002: Multi-tenant lógico con TenantId en tablas compartidas

**Estado:** Aceptado
**Fecha:** 2026-03-17

## Contexto
El sistema debe soportar múltiples empresas (constructoras, inmobiliarias) con aislamiento de datos completo pero manteniendo operaciones simples.

## Decisión
Se usa **multi-tenant lógico**: una base de datos, un esquema, con columna `tenant_id` en todas las tablas de negocio + EF Core Global Query Filters.

## Consecuencias positivas
- Una sola BD a mantener y respaldar
- Migraciones únicas aplicadas a todos los tenants
- Menor costo de infraestructura inicial
- Código de dominio no cambia entre tenants

## Consecuencias negativas
- Un bug en los filtros puede filtrar datos entre tenants → se mitiga con tests de seguridad
- Rendimiento puede degradarse con muchos tenants activos → índices en tenant_id + particionamiento futuro
- Cumplimiento regulatorio puede requerir BD separada para algunos clientes → arquitectura permite evolucionar

## Alternativas descartadas
- **Schema-per-tenant:** Mayor complejidad operacional, migraciones multiplicadas
- **DB-per-tenant:** Costo prohibitivo en etapa inicial

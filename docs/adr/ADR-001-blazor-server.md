# ADR-001: Usar Blazor Server en lugar de Blazor WebAssembly

**Estado:** Aceptado
**Fecha:** 2026-03-17

## Contexto
El frontend web de ITO Cloud es una herramienta de gestión para supervisores, jefes de obra y administradores. Necesita dashboards en tiempo real, integración directa con Identity y simplicidad operacional.

## Decisión
Se usa **Blazor Server**.

## Consecuencias positivas
- Tiempo de carga inicial mínimo
- Integración nativa con ASP.NET Identity (cookies server-side)
- SignalR nativo para dashboards en tiempo real
- Ciclo de desarrollo más rápido (código compartido con dominio)
- Sin overhead de serialización JSON para acceso a datos interno

## Consecuencias negativas
- Requiere conexión activa (SignalR)
- Escalabilidad stateful → se mitiga con Redis para sesiones distribuidas
- No funciona offline → no es un requisito del web, sí del móvil

## Alternativas descartadas
- **Blazor WASM:** Mayor complejidad de auth, descarga inicial pesada, sin ventaja real para este perfil de uso
- **React/Angular:** Cambio de stack, pierde ventaja del ecosistema .NET unificado

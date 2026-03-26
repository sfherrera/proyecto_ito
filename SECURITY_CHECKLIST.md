# Checklist de Seguridad - ITO Cloud en Oracle Cloud

## ⚠️ IMPORTANTE: Esta checklist debe completar ANTES de usar en producción

---

## Fase 1: Preparación Inicial

- [ ] **Llave SSH**: Convertida a formato OpenSSH (de .ppk a .pem)
- [ ] **Permisos SSH**: Llave con permisos restringidos (`chmod 400 key.pem`)
- [ ] **Conectividad**: Conexión SSH verificada al servidor
- [ ] **Puertos**: Revisados en Oracle Cloud Network Security Groups

---

## Fase 2: Credenciales por Defecto (CAMBIAR OBLIGATORIAMENTE)

### Base de Datos PostgreSQL

- [ ] **Contraseña PostgreSQL**: Cambiar de `joanmiro` a:
  ```
  openssl rand -base64 16
  ```
  Valor: `________________________`

### Redis

- [ ] **Contraseña Redis**: Cambiar de `redispass123` a:
  ```
  openssl rand -base64 16
  ```
  Valor: `________________________`

### MinIO (Almacenamiento)

- [ ] **Usuario MinIO**: Cambiar de `minioadmin` a:
  ```
  Valor: ________________________
  ```

- [ ] **Contraseña MinIO**: Cambiar de `minioadmin123` a:
  ```
  openssl rand -base64 16
  ```
  Valor: `________________________`

### JWT (Autenticación)

- [ ] **JWT Secret**: Generar con:
  ```
  openssl rand -base64 32
  ```
  Valor: `________________________`

---

## Fase 3: Configuración de Red

### Seguridad de Puertos

- [ ] **Puerto 8080 (API)**: 
  - [ ] Abierto en Oracle Cloud Security Group
  - [ ] Acceso: ¿Público o Restringido?
  - [ ] Consideración: https:// en producción

- [ ] **Puerto 8081 (Web Blazor)**:
  - [ ] Abierto en Oracle Cloud Security Group
  - [ ] Acceso: Público
  - [ ] Consideración: https:// en producción

- [ ] **Puerto 5432 (PostgreSQL)**:
  - [ ] ¿Permítir acceso remoto? NO por defecto
  - [ ] Si SÍ: Restringir a IPs específicas

- [ ] **Puerto 6379 (Redis)**:
  - [ ] ¿Permítir acceso remoto? NO por defecto
  - [ ] Si SÍ: Requiere contraseña (verificado)

- [ ] **Puerto 9001 (MinIO Console)**:
  - [ ] ¿Necesita ser público? Evaluado
  - [ ] Si es privado: Usar Nginx como proxy

### HTTPS/SSL

- [ ] [ ] **Certificado SSL**: Consideración de Let's Encrypt
- [ ] **Reverse Proxy**: Nginx configurado (opcional)
- [ ] **Redirect HTTP → HTTPS**: Implementado

---

## Fase 4: Respaldo y Recuperación

### Base de Datos

- [ ] **Backup Strategy**: Documentado
- [ ] **Ubicación Backups**: `/home/ubuntu/backups/`
- [ ] **Frecuencia**: Diaria recomendada
- [ ] **Script de Backup**: Creado en cron

  ```bash
  # Crear script de backup diario
  sudo crontab -e
  # Agregar: 0 2 * * * /home/ubuntu/ito/backup-db.sh
  ```

### MinIO / Almacenamiento

- [ ] **Backup de Datos**: Estrategia definida
- [ ] **Replicación**: Considerada para producción
- [ ] **Retención**: Políticas configuradas

---

## Fase 5: Monitoreo y Logs

- [ ] **Logs Centralizados**: Considerado (ELK, Splunk, etc.)
- [ ] **Alertas**: Configuradas para:
  - [ ] Espacio en disco bajo
  - [ ] Contenedor caído
  - [ ] Errores en aplicación
  - [ ] Uso alto de CPU/Memoria

- [ ] **Métricas**: Prometheus + Grafana (opcional)

---

## Fase 6: Aplicación (.NET)

### API

- [ ] **Conexión String**: Apunta aProdução PostgreSQL
- [ ] **JWT Secret**: Configurado en variables de entorno
- [ ] **CORS**: Configurado correctamente
- [ ] **Logging**: Nivel configurado (Information/Warning)
- [ ] **Health Check**: Implementado (`/api/health`)

### Web (Blazor)

- [ ] **API Base URL**: Apunta correctamente
- [ ] **JWT Handling**: Corrección configurada
- [ ] **Sesiones**: Persistidas en Redis
- [ ] **Caché**: Configurado correctamente

---

## Fase 7: Documentación

- [ ] **README.md**: Actualizado con instrucciones
- [ ] **Passwords Documento**: Guardado en lugar seguro
  - Ubicación: `________________________`
  - Acceso: ¿Encriptado?
- [ ] **Runbooks**: Documentados para:
  - [ ] Restart de servicios
  - [ ] Recuperación de fallos
  - [ ] Escalamiento

---

## Fase 8: Testing

### Conectividad

- [ ] **API responde**: `curl http://localhost:8080/api/health`
- [ ] **Web accesible**: `curl http://localhost:8081`
- [ ] **Base de datos**: `docker exec ito-postgres psql -U postgres -c "SELECT 1"`
- [ ] **Redis**: `docker exec ito-redis redis-cli ping`
- [ ] **MinIO**: Consola web accesible

### Funcionalidad

- [ ] **Login**: Probado y funciona
- [ ] **Crear recursos**: Probado
- [ ] **Upload de archivos**: Funciona en MinIO
- [ ] **Reportes**: Generan correctamente
- [ ] **Notificaciones**: Entregan correctamente

---

## Fase 9: Performance

- [ ] **Carga**: Base de datos respondiendo según SLA
- [ ] **Memoria**: Contenedores dentro de límites
- [ ] **Disco**: Espacio suficiente (alertar al 80%)
- [ ] **CPU**: Bajo normal

---

## Fase 10: Cumplimiento

### Legal/Regulatorio

- [ ] **GDPR**: Si aplica, datos personales encriptados
- [ ] **Auditoría**: Logs retenidos según política
- [ ] **Términos de Servicio**: Aceptados en Oracle Cloud

### Operacional

- [ ] **SLA**: Definido y aceptado
- [ ] **RTO**: Documentado (Recovery Time Objective)
- [ ] **RPO**: Documentado (Recovery Point Objective)
- [ ] **Downtime previsto**: Comunicado a usuarios

---

## Cambios de Contraseñas

Una vez completado el deployment inicial, cambiar inmediatamente:

```bash
# En docker-compose.yml, busca y reemplaza:

# PostgreSQL
POSTGRES_PASSWORD: [NUEVA_PASS]

# Redis
redis-server --requirepass [NUEVA_PASS]

# MinIO
MINIO_ROOT_PASSWORD: [NUEVA_PASS]

# JWT
JwtSettings__SecretKey: [NUEVA_CLAVE]

# Luego:
docker-compose restart
```

---

## Contactos de Emergencia

| Rol | Nombre | Teléfono | Email |
|-----|--------|----------|-------|
| Admin | | | |
| DBA | | | |
| DevOps | | | |

---

## Notas Adicionales

```
[Espacio para notas]
```

---

**Fecha de Completación**: ______________

**Aprobado por**: ______________

**Fecha de Próxima Revisión**: ______________

---

_Versión 1.0 - Marzo 2026_

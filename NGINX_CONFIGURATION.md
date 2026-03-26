# Configuración de Nginx - Todo en Puerto 80

## Estructura de Enrutamiento

Todo el tráfico entra a través del **puerto 80** y Nginx lo enruta a los servicios internos:

```
┌─────────────────────────────────────────────────────────────────┐
│                       PUERTO 80 (Público)                       │
│                   Nginx Reverse Proxy 🔀                        │
└─────────────────────────────────────────────────────────────────┘
         │                    │                 │                  │
         ▼                    ▼                 ▼                  ▼
    /appito/             /apiito/             /minioito/          /s3ito/
         │                    │                 │                  │
         ├─────────────┐      ├─────────┐      ├─────────┐       ├────────┐
         │             │      │         │      │         │       │        │
         ▼             ▼      ▼         ▼      ▼         ▼       ▼        ▼
    Web:8081       API:8080  WebSocket MinIO MinIO:9001 MinIO  S3 API
    (Blazor)      (.NET)    (/_blazor) :9001 (Console)  (9000)
```

---

## URLs de Acceso

### Cliente (Público en Internet)

| Servicio | URL |
|----------|-----|
| Web (Blazor) | `http://129.151.98.129/appito/` |
| API REST | `http://129.151.98.129/apiito/` |
| MinIO Console | `http://129.151.98.129/minioito/` |
| MinIO S3 API | `http://129.151.98.129/s3ito/` |

### Interno (Solo dentro de Docker)

| Servicio | URL Interna |
|----------|-----|
| API | `http://api:8080` |
| Web | `http://web:8081` |
| MinIO | `http://minio:9000` (S3) ó `http://minio:9001` (Console) |
| PostgreSQL | `postgres://postgres:5432` |
| Redis | `redis://redis:6379` |

---

## Instalación

Los archivos ya incluyen:

1. **nginx.conf** - Configuración del reverse proxy
2. **docker-compose-oracle.yml** - Configuración con Nginx
3. **Dockerfile (API)** - Escucha en puerto 8080 internamente

### Estructura en el servidor (/ito)

```
/ito/
├── docker-compose-oracle.yml  (renombrado a docker-compose.yml)
├── nginx.conf
├── src/
├── docs/
└── ...
```

---

## Configuración de Nginx Explicada

### 1. Health Check (Puerto 80)

```nginx
location /health {
    access_log off;
    return 200 "healthy\n";
}
```

Permite a Docker verificar que Nginx está funcionando.

### 2. API REST (Rutas `/apiito/*`)

```nginx
location /apiito/ {
    proxy_pass http://api_backend/;  # Envia a API:8080
    # Headers importantes para .NET
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
}
```

**Ejemplo**: `GET /apiito/api/companies` → `http://api:8080/api/companies`

### 3. WebSocket para Blazor (Rutas `/appito/_blazor`)

```nginx
location /appito/_blazor {
    proxy_pass http://web_backend;
    proxy_set_header Upgrade $http_upgrade;  # IMPORTANTE para WebSocket
    proxy_set_header Connection "upgrade";
}
```

Blazor Server usa WebSockets para comunicación en tiempo real.

### 4. MinIO S3 (Rutas `/s3ito/*`)

```nginx
location /s3ito/ {
    proxy_pass http://minio_s3_backend/;
    proxy_buffering off;  # Desactivar buffer para archivos grandes
}
```

**Ejemplo**: `PUT /s3ito/uploads/file.pdf` → `http://minio:9000/uploads/file.pdf`

### 5. MinIO Console (Rutas `/minioito/*`)

```nginx
location /minioito/ {
    proxy_pass http://minio_backend/;
    # WebSocket para console (actualización en tiempo real)
    proxy_set_header Connection "Upgrade";
    proxy_set_header Upgrade $http_upgrade;
}
```

**URL**: `http://129.151.98.129/minioito/`

### 6. Web (Subruta `/appito/*`)

```nginx
location /appito/ {
    proxy_pass http://web_backend/;
}
```

**Ejemplo**: `GET /appito/` → `http://web:8081/`
**Ejemplo**: `GET /appito/counter` → `http://web:8081/counter`

---

## Ventajas de esta Configuración

✅ **Un solo puerto público (80)**
✅ **Servicios aislados internamente**
✅ **Escalable (agregar servicios es fácil)**
✅ **Seguridad: Servicios internos no expuestos directamente**
✅ **SSL/HTTPS listo para agregar**
✅ **Compatible con load balancing**

---

## Cómo Agregar HTTPS

Para agregar SSL/TLS (recomendado en producción):

1. Obtener certificado Let's Encrypt:
   ```bash
   sudo certbot certonly --standalone -d tu-dominio.com
   ```

2. Actualizar `nginx.conf`:
   ```nginx
   listen 443 ssl http2;
   ssl_certificate /etc/letsencrypt/live/tu-dominio.com/fullchain.pem;
   ssl_certificate_key /etc/letsencrypt/live/tu-dominio.com/privkey.pem;

   # Redirect HTTP a HTTPS
   server {
       listen 80;
       return 301 https://$host$request_uri;
   }
   ```

3. Reconstruir el contenedor:
   ```bash
   docker-compose down
   docker-compose up -d
   ```

---

## Limitaciones Conocidas

⚠️ **Nota sobre URLs base en la aplicación**

En el archivo `docker-compose-oracle.yml`, la configuración de Web es:

```yaml
ApiSettings__BaseUrl: http://localhost:8080
```

Esto es **correcto** porque:
- La Web se comunica INTERNAMENTE con la API (dentro de Docker)
- Nginx solo enruta el tráfico **del cliente**

Si necesitas cambiar desde el cliente, usa:
- `http://129.151.98.129/api/` (a través de Nginx)

---

## Troubleshooting

### "502 Bad Gateway"
Significa que Nginx no puede conectar con el servicio. Verifica:

```bash
# Ver logs de Nginx
docker logs ito-nginx

# Verificar que los servicios están corriendo
docker-compose ps

# Probar conectividad interna
docker exec ito-nginx ping api
```

### "Connection refused"
El servicio aún no está listo. Espera:

```bash
# Ver estado de salud
docker-compose ps

# Ver logs del servicio
docker-compose logs web
docker-compose logs api
```

### Cambios en nginx.conf no se aplican
Necesitas recargar o reconstruir:

```bash
# Recarga sin detener (valida sintaxis primero)
docker exec ito-nginx nginx -s reload

# O reinicia el contenedor
docker-compose restart nginx
```

---

## Monitoreo

### Ver tráfico en Nginx

```bash
# Logs en tiempo real
docker logs -f ito-nginx

# O acceder al contenedor
docker exec -it ito-nginx bash
tail -f /var/log/nginx/access.log
```

### Métricas

```bash
# Conexiones activas
docker exec ito-nginx netstat -an | grep ESTABLISHED | wc -l

# Estadísticas de Nginx
docker exec ito-nginx nginx -T
```

---

**Versión 1.0 - Marzo 2026**

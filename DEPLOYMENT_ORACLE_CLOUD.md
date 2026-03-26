# Deployment de ITO Cloud en Oracle Cloud

Guía completa para desplegar la aplicación ITO Cloud en una instancia de Compute Engine de Oracle Cloud.

## Requisitos Previos

- **Oracle Cloud Account** con una instancia Ubuntu 22.04 LTS
- **IP Pública**: 129.151.98.129
- **Usuario SSH**: ubuntu
- **Llave privada SSH**: `admincloudora.ppk`
- **Docker**: Se instalará automáticamente
- **Puertos abiertos** en Security Lists/Network Security Groups:
  - **80** (HTTP - Nginx reverse proxy, acceso a TODO)
  - 5432 (PostgreSQL - opcional, solo si acceso remoto)
  - 6379 (Redis - opcional, solo si acceso remoto)

---

## Paso 1: Preparación en tu computadora local

### 1.1 Convertir la llave PuTTY a formato OpenSSH (si es necesario)

Si tienes la llave en formato `.ppk` (PuTTY):

```powershell
# Instala PuTTYgen o usa directamente PuTTY Key Agent
# Opción 1: Convertir manualmente con PuTTYgen:
# - Abre PuTTYgen
# - Abre admincloudora.ppk
# - Export Key → Export OpenSSH key → Guarda como admincloudora.pem

# Opción 2: Usar puttygen en PowerShell
puttygen "C:\Users\FERNANDO HERRERA\Dropbox\oracle cloud server\admincloudora.ppk" -O private-openssh -o admincloudora.pem
```

### 1.2 Dar permisos a la llave en PowerShell

```powershell
# En PowerShell como Administrador
$key = "admincloudora.pem"
icacls $key /Inheritance:r /Grant:r "$env:USERNAME`:F"
```

---

## Paso 2: Conectarse a Oracle Cloud y configurar el servidor

### 2.1 Conectar por SSH

**Opción A: Con PowerShell (Windows 10+)**

```powershell
ssh -i "admincloudora.pem" ubuntu@129.151.98.129
```

**Opción B: Con PuTTY**

1. Abre PuTTY
2. Host: `129.151.98.129`
3. Connection → SSH → Auth: Carga `admincloudora.ppk`
4. Open

**Opción C: Con WSL2**

```bash
ssh -i ~/admincloudora.pem ubuntu@129.151.98.129
```

### 2.2 Una vez dentro del servidor (como ubuntu)

Ejecuta los siguientes comandos:

```bash
# Actualizar el sistema
sudo apt-get update && sudo apt-get upgrade -y

# Instalar Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Permitir que ubuntu use docker sin sudo
sudo usermod -aG docker ubuntu

# Instalar Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Crear directorio para la aplicación
mkdir -p ~/ito
cd ~/ito
```

Luego, **cierra la sesión SSH**:

```bash
exit
```

---

## Paso 3: Transferir los archivos de la aplicación

Desde tu computadora local (PowerShell o CMD):

```powershell
# Definir variables
$remote = "ubuntu@129.151.98.129"
$keyPath = "admincloudora.pem"
$projectPath = "C:\Fuentes\poyectos\proyecto_ito"

# Transferir todos los archivos
scp -r -i $keyPath "$projectPath\*" "${remote}:~/ito/"
```

Si la transferencia falla o es lenta, usa **WinSCP**:

1. Host: `129.151.98.129`
2. Usuario: `ubuntu`
3. Llave privada: `admincloudora.pem`
4. Navega a `/home/ubuntu/ito/`
5. Arrastra y suelta los archivos del proyecto

---

## Paso 4: Levantar los servicios en Oracle Cloud

### 4.1 Conectar nuevamente por SSH

```powershell
ssh -i "admincloudora.pem" ubuntu@129.151.98.129
```

### 4.2 Navegar a la carpeta y levantar los servicios

```bash
cd ~/ito

# Copiar archivos sql si no existen
mkdir -p docs/sql

# Ver los arhivos existentes
ls -la

# Crear el archivo de composición
# (Copia el contenido de docker-compose-oracle.yml a docker-compose.yml)
cp docker-compose-oracle.yml docker-compose.yml

# Opcional: Crear archivo .env con configuraciones
cat > .env << 'EOF'
JWT_SECRET=SuperSecretKeyParaITOCloud2025XYZ!
POSTGRES_USER=postgres
POSTGRES_PASSWORD=joanmiro
POSTGRES_DB=ito_cloud
REDIS_PASSWORD=redispass123
MINIO_ROOT_USER=minioadmin
MINIO_ROOT_PASSWORD=minioadmin123
ASPNETCORE_ENVIRONMENT=Production
LOG_LEVEL=Information
EOF

# Levantar los servicios
docker-compose up -d

# Esperar ~60 segundos a que se construyan las imágenes
# Ver progreso:
docker-compose logs -f
```

---

## Paso 5: Verificar el deployment

### 5.1 Desde el servidor (SSH)

```bash
# Ver estado de los contenedores
docker-compose ps

# Ver logs de todos los servicios
docker-compose logs

# Ver logs de un servicio específico
docker-compose logs api
docker-compose logs web
docker-compose logs postgres

# Probar conectividad a la API
curl http://localhost:8080/health
```

### 5.2 Desde tu navegador

Accede a los siguientes servicios usando la IP pública de Oracle Cloud (TODO en puerto 80):

| Servicio | URL |
|----------|-----|
| **Web (Blazor)** | `http://129.151.98.129/appito/` |
| **API REST** | `http://129.151.98.129/apiito/` |
| **MinIO Console** | `http://129.151.98.129/minioito/` |
| **MinIO S3 API** | `http://129.151.98.129/s3ito/` |

**Credenciales MinIO:**
- Usuario: `minioadmin`
- Contraseña: `minioadmin123`

---

## Paso 6: Consideraciones de Seguridad para Producción

### ⚠️ IMPORTANTE: Cambiar credenciales por defecto

Antes de exponir en internet, edita el archivo `docker-compose.yml`:

```bash
# Generar contraseñas seguras
openssl rand -base64 32  # Para JWT
openssl rand -base64 16  # Para PostgreSQL
openssl rand -base64 16  # Para Redis
openssl rand -base64 16  # Para MinIO
```

Actualiza estas variables en `docker-compose.yml`:

```yaml
# PostgreSQL
POSTGRES_PASSWORD: your_secure_password

# Redis
REDIS_PASSWORD: your_secure_password

# MinIO
MINIO_ROOT_PASSWORD: your_secure_password

# JWT
JwtSettings__SecretKey: your_secure_jwt_key
```

Luego reconstruye:

```bash
docker-compose down
docker-compose up -d
```

### Configurar HTTPS/SSL

Usa Nginx como reverse proxy con Let's Encrypt (opcional):

```bash
# Instalar Certbot
sudo apt-get install certbot python3-certbot-nginx

# Obtener certificado
sudo certbot certonly --standalone -d tu-dominio.com

# Configurar Nginx como proxy...
```

### Abrir puertos en Oracle Cloud

1. Ve a **Networking → Virtual Cloud Networks**
2. Abre el Security List del subnet
3. Agrega Security Rules para:
   - Puerto 8080 (API) - Ingress
   - Puerto 8081 (Web) - Ingress
   - Puerto 9001 (MinIO) - Ingress (opcional)

---

## Paso 7: Mantenimiento

### Ver logs en tiempo real

```bash
cd ~/ito
docker-compose logs -f --tail=100
```

### Detener servicios

```bash
docker-compose down
```

### Reiniciar servicios

```bash
docker-compose restart
```

### Actualizar la aplicación

```bash
# Descargar nuevos archivos
cd ~/ito
scp -r -i admincloudora.pem user@local-machine:~/new-files/* .

# Reconstruir imágenes
docker-compose down
docker-compose up -d --build
```

### Hacer backup de la base de datos

```bash
# Backup PostgreSQL
docker exec ito-postgres pg_dump -U postgres ito_cloud > backup_$(date +%Y%m%d_%H%M%S).sql

# Backup de MinIO
docker-compose exec minio mc cp -r --recursive \
  minio/ito-documents ~/backups/minio_backup_$(date +%Y%m%d_%H%M%S)/
```

---

## Troubleshooting

### Error: "Cannot connect to Docker daemon"

```bash
# Solución: Agregar ubuntu al grupo docker
sudo usermod -aG docker ubuntu
# Luego desconéctate y reconéctate por SSH
```

### Error: "Port 8080 already in use"

```bash
# Cambiar puertos en docker-compose.yml
# Busca "8080:8080" y cámbialo a "28080:8080"
docker-compose down
docker-compose up -d
```

### La aplicación se queda en "Starting"

```bash
# Ver logs para ver el error real
docker-compose logs api
docker-compose logs web

# Reconstruir desde cero
docker-compose down
docker-compose up -d --build --remove-orphans
```

### PostgreSQL no inicia

```bash
# Verificar que el volumen existe
docker volume ls

# Eliminar y recrear (CUIDADO: perderás datos)
docker-compose down -v
docker-compose up -d
```

---

## Scripts de utilidad

### Monitoreo de espacio en disco

```bash
# Ver uso de espacio
df -h

# Ver uso de Docker
docker system df

# Limpiar imágenes/contenedores no usados
docker system prune -a -f
```

### Exportar logs a archivo

```bash
cd ~/ito
docker-compose logs > logs_$(date +%Y%m%d_%H%M%S).txt
```

---

## Contacto y Soporte

Para problemas, consulta:

1. **Logs de la aplicación**: `docker-compose logs`
2. **Documentación de Oracle Cloud**: https://docs.oracle.com/en-us/iaas/
3. **Docker Documentation**: https://docs.docker.com/
4. **Repositorio del proyecto**: Revisar README.md en el proyecto

---

**Última actualización**: 24 de Marzo de 2026

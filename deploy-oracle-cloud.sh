#!/bin/bash

################################################################################
# Script de Deployment para Oracle Cloud
# Uso: bash deploy-oracle-cloud.sh
# 
# Este script:
# 1. Instala Docker y Docker Compose
# 2. Crea estructura de carpetas en /ito
# 3. Levanta los servicios (PostgreSQL, Redis, MinIO, API, Web)
################################################################################

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}  Deployment ITO Cloud en Oracle Cloud${NC}"
echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"

# ─────────────────────────────────────────────────────────────────────────────
# 1. Verificar si está corriendo como root/sudo
# ─────────────────────────────────────────────────────────────────────────────
if [[ $EUID -ne 0 ]]; then
   echo -e "${RED}Este script debe ejecutarse como root (usa: sudo bash deploy-oracle-cloud.sh)${NC}"
   exit 1
fi

# ─────────────────────────────────────────────────────────────────────────────
# 2. Actualizar sistema
# ─────────────────────────────────────────────────────────────────────────────
echo -e "${YELLOW}[1/6] Actualizando sistema...${NC}"
apt-get update
apt-get upgrade -y

# ─────────────────────────────────────────────────────────────────────────────
# 3. Instalar Docker
# ─────────────────────────────────────────────────────────────────────────────
echo -e "${YELLOW}[2/6] Instalando Docker...${NC}"
if ! command -v docker &> /dev/null; then
    apt-get install -y apt-transport-https ca-certificates curl software-properties-common
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | apt-key add -
    add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable"
    apt-get update
    apt-get install -y docker-ce docker-ce-cli containerd.io
    
    # Permitir al usuario ubuntu usar docker sin sudo
    usermod -aG docker ubuntu
    echo -e "${GREEN}✓ Docker instalado${NC}"
else
    echo -e "${GREEN}✓ Docker ya está instalado${NC}"
fi

# ─────────────────────────────────────────────────────────────────────────────
# 4. Instalar Docker Compose
# ─────────────────────────────────────────────────────────────────────────────
echo -e "${YELLOW}[3/6] Instalando Docker Compose...${NC}"
if ! command -v docker-compose &> /dev/null; then
    curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    chmod +x /usr/local/bin/docker-compose
    echo -e "${GREEN}✓ Docker Compose instalado${NC}"
else
    echo -e "${GREEN}✓ Docker Compose ya está instalado${NC}"
fi

# ─────────────────────────────────────────────────────────────────────────────
# 5. Crear estructura de directorios
# ─────────────────────────────────────────────────────────────────────────────
echo -e "${YELLOW}[4/6] Creando estructura de directorios...${NC}"
mkdir -p /ito
cd /ito

# ─────────────────────────────────────────────────────────────────────────────
# 6. Dar permisos al usuario ubuntu
# ─────────────────────────────────────────────────────────────────────────────
echo -e "${YELLOW}[5/6] Configurando permisos...${NC}"
chown -R ubuntu:ubuntu /ito
chmod -R 755 /ito

# ─────────────────────────────────────────────────────────────────────────────
# 7. Información final
# ─────────────────────────────────────────────────────────────────────────────
echo -e "${YELLOW}[6/6] Completado${NC}"
echo ""
echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}  ✓ Instalación completada exitosamente${NC}"
echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
echo ""
echo "Próximos pasos:"
echo "1. Los archivos de la aplicación están en: /ito"
echo "2. Para levantar los servicios, ejecuta:"
echo "   cd /ito"
echo "   docker-compose up -d"
echo ""
echo "3. Acceso a los servicios:"
echo "   - API:       http://$(hostname -I | awk '{print $1}'):8080"
echo "   - Web:       http://$(hostname -I | awk '{print $1}'):8081"
echo "   - MinIO:     http://$(hostname -I | awk '{print $1}'):9001 (usuario: minioadmin / pass: minioadmin123)"
echo "   - PostgreSQL: $(hostname -I | awk '{print $1}'):5432"
echo ""
echo "4. Ver logs:"
echo "   docker-compose logs -f"
echo ""

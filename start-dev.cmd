@echo off
echo.
echo  ===================================================
echo   ITO Cloud - Modo Desarrollo
echo  ===================================================
echo.

echo [0/4] Limpiando procesos anteriores...
powershell -Command "Get-Process dotnet,iisexpress -ErrorAction SilentlyContinue | Stop-Process -Force" >nul 2>&1
timeout /t 2 /nobreak >nul

echo [1/4] Levantando base de datos (PostgreSQL en Docker)...
docker-compose up postgres -d
timeout /t 5 /nobreak >nul

echo [2/4] Iniciando API en http://localhost:5095 ...
start "ITO Cloud API" cmd /k "cd /d "%~dp0src\ITO.Cloud.API" && dotnet run --launch-profile http"
timeout /t 10 /nobreak >nul

echo [3/4] Iniciando Web en http://localhost:5047 ...
start "ITO Cloud Web" cmd /k "cd /d "%~dp0src\ITO.Cloud.Web" && dotnet run --launch-profile http"
timeout /t 8 /nobreak >nul

echo [4/4] Abriendo navegador...
start http://localhost:5047/login

echo.
echo  ===================================================
echo   LISTO
echo   Login: admin@itocloud.cl / Admin123!
echo  ===================================================

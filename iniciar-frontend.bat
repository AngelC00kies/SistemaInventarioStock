@echo off
title Sistema de Inventario - Frontend
cd /d "%~dp0frontend"

rem Pre-requisito: sin Node.js el doble clic solo mostraria un mensaje de cmd
rem que la ventana cerraria sin dar tiempo a leer.
where node >nul 2>&1
if errorlevel 1 goto :faltaNode
where npm >nul 2>&1
if errorlevel 1 goto :faltaNode

netstat -ano | findstr /r /c:":5173 .*LISTENING" >nul
if not errorlevel 1 goto :puertoOcupado

if not exist "node_modules" (
    echo [AVISO] Faltan dependencias. Ejecutando npm install, espere...
    call npm install
    if errorlevel 1 goto :falloInstall
)

echo Iniciando frontend en http://localhost:5173 ...
rem --strictPort: si 5173 esta ocupado Vite falla en vez de saltar al 5174
rem y dejar el navegador apuntando a una instancia anterior.
call npm run dev -- --strictPort
if errorlevel 1 goto :fallo

pause
exit /b 0

:faltaNode
echo.
echo [ERROR] No se encontro "node" ni "npm" en el PATH de Windows.
echo         Instale Node.js (version LTS) y vuelva a ejecutar este archivo.
echo         Si acaba de instalarlo, cierre el explorador de archivos y
echo         vuelva a abrirlo para que tome el PATH nuevo.
echo.
pause
exit /b 1

:puertoOcupado
echo.
echo [AVISO] El puerto 5173 ya esta en uso.
echo         Cierre la otra terminal de Vite y vuelva a ejecutar este archivo.
echo.
pause
exit /b 1

:falloInstall
echo.
echo [ERROR] No se pudieron instalar las dependencias.
echo         Revise su conexion y vuelva a ejecutar este archivo.
echo.
pause
exit /b 1

:fallo
echo.
echo [AVISO] El servidor se detuvo o no pudo iniciar en el puerto 5173.
echo         Si no lo tenia abierto, cierre la otra terminal de Vite,
echo         ejecute "npm install" o vea la seccion 4 del README.
echo.
pause
exit /b 1

@echo off
title Sistema de Inventario - API
cd /d "%~dp0backend"

rem Pre-requisito: sin el SDK de .NET en el PATH, el doble clic solo mostraria un
rem mensaje de cmd que la ventana cerraria sin dar tiempo a leer.
where dotnet >nul 2>&1
if errorlevel 1 goto :faltaDotnet

rem Evita el error MSB3021/MSB3027 (DLL bloqueados) que aparece cuando ya
rem hay una instancia en ejecucion y "dotnet run" vuelve a compilar.
tasklist /fi "imagename eq SistemaInventario.Api.exe" | findstr /i /r /c:"SistemaInventario\.Api\.exe" >nul
if not errorlevel 1 goto :yaCorriendo

netstat -ano | findstr /r /c:":5080 .*LISTENING" >nul
if not errorlevel 1 goto :puertoOcupado

echo Iniciando API en http://localhost:5080 ...
dotnet run --project src\SistemaInventario.Api --launch-profile http
if errorlevel 1 goto :fallo

pause
exit /b 0

:faltaDotnet
echo.
echo [ERROR] No se encontro "dotnet" en el PATH de Windows.
echo         Instale el SDK de .NET 8 y vuelva a ejecutar este archivo.
echo         Si acaba de instalarlo, cierre el explorador de archivos y
echo         vuelva a abrirlo para que tome el PATH nuevo.
echo.
pause
exit /b 1

:yaCorriendo
echo.
echo [AVISO] La API ya esta en ejecucion en http://localhost:5080.
echo         Detenga la ventana anterior o termine el proceso
echo         "SistemaInventario.Api" y vuelva a ejecutar este archivo.
echo.
pause
exit /b 1

:puertoOcupado
echo.
echo [AVISO] El puerto 5080 ya esta en uso por otro programa.
echo         Cierre ese programa y vuelva a intentarlo.
echo.
pause
exit /b 1

:fallo
echo.
echo [ERROR] La API no pudo iniciarse.
echo         Compruebe el puerto 5080, ejecute "dotnet restore" si es la
echo         primera vez y verifique que SQL Server este en marcha.
echo         Vea la seccion 4 del README.
echo.
pause
exit /b 1

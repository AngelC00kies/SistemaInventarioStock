@echo off
title Sistema de Inventario - API
cd /d "%~dp0backend"
echo Iniciando API en http://localhost:5080 ...
dotnet run --project src\SistemaInventario.Api
pause

@echo off
title Nexoris Sync System Starter
color 0A
echo ===============================================================
echo   STARTING NEXORIS CENTRAL API ^& SYNC SERVICE (.NET 4.6.1)
echo ===============================================================
echo.

cd /d "%~dp0"

echo [1/2] Launching Central API on http://localhost:5000 ...
start "Nexoris Central API (Head Office)" "Nexoris.CentralApi\bin\Debug\Nexoris.CentralApi.exe"

timeout /t 2 /nobreak >nul

echo [2/2] Launching Branch Sync Worker Service ...
start "Nexoris Branch Sync Service" "Nexoris.SyncService\bin\Debug\Nexoris.SyncService.exe"

echo.
echo ===============================================================
echo   BOTH SERVICES ARE RUNNING IN BACKGROUND WINDOWS!
echo   You can also run all projects directly from Visual Studio 2019.
echo ===============================================================
echo.
pause

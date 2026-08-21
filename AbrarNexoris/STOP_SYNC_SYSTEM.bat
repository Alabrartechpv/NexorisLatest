@echo off
title Stop Nexoris Sync Services
color 0C
echo ========================================================
echo   STOPPING NEXORIS CENTRAL API & SYNC SERVICE
echo ========================================================
echo.

taskkill /F /IM Nexoris.CentralApi.exe 2>nul
taskkill /F /IM Nexoris.SyncService.exe 2>nul

echo.
echo [OK] All sync background services stopped.
echo.
pause

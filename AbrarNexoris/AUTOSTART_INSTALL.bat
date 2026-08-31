@echo off
title Install Nexoris Auto-Start Task
color 0A
echo ===============================================================
echo   INSTALLING NEXORIS SERVICES AUTO-START (ON WINDOWS BOOT/LOGON)
echo ===============================================================
echo.

set "SCRIPT_PATH=%~dp0START_SYNC_BACKGROUND.vbs"

echo Creating Windows Scheduled Task: "NexorisSyncServices"...
schtasks /create /tn "NexorisSyncServices" /tr "wscript.exe \"%SCRIPT_PATH%\"" /sc onlogon /rl highest /f

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ===============================================================
    echo   [OK] Auto-Start successfully installed!
    echo   Services will now start automatically whenever Windows starts.
    echo ===============================================================
) else (
    echo.
    echo [ERROR] Failed to create scheduled task. Please run this script as Administrator.
)

echo.
pause

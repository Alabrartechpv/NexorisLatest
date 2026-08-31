@echo off
title Uninstall Nexoris Auto-Start Task
color 0C
echo ===============================================================
echo   UNINSTALLING NEXORIS SERVICES AUTO-START
echo ===============================================================
echo.

schtasks /delete /tn "NexorisSyncServices" /f

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [OK] Auto-Start task "NexorisSyncServices" removed successfully.
) else (
    echo.
    echo [WARN] Task not found or already removed.
)

echo.
pause

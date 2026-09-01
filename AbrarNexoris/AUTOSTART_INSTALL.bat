@echo off
title Install Nexoris Auto-Start Task
color 0A
echo ===============================================================
echo   INSTALLING NEXORIS AUTO-START ON THIS PC
echo ===============================================================
echo.

set "SCRIPT_PATH=%~dp0START_SYNC_BACKGROUND.vbs"

echo [1/2] Creating Windows Startup Shortcut...
powershell -NoProfile -ExecutionPolicy Bypass -Command "$WshShell = New-Object -ComObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut([System.IO.Path]::Combine([System.Environment]::GetFolderPath('Startup'), 'NexorisSyncServices.lnk')); $Shortcut.TargetPath = 'wscript.exe'; $Shortcut.Arguments = '\"%SCRIPT_PATH%\"'; $Shortcut.WorkingDirectory = '%~dp0'; $Shortcut.Description = 'Auto-start Nexoris Sync Services'; $Shortcut.Save()"

echo [2/2] Verifying setup...
if exist "%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\NexorisSyncServices.lnk" (
    echo.
    echo ===============================================================
    echo   [OK] Auto-Start successfully installed on this PC!
    echo   Services will now start automatically on Windows boot.
    echo ===============================================================
) else (
    echo.
    echo [WARN] Could not create startup shortcut. Please run as Administrator.
)

echo.
pause

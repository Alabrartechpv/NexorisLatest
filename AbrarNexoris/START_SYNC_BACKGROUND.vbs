' ===============================================================
' Nexoris Silent Background Service Launcher
' Starts CentralApi and SyncService in the background without console windows
' ===============================================================

Set WshShell = CreateObject("WScript.Shell")
strCurrentDir = Left(WScript.ScriptFullName, InStrRev(WScript.ScriptFullName, "\"))

' 1. Start Central API in background (0 = hide window)
apiExe = strCurrentDir & "Nexoris.CentralApi\bin\Debug\Nexoris.CentralApi.exe"
WshShell.Run """" & apiExe & """", 0, False

' Wait 2 seconds for API to initialize
WScript.Sleep 2000

' 2. Start Branch Sync Service in background (0 = hide window)
syncExe = strCurrentDir & "Nexoris.SyncService\bin\Debug\Nexoris.SyncService.exe"
WshShell.Run """" & syncExe & """", 0, False

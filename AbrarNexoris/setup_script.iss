; Inno Setup Deployment Script for Nexoris POS
; Compatible with Inno Setup compiler v5 or v6.

[Setup]
AppName=Nexoris POS
AppVersion=1.0.1
AppPublisher=Nexoris
DefaultDirName={autopf}\Nexoris POS
DefaultGroupName=Nexoris POS
OutputDir=.\OutputInstaller
OutputBaseFilename=Nexoris_POS_Setup
Compression=lzma
SolidCompression=yes
SetupIconFile=PosBranch-Win\Resources\app_icon.ico
PrivilegesRequired=admin
DisableWelcomePage=no
DisableDirPage=no
DisableProgramGroupPage=yes
MinVersion=6.1sp1

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Copy all compiled binaries and assets recursively from the build folder
Source: "PosBranch-Win\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Copy Crystal Report (.rpt) files — NOT output to bin\Release by the build, must be explicitly included
Source: "CrsReports\SalesInvoicePrint.rpt"; DestDir: "{app}\Reports"; Flags: ignoreversion
Source: "CrsReports\CrystalReportPurcase.rpt"; DestDir: "{app}\Reports"; Flags: ignoreversion
Source: "PosBranch-Win\Reportrpt\Sales_Daily.rpt"; DestDir: "{app}\Reports"; Flags: ignoreversion
; Copy application icon to install folder (used for shortcuts)
Source: "PosBranch-Win\Resources\app_icon.ico"; DestDir: "{app}"; Flags: ignoreversion
; Copy the Crystal Reports runtime installer MSI to the temp directory during setup
Source: "Prerequisites\CRRuntime_32bit.msi"; DestDir: "{tmp}"; Flags: nocompression deleteafterinstall

[Icons]
Name: "{group}\Nexoris POS"; Filename: "{app}\NexorisPOS.exe"; IconFilename: "{app}\app_icon.ico"
Name: "{commondesktop}\Nexoris POS"; Filename: "{app}\NexorisPOS.exe"; IconFilename: "{app}\app_icon.ico"; Tasks: desktopicon

[Run]
; Install SAP Crystal Reports Runtime silently with basic progress bar (/qb) during installation
Filename: "msiexec.exe"; Parameters: "/i ""{tmp}\CRRuntime_32bit.msi"" /qb"; StatusMsg: "Installing SAP Crystal Reports engine, please wait..."; Flags: runhidden
; Run the POS application after setup completes
Filename: "{app}\NexorisPOS.exe"; Description: "{cm:LaunchProgram,Nexoris POS}"; Flags: nowait postinstall skipifsilent

[Code]
var
  DbConfigPage: TInputQueryWizardPage;

// Helper function to extract only the value from a key=value string.
// If no '=' is present, returns the trimmed string itself.
function GetConfigValue(Part: string; DefaultVal: string): string;
var
  EqPos: Integer;
begin
  EqPos := Pos('=', Part);
  if EqPos > 0 then
    Result := Trim(Copy(Part, EqPos + 1, Length(Part) - EqPos))
  else
    Result := Trim(Part);
    
  if Result = '' then
    Result := DefaultVal;
end;

procedure InitializeWizard;
var
  ConfigPath: string;
  FileContentAnsi: AnsiString;
  FileContent: string;
  Separators: TArrayOfString;
  Parts: TArrayOfString;
  ServerVal: string;
  DbVal: string;
  UserVal: string;
  PassVal: string;
begin
  // Create a custom wizard page to request SQL Connection details
  DbConfigPage := CreateInputQueryPage(wpSelectDir,
    'Database Connection Settings', 'Specify database connection parameters',
    'Please enter the SQL Server instance name, database name, and credentials. These settings will be saved to C:\Connection\Config.txt.');

  // Add fields
  DbConfigPage.Add('SQL Server / Server IP (e.g. 192.168.1.232\SQLEXPRESS or localhost\SQLEXPRESS):', False);
  DbConfigPage.Add('Database Name:', False);
  DbConfigPage.Add('Database User ID:', False);
  DbConfigPage.Add('Database Password:', True); // password masked

  // Default values
  ServerVal := 'localhost\SQLEXPRESS';
  DbVal := 'NexorisPOS';
  UserVal := 'sa';
  PassVal := '';

  // Check if an existing config file exists, if so read and parse it to pre-fill the form
  ConfigPath := 'C:\Connection\Config.txt';
  if FileExists(ConfigPath) then
  begin
    if LoadStringFromFile(ConfigPath, FileContentAnsi) then
    begin
      FileContent := Trim(String(FileContentAnsi));
      SetLength(Separators, 1);
      Separators[0] := ';';
      Parts := StringSplit(FileContent, Separators, stAll);
      if GetArrayLength(Parts) >= 4 then
      begin
        ServerVal := GetConfigValue(Parts[0], ServerVal);
        DbVal := GetConfigValue(Parts[1], DbVal);
        UserVal := GetConfigValue(Parts[2], UserVal);
        PassVal := GetConfigValue(Parts[3], PassVal);
      end;
    end;
  end;

  // Set values to the page inputs
  DbConfigPage.Values[0] := ServerVal;
  DbConfigPage.Values[1] := DbVal;
  DbConfigPage.Values[2] := UserVal;
  DbConfigPage.Values[3] := PassVal;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ConfigDir: string;
  ConfigFile: string;
  ConfigContent: string;
  Server: string;
  DbName: string;
  User: string;
  Pass: string;
  FileContentAnsi: AnsiString;
  FileContent: string;
  Separators: TArrayOfString;
  Parts: TArrayOfString;
  ExtraPart: string;
  I: Integer;
begin
  if CurStep = ssPostInstall then
  begin
    ConfigDir := 'C:\Connection';
    ConfigFile := ConfigDir + '\Config.txt';
    
    // Retrieve values entered by the user in the wizard
    Server := Trim(DbConfigPage.Values[0]);
    DbName := Trim(DbConfigPage.Values[1]);
    User := Trim(DbConfigPage.Values[2]);
    Pass := DbConfigPage.Values[3];

    // Build connection string template with required SQL keys
    ConfigContent := 'Data Source=' + Server + ';Initial Catalog=' + DbName + ';User ID=' + User + ';Password=' + Pass + ';';

    // Preserve any extra parameters (like CounterId) from the original file if it exists
    if FileExists(ConfigFile) then
    begin
      if LoadStringFromFile(ConfigFile, FileContentAnsi) then
      begin
        FileContent := Trim(String(FileContentAnsi));
        SetLength(Separators, 1);
        Separators[0] := ';';
        Parts := StringSplit(FileContent, Separators, stAll);
        
        // Append all extra parameters (index 4 onwards)
        for I := 4 to GetArrayLength(Parts) - 1 do
        begin
          ExtraPart := Trim(Parts[I]);
          if ExtraPart <> '' then
          begin
            ConfigContent := ConfigContent + ExtraPart + ';';
          end;
        end;
      end;
    end;

    try
      // Ensure Connection Directory exists
      if not DirExists(ConfigDir) then
      begin
        if CreateDir(ConfigDir) then
          Log('Created folder C:\Connection')
        else
          Log('Failed to create folder C:\Connection');
      end;

      // Write user inputs to config file (overwriting any previous value)
      if SaveStringToFile(ConfigFile, ConfigContent, False) then
        Log('Successfully wrote connection configuration to C:\Connection\Config.txt')
      else
        Log('Failed to write connection configuration to C:\Connection\Config.txt');
    except
      MsgBox('An unexpected error occurred while creating database connection configuration files. Please configure C:\Connection\Config.txt manually.', mbError, MB_OK);
    end;
  end;
end;

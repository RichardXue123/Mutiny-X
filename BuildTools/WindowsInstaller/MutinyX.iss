#define MyAppName "Mutiny X"
#define MyAppPublisher "RichardXue"
#define MyAppExeName "Mutiny X.exe"
#define MyAppVersion GetEnv("MUTINY_APP_VERSION")
#define MyBuildNumber GetEnv("MUTINY_BUILD_NUMBER")
#define MySourceDir GetEnv("MUTINY_SOURCE_DIR")
#define MyOutputDir GetEnv("MUTINY_OUTPUT_DIR")

#if MyAppVersion == ""
  #error MUTINY_APP_VERSION is not set
#endif
#if MyBuildNumber == ""
  #error MUTINY_BUILD_NUMBER is not set
#endif
#if MySourceDir == ""
  #error MUTINY_SOURCE_DIR is not set
#endif
#if MyOutputDir == ""
  #error MUTINY_OUTPUT_DIR is not set
#endif

[Setup]
AppId={{7B1F7995-F566-4F86-B402-D9E17AA9E23F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir={#MyOutputDir}
OutputBaseFilename=MutinyX-Setup-{#MyAppVersion}+{#MyBuildNumber}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupLogging=yes
CloseApplications=yes
RestartApplications=no

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "{#MySourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

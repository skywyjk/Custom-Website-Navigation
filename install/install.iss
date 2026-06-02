[Setup]
AppId={{138028de-1249-49ed-961e-ea3187f3d557}}
AppName=skyの自定义导航
AppVersion=1.0.0
AppPublisher=sky
DefaultDirName={autopf}\skyの自定义导航
OutputDir=..\output
OutputBaseFilename=WebNavigatorSetup
SetupIconFile=..\logo.ico
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest
WizardStyle=modern
DisableProgramGroupPage=yes

[Languages]
Name: "chinese"; MessagesFile: "compiler:ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加任务"; Flags: unchecked

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{commondesktop}\skyの自定义导航"; Filename: "{app}\WebNavigator.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\WebNavigator.exe"; Description: "启动 skyの自定义导航"; Flags: nowait postinstall skipifsilent
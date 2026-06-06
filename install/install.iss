[Setup]
AppId={{138028de-1249-49ed-961e-ea3187f3d557}}
AppName=skyの自定义网站导航
PrivilegesRequired=admin
AppVersion=1.0.0
AppPublisher=Sky
DefaultDirName={autopf}\skyの自定义网站导航
DefaultGroupName=skyの自定义网站导航
UninstallDisplayIcon={app}\WebNavigator.exe
OutputDir=output
OutputBaseFilename=skyの自定义导航Setup
SetupIconFile=..\logo.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
DisableProgramGroupPage=no
AllowNoIcons=yes
UsePreviousAppDir=no
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "chinesesimplified"; MessagesFile: "languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加任务:";

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Registry]
Root: "HKLM"; Subkey: "Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"; ValueType: string; ValueName: "{app}\WebNavigator.exe"; ValueData: "RUNASADMIN"; Flags: uninsdeletevalue
Root: "HKLM"; Subkey: "Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"; ValueType: string; ValueName: "{app}\unins000.exe"; ValueData: "RUNASADMIN"; Flags: uninsdeletevalue

[Icons]
Name: "{group}\sky导航"; Filename: "{app}\WebNavigator.exe"
Name: "{commondesktop}\sky导航"; Filename: "{app}\WebNavigator.exe"; Tasks: desktopicon
Name: "{group}\卸载 sky导航"; Filename: "{uninstallexe}"

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Run]
Filename: "{app}\WebNavigator.exe"; Description: "启动 skyの自定义网站导航 "; Flags: nowait postinstall skipifsilent shellexec

[Code]
procedure InstallAndDeleteCertificate;
var
  CerFile: String;
  ResultCode: Integer;
  FindRec: TFindRec;
begin
  if FindFirst(ExpandConstant('{app}\*.cer'), FindRec) then
  begin
    try
      repeat
        CerFile := ExpandConstant('{app}\') + FindRec.Name;
        
        // First install the certificate to root store
        if ShellExec('', 'certutil', '-addstore root "' + CerFile + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
        begin
          // Then delete the cer file from app directory (no message popup)
          DeleteFile(CerFile);
        end;
      until not FindNext(FindRec);
    finally
      FindClose(FindRec);
    end;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    InstallAndDeleteCertificate;
  end;
end;

procedure InitializeWizard;
begin
  WizardForm.FormStyle := fsStayOnTop;
end;

procedure UninstallInitialize;
begin
  UninstallProgressForm.FormStyle := fsStayOnTop;
end;
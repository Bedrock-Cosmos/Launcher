#define AppName "Bedrock Cosmos"
#define AppExeName "BedrockCosmos.exe"
#define AppVersion "1.0.1"
#define BuildOutput "..\Launcher\BedrockCosmos\bin\Release"

[Setup]
AppId={{184CA1AA-0199-49FD-ACD9-A77B40E236E3}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=Bedrock Cosmos
DefaultDirName={autopf}\Bedrock Cosmos
DefaultGroupName=Bedrock Cosmos
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=BedrockCosmos-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern dynamic
LicenseFile=Texts\TermsOfService\en_US.txt
InfoBeforeFile=Texts\CertificateAgreement\en_US.txt
UninstallDisplayIcon={app}\{#AppExeName}
SetupIconFile=..\Launcher\BedrockCosmos\Icon.ico
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "indonesian"; MessagesFile: "Texts\Other\Indonesian.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#BuildOutput}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Texts\CertificateAgreement\en_US.txt"; DestName: "Certificate_en_US.txt"; Flags: dontcopy
Source: "Texts\CertificateAgreement\de_DE.txt"; DestName: "Certificate_de_DE.txt"; Flags: dontcopy
Source: "Texts\CertificateAgreement\id_ID.txt"; DestName: "Certificate_id_ID.txt"; Flags: dontcopy
Source: "Texts\CertificateAgreement\es_ES.txt"; DestName: "Certificate_es_ES.txt"; Flags: dontcopy
Source: "Texts\CertificateAgreement\ja_JP.txt"; DestName: "Certificate_ja_JP.txt"; Flags: dontcopy
Source: "Texts\TermsOfService\en_US.txt"; DestName: "Terms_en_US.txt"; Flags: dontcopy
Source: "Texts\TermsOfService\de_DE.txt"; DestName: "Terms_de_DE.txt"; Flags: dontcopy
Source: "Texts\TermsOfService\id_ID.txt"; DestName: "Terms_id_ID.txt"; Flags: dontcopy
Source: "Texts\TermsOfService\es_ES.txt"; DestName: "Terms_es_ES.txt"; Flags: dontcopy
Source: "Texts\TermsOfService\ja_JP.txt"; DestName: "Terms_ja_JP.txt"; Flags: dontcopy

[Icons]
Name: "{autoprograms}\Bedrock Cosmos"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\Bedrock Cosmos"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCR; Subkey: "bedrockcosmos"; ValueType: string; ValueName: ""; ValueData: "URL:Bedrock Cosmos"; Flags: uninsdeletekey
Root: HKCR; Subkey: "bedrockcosmos"; ValueType: string; ValueName: "URL Protocol"; ValueData: ""; Flags: uninsdeletekey
Root: HKCR; Subkey: "bedrockcosmos\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\BCPackIcon.ico"; Flags: uninsdeletekey
Root: HKCR; Subkey: "bedrockcosmos\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" ""%1"""; Flags: uninsdeletekey
Root: HKCR; Subkey: ".bcpack"; ValueType: string; ValueName: ""; ValueData: "BedrockCosmos.BCPack"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "BedrockCosmos.BCPack"; ValueType: string; ValueName: ""; ValueData: "Bedrock Cosmos Pack"; Flags: uninsdeletekey; Languages: english
Root: HKCR; Subkey: "BedrockCosmos.BCPack"; ValueType: string; ValueName: ""; ValueData: "Bedrock Cosmos Paketdatei"; Flags: uninsdeletekey; Languages: german
Root: HKCR; Subkey: "BedrockCosmos.BCPack"; ValueType: string; ValueName: ""; ValueData: "Berkas paket Bedrock Cosmos"; Flags: uninsdeletekey; Languages: indonesian
Root: HKCR; Subkey: "BedrockCosmos.BCPack"; ValueType: string; ValueName: ""; ValueData: "Archivo de paquete de Bedrock Cosmos"; Flags: uninsdeletekey; Languages: spanish
Root: HKCR; Subkey: "BedrockCosmos.BCPack"; ValueType: string; ValueName: ""; ValueData: "Bedrock Cosmos パック ファイル"; Flags: uninsdeletekey; Languages: japanese
Root: HKCR; Subkey: "BedrockCosmos.BCPack\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\BCPackIcon.ico"; Flags: uninsdeletekey
Root: HKCR; Subkey: "BedrockCosmos.BCPack\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" ""%1"""; Flags: uninsdeletekey
Root: HKCR; Subkey: ".bcpersona"; ValueType: string; ValueName: ""; ValueData: "BedrockCosmos.BCPersona"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "BedrockCosmos.BCPersona"; ValueType: string; ValueName: ""; ValueData: "Bedrock Cosmos Persona"; Flags: uninsdeletekey; Languages: english
Root: HKCR; Subkey: "BedrockCosmos.BCPersona"; ValueType: string; ValueName: ""; ValueData: "Bedrock Cosmos Persona-Datei"; Flags: uninsdeletekey; Languages: german
Root: HKCR; Subkey: "BedrockCosmos.BCPersona"; ValueType: string; ValueName: ""; ValueData: "Berkas persona Bedrock Cosmos"; Flags: uninsdeletekey; Languages: indonesian
Root: HKCR; Subkey: "BedrockCosmos.BCPersona"; ValueType: string; ValueName: ""; ValueData: "Archivo de persona de Bedrock Cosmos"; Flags: uninsdeletekey; Languages: spanish
Root: HKCR; Subkey: "BedrockCosmos.BCPersona"; ValueType: string; ValueName: ""; ValueData: "Bedrock Cosmos Persona ファイル"; Flags: uninsdeletekey; Languages: japanese
Root: HKCR; Subkey: "BedrockCosmos.BCPersona\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\BCPackIcon.ico"; Flags: uninsdeletekey
Root: HKCR; Subkey: "BedrockCosmos.BCPersona\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" ""%1"""; Flags: uninsdeletekey

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[Code]
function CurrentLanguageCode(): String;
begin
  if ActiveLanguage = 'german' then
    Result := 'de_DE'
  else if ActiveLanguage = 'indonesian' then
    Result := 'id_ID'
  else if ActiveLanguage = 'spanish' then
    Result := 'es_ES'
  else if ActiveLanguage = 'japanese' then
    Result := 'ja_JP'
  else
    Result := 'en_US';
end;

function Localize(Key: String): String;
begin
  if ActiveLanguage = 'german' then
  begin
    if Key = 'CertificateTitle' then Result := 'Hinweis zum Proxy-Zertifikat'
    else if Key = 'CertificateDescription' then Result := 'Bitte lies den folgenden Hinweis, bevor du fortfährst.'
    else Result := Key;
  end
  else
  begin
    if Key = 'CertificateTitle' then Result := 'Proxy Certificate Notice'
    else if Key = 'CertificateDescription' then Result := 'Please read the following notice before continuing.'
    else Result := Key;
  end;
end;

function LocalizeCertificateText(Key: String): String;
begin
  if ActiveLanguage = 'indonesian' then
  begin
    if Key = 'CertificateTitle' then Result := 'Pemberitahuan sertifikat proxy'
    else if Key = 'CertificateDescription' then Result := 'Harap baca pemberitahuan berikut sebelum melanjutkan.'
    else Result := Key;
  end
  else if ActiveLanguage = 'spanish' then
  begin
    if Key = 'CertificateTitle' then Result := 'Aviso sobre el certificado del proxy'
    else if Key = 'CertificateDescription' then Result := 'Lea el siguiente aviso antes de continuar.'
    else Result := Key;
  end
  else if ActiveLanguage = 'japanese' then
  begin
    if Key = 'CertificateTitle' then Result := 'Proxy Certificate Notice'
    else if Key = 'CertificateDescription' then Result := 'Please read the following notice before continuing.'
    else Result := Key;
  end
  else
    Result := Localize(Key);
end;

function ReadLocalizedText(Kind: String): String;
var
  TempFile: String;
  Lines: TArrayOfString;
  Index: Integer;
begin
  Result := '';
  TempFile := Kind + '_' + CurrentLanguageCode() + '.txt';
  ExtractTemporaryFile(TempFile);
  if LoadStringsFromFile(ExpandConstant('{tmp}\') + TempFile, Lines) then
  begin
    for Index := 0 to GetArrayLength(Lines) - 1 do
    begin
      if Index > 0 then
        Result := Result + #13#10;
      Result := Result + Lines[Index];
    end;
  end;
end;

function EscapeRtfText(Value: String): String;
var
  Index: Integer;
  CodePoint: Integer;
  Character: String;
begin
  Result := '';
  Index := 1;
  while Index <= Length(Value) do
  begin
    Character := Copy(Value, Index, 1);
    if Character = '\' then
      Result := Result + '\\'
    else if Character = '{' then
      Result := Result + '\{'
    else if Character = '}' then
      Result := Result + '\}'
    else if Character = #9 then
      Result := Result + '\tab '
    else if Character = #13 then
    begin
      if (Index < Length(Value)) and (Copy(Value, Index + 1, 1) = #10) then
        Index := Index + 1;
      Result := Result + '\par ' + #13#10;
    end
    else if Character = #10 then
      Result := Result + '\par ' + #13#10
    else
    begin
      CodePoint := Ord(Character[1]);
      if (CodePoint >= 32) and (CodePoint <= 126) then
        Result := Result + Character
      else
      begin
        if CodePoint > 32767 then
          CodePoint := CodePoint - 65536;
        Result := Result + '\u' + IntToStr(CodePoint) + '?';
      end;
    end;

    Index := Index + 1;
  end;
end;

function BuildViewerRtf(Value: String): String;
var
  TextColor: String;
begin
  if IsDarkInstallMode then
    TextColor := '\red245\green245\blue245;'
  else
    TextColor := '\red31\green31\blue31;';

  Result :=
    '{\rtf1\ansi\uc1\deff0' +
    '{\fonttbl{\f0\fnil Segoe UI;}}' +
    '{\colortbl ;' + TextColor + '}' +
    '\viewkind4\pard\f0\fs22\cf1 ' +
    EscapeRtfText(Value) +
    '}';
end;

procedure LoadLocalizedWizardText(Viewer: TRichEditViewer; Kind: String);
begin
  Viewer.UseRichEdit := True;
  Viewer.RTFText := BuildViewerRtf(ReadLocalizedText(Kind));
end;

procedure InitializeWizard();
begin
  { Use the built-in Inno Setup license page so acceptance is mandatory. }
  LoadLocalizedWizardText(WizardForm.LicenseMemo, 'Terms');
  LoadLocalizedWizardText(WizardForm.InfoBeforeMemo, 'Certificate');
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpInfoBefore then
  begin
    WizardForm.PageNameLabel.Caption := LocalizeCertificateText('CertificateTitle');
    WizardForm.PageDescriptionLabel.Caption := LocalizeCertificateText('CertificateDescription');
  end;
end;

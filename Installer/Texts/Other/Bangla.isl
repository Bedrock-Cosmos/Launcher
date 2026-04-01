; *** Inno Setup সংস্করণ 6.5.0+ বাংলা বার্তা ***
;
; এই ফাইলের ব্যবহারকারী-অবদানকৃত অনুবাদ ডাউনলোড করতে, যান:
;   https://jrsoftware.org/files/istrans/
;
; দ্রষ্টব্য: এই পাঠ্য অনুবাদ করার সময়, যেসব বার্তায় আগে থেকে পিরিয়ড (.) নেই
; সেগুলোর শেষে পিরিয়ড যোগ করবেন না, কারণ সেসব বার্তায় Inno Setup স্বয়ংক্রিয়ভাবে
; পিরিয়ড যোগ করে (পিরিয়ড যোগ করলে দুটি পিরিয়ড প্রদর্শিত হবে)।

[LangOptions]
; নিচের তিনটি এন্ট্রি অত্যন্ত গুরুত্বপূর্ণ। অনুগ্রহ করে হেল্প ফাইলে
; '[LangOptions] section' বিষয়টি পড়ুন এবং বুঝুন।
LanguageName=Bengali
LanguageID=$0445
; LanguageCodePage সম্ভব হলে সবসময় সেট করা উচিত, এমনকি এই ফাইলটি Unicode হলেও
LanguageCodePage=0
; আপনি যে ভাষায় অনুবাদ করছেন তাতে যদি বিশেষ ফন্ট ফেস বা
; আকার প্রয়োজন হয়, নিচের যেকোনো এন্ট্রি আনকমেন্ট করুন এবং সেই অনুযায়ী পরিবর্তন করুন।
;DialogFontName=
;DialogFontSize=9
;DialogFontBaseScaleWidth=7
;DialogFontBaseScaleHeight=15
;WelcomeFontName=Segoe UI
;WelcomeFontSize=14

[Messages]

; *** অ্যাপ্লিকেশন শিরোনাম
SetupAppTitle=সেটআপ
SetupWindowTitle=সেটআপ - %1
UninstallAppTitle=আনইনস্টল
UninstallAppFullTitle=%1 আনইনস্টল

; *** বিবিধ সাধারণ
InformationTitle=তথ্য
ConfirmTitle=নিশ্চিত করুন
ErrorTitle=ত্রুটি

; *** SetupLdr বার্তা
SetupLdrStartupMessage=এটি %1 ইনস্টল করবে। আপনি কি অব্যাহত রাখতে চান?
LdrCannotCreateTemp=একটি অস্থায়ী ফাইল তৈরি করা সম্ভব হয়নি। সেটআপ বাতিল করা হয়েছে
LdrCannotExecTemp=অস্থায়ী ডিরেক্টরিতে ফাইলটি চালানো সম্ভব হয়নি। সেটআপ বাতিল করা হয়েছে
HelpTextNote=

; *** স্টার্টআপ ত্রুটি বার্তা
LastErrorMessage=%1.%n%nত্রুটি %2: %3
SetupFileMissing=ইনস্টলেশন ডিরেক্টরি থেকে %1 ফাইলটি অনুপস্থিত। অনুগ্রহ করে সমস্যাটি সমাধান করুন বা প্রোগ্রামটির নতুন কপি সংগ্রহ করুন।
SetupFileCorrupt=সেটআপ ফাইলগুলো দূষিত। অনুগ্রহ করে প্রোগ্রামটির নতুন কপি সংগ্রহ করুন।
SetupFileCorruptOrWrongVer=সেটআপ ফাইলগুলো দূষিত, অথবা Setup-এর এই সংস্করণের সাথে অসামঞ্জস্যপূর্ণ। অনুগ্রহ করে সমস্যাটি সমাধান করুন বা প্রোগ্রামটির নতুন কপি সংগ্রহ করুন।
InvalidParameter=কমান্ড লাইনে একটি অবৈধ প্যারামিটার পাস করা হয়েছে:%n%n%1
SetupAlreadyRunning=সেটআপ ইতিমধ্যে চলছে।
WindowsVersionNotSupported=আপনার কম্পিউটারে চলমান Windows সংস্করণটি এই প্রোগ্রাম সমর্থন করে না।
WindowsServicePackRequired=এই প্রোগ্রামের জন্য %1 Service Pack %2 বা তার পরবর্তী সংস্করণ প্রয়োজন।
NotOnThisPlatform=এই প্রোগ্রামটি %1-এ চলবে না।
OnlyOnThisPlatform=এই প্রোগ্রামটি অবশ্যই %1-এ চালাতে হবে।
OnlyOnTheseArchitectures=এই প্রোগ্রামটি কেবলমাত্র নিচের প্রসেসর আর্কিটেকচারের জন্য ডিজাইন করা Windows সংস্করণগুলোতে ইনস্টল করা যাবে:%n%n%1
WinVersionTooLowError=এই প্রোগ্রামের জন্য %1 সংস্করণ %2 বা তার পরবর্তী সংস্করণ প্রয়োজন।
WinVersionTooHighError=এই প্রোগ্রামটি %1 সংস্করণ %2 বা তার পরবর্তী সংস্করণে ইনস্টল করা যাবে না।
AdminPrivilegesRequired=এই প্রোগ্রামটি ইনস্টল করার সময় আপনাকে অ্যাডমিনিস্ট্রেটর হিসেবে লগইন করতে হবে।
PowerUserPrivilegesRequired=এই প্রোগ্রামটি ইনস্টল করার সময় আপনাকে অ্যাডমিনিস্ট্রেটর বা Power Users গ্রুপের সদস্য হিসেবে লগইন করতে হবে।
SetupAppRunningError=সেটআপ সনাক্ত করেছে যে %1 বর্তমানে চলছে।%n%nঅনুগ্রহ করে এখনই এর সকল ইনস্ট্যান্স বন্ধ করুন, তারপর চালিয়ে যেতে OK ক্লিক করুন, অথবা প্রস্থান করতে Cancel ক্লিক করুন।
UninstallAppRunningError=আনইনস্টল সনাক্ত করেছে যে %1 বর্তমানে চলছে।%n%nঅনুগ্রহ করে এখনই এর সকল ইনস্ট্যান্স বন্ধ করুন, তারপর চালিয়ে যেতে OK ক্লিক করুন, অথবা প্রস্থান করতে Cancel ক্লিক করুন।

; *** স্টার্টআপ প্রশ্ন
PrivilegesRequiredOverrideTitle=সেটআপ ইনস্টল মোড নির্বাচন করুন
PrivilegesRequiredOverrideInstruction=ইনস্টল মোড নির্বাচন করুন
PrivilegesRequiredOverrideText1=%1 সকল ব্যবহারকারীর জন্য ইনস্টল করা যাবে (প্রশাসনিক সুবিধা প্রয়োজন), অথবা শুধুমাত্র আপনার জন্য।
PrivilegesRequiredOverrideText2=%1 শুধুমাত্র আপনার জন্য, অথবা সকল ব্যবহারকারীর জন্য ইনস্টল করা যাবে (প্রশাসনিক সুবিধা প্রয়োজন)।
PrivilegesRequiredOverrideAllUsers=&সকল ব্যবহারকারীর জন্য ইনস্টল করুন
PrivilegesRequiredOverrideAllUsersRecommended=&সকল ব্যবহারকারীর জন্য ইনস্টল করুন (প্রস্তাবিত)
PrivilegesRequiredOverrideCurrentUser=শুধুমাত্র &আমার জন্য ইনস্টল করুন
PrivilegesRequiredOverrideCurrentUserRecommended=শুধুমাত্র &আমার জন্য ইনস্টল করুন (প্রস্তাবিত)

; *** বিবিধ ত্রুটি
ErrorCreatingDir=সেটআপ "%1" ডিরেক্টরি তৈরি করতে পারেনি
ErrorTooManyFilesInDir="%1" ডিরেক্টরিতে একটি ফাইল তৈরি করা সম্ভব হয়নি কারণ এতে অনেক বেশি ফাইল রয়েছে

; *** সেটআপ সাধারণ বার্তা
ExitSetupTitle=সেটআপ থেকে প্রস্থান করুন
ExitSetupMessage=সেটআপ সম্পন্ন হয়নি। আপনি এখন প্রস্থান করলে প্রোগ্রামটি ইনস্টল হবে না।%n%nইনস্টলেশন সম্পন্ন করতে আপনি অন্য সময়ে আবার সেটআপ চালাতে পারবেন।%n%nসেটআপ থেকে প্রস্থান করবেন?
AboutSetupMenuItem=সেটআপ &সম্পর্কে...
AboutSetupTitle=সেটআপ সম্পর্কে
AboutSetupMessage=%1 সংস্করণ %2%n%3%n%n%1 হোম পেজ:%n%4
AboutSetupNote=
TranslatorNote=

; *** বোতাম
ButtonBack=< &পূর্ববর্তী
ButtonNext=&পরবর্তী >
ButtonInstall=&ইনস্টল করুন
ButtonOK=ঠিক আছে
ButtonCancel=বাতিল
ButtonYes=&হ্যাঁ
ButtonYesToAll=সবকিছুতে &হ্যাঁ
ButtonNo=&না
ButtonNoToAll=সবকিছুতে &না
ButtonFinish=&সম্পন্ন করুন
ButtonBrowse=&ব্রাউজ করুন...
ButtonWizardBrowse=&ব্রাউজ করুন...
ButtonNewFolder=&নতুন ফোল্ডার তৈরি করুন

; *** "ভাষা নির্বাচন করুন" ডায়ালগ বার্তা
SelectLanguageTitle=সেটআপ ভাষা নির্বাচন করুন
SelectLanguageLabel=ইনস্টলেশনের সময় ব্যবহার করার জন্য ভাষা নির্বাচন করুন।

; *** সাধারণ উইজার্ড টেক্সট
ClickNext=চালিয়ে যেতে Next ক্লিক করুন, অথবা সেটআপ থেকে প্রস্থান করতে Cancel ক্লিক করুন।
BeveledLabel=
BrowseDialogTitle=ফোল্ডার খুঁজুন
BrowseDialogLabel=নিচের তালিকা থেকে একটি ফোল্ডার নির্বাচন করুন, তারপর OK ক্লিক করুন।
NewFolderName=নতুন ফোল্ডার

; *** "স্বাগতম" উইজার্ড পেজ
WelcomeLabel1=[name] সেটআপ উইজার্ডে স্বাগতম
WelcomeLabel2=এটি আপনার কম্পিউটারে [name/ver] ইনস্টল করবে।%n%nচালিয়ে যাওয়ার আগে অন্য সকল অ্যাপ্লিকেশন বন্ধ করার পরামর্শ দেওয়া হচ্ছে।

; *** "পাসওয়ার্ড" উইজার্ড পেজ
WizardPassword=পাসওয়ার্ড
PasswordLabel1=এই ইনস্টলেশনটি পাসওয়ার্ড সুরক্ষিত।
PasswordLabel3=অনুগ্রহ করে পাসওয়ার্ড প্রদান করুন, তারপর চালিয়ে যেতে Next ক্লিক করুন। পাসওয়ার্ড কেস-সেন্সিটিভ।
PasswordEditLabel=&পাসওয়ার্ড:
IncorrectPassword=আপনি যে পাসওয়ার্ড দিয়েছেন সেটি সঠিক নয়। অনুগ্রহ করে আবার চেষ্টা করুন।

; *** "লাইসেন্স চুক্তি" উইজার্ড পেজ
WizardLicense=লাইসেন্স চুক্তি
LicenseLabel=চালিয়ে যাওয়ার আগে অনুগ্রহ করে নিচের গুরুত্বপূর্ণ তথ্যটি পড়ুন।
LicenseLabel3=অনুগ্রহ করে নিচের লাইসেন্স চুক্তিটি পড়ুন। ইনস্টলেশন চালিয়ে যাওয়ার আগে আপনাকে এই চুক্তির শর্তাবলী মেনে নিতে হবে।
LicenseAccepted=আমি চুক্তিটি &গ্রহণ করি
LicenseNotAccepted=আমি চুক্তিটি &গ্রহণ করি না

; *** "তথ্য" উইজার্ড পেজ
WizardInfoBefore=তথ্য
InfoBeforeLabel=চালিয়ে যাওয়ার আগে অনুগ্রহ করে নিচের গুরুত্বপূর্ণ তথ্যটি পড়ুন।
InfoBeforeClickLabel=সেটআপ চালিয়ে যেতে প্রস্তুত হলে, Next ক্লিক করুন।
WizardInfoAfter=তথ্য
InfoAfterLabel=চালিয়ে যাওয়ার আগে অনুগ্রহ করে নিচের গুরুত্বপূর্ণ তথ্যটি পড়ুন।
InfoAfterClickLabel=সেটআপ চালিয়ে যেতে প্রস্তুত হলে, Next ক্লিক করুন।

; *** "ব্যবহারকারীর তথ্য" উইজার্ড পেজ
WizardUserInfo=ব্যবহারকারীর তথ্য
UserInfoDesc=অনুগ্রহ করে আপনার তথ্য লিখুন।
UserInfoName=&ব্যবহারকারীর নাম:
UserInfoOrg=&প্রতিষ্ঠান:
UserInfoSerial=&সিরিয়াল নম্বর:
UserInfoNameRequired=আপনাকে অবশ্যই একটি নাম লিখতে হবে।

; *** "গন্তব্য স্থান নির্বাচন করুন" উইজার্ড পেজ
WizardSelectDir=গন্তব্য স্থান নির্বাচন করুন
SelectDirDesc=[name] কোথায় ইনস্টল করা হবে?
SelectDirLabel3=সেটআপ নিচের ফোল্ডারে [name] ইনস্টল করবে।
SelectDirBrowseLabel=চালিয়ে যেতে Next ক্লিক করুন। আপনি যদি অন্য একটি ফোল্ডার নির্বাচন করতে চান, তাহলে Browse ক্লিক করুন।
DiskSpaceGBLabel=কমপক্ষে [gb] GB ফ্রি ডিস্ক স্পেস প্রয়োজন।
DiskSpaceMBLabel=কমপক্ষে [mb] MB ফ্রি ডিস্ক স্পেস প্রয়োজন।
CannotInstallToNetworkDrive=সেটআপ একটি নেটওয়ার্ক ড্রাইভে ইনস্টল করতে পারে না।
CannotInstallToUNCPath=সেটআপ একটি UNC পাথে ইনস্টল করতে পারে না।
InvalidPath=আপনাকে অবশ্যই ড্রাইভ লেটারসহ একটি সম্পূর্ণ পাথ লিখতে হবে; উদাহরণস্বরূপ:%n%nC:\APP%n%nঅথবা এই ফর্ম্যাটে একটি UNC পাথ:%n%n\\server\share
InvalidDrive=আপনি যে ড্রাইভ বা UNC শেয়ার নির্বাচন করেছেন সেটি বিদ্যমান নেই বা অ্যাক্সেসযোগ্য নয়। অনুগ্রহ করে অন্যটি নির্বাচন করুন।
DiskSpaceWarningTitle=পর্যাপ্ত ডিস্ক স্পেস নেই
DiskSpaceWarning=ইনস্টল করতে সেটআপের কমপক্ষে %1 KB ফ্রি স্পেস প্রয়োজন, কিন্তু নির্বাচিত ড্রাইভে মাত্র %2 KB পাওয়া গেছে।%n%nআপনি কি তবুও চালিয়ে যেতে চান?
DirNameTooLong=ফোল্ডারের নাম বা পাথ অনেক বড়।
InvalidDirName=ফোল্ডারের নামটি বৈধ নয়।
BadDirName32=ফোল্ডারের নামে নিচের কোনো অক্ষর থাকতে পারবে না:%n%n%1
DirExistsTitle=ফোল্ডার বিদ্যমান
DirExists=ফোল্ডারটি:%n%n%1%n%nইতিমধ্যে বিদ্যমান। আপনি কি তবুও সেই ফোল্ডারে ইনস্টল করতে চান?
DirDoesntExistTitle=ফোল্ডার বিদ্যমান নেই
DirDoesntExist=ফোল্ডারটি:%n%n%1%n%nবিদ্যমান নেই। আপনি কি ফোল্ডারটি তৈরি করতে চান?

; *** "কম্পোনেন্ট নির্বাচন করুন" উইজার্ড পেজ
WizardSelectComponents=কম্পোনেন্ট নির্বাচন করুন
SelectComponentsDesc=কোন কম্পোনেন্টগুলো ইনস্টল করা হবে?
SelectComponentsLabel2=আপনি যে কম্পোনেন্টগুলো ইনস্টল করতে চান সেগুলো নির্বাচন করুন; যেগুলো ইনস্টল করতে চান না সেগুলো আনচেক করুন। চালিয়ে যেতে প্রস্তুত হলে Next ক্লিক করুন।
FullInstallation=সম্পূর্ণ ইনস্টলেশন
; সম্ভব হলে 'Compact' কে 'Minimal' হিসেবে অনুবাদ করবেন না (আমি আপনার ভাষায় 'Minimal' বোঝাচ্ছি)
CompactInstallation=সংক্ষিপ্ত ইনস্টলেশন
CustomInstallation=কাস্টম ইনস্টলেশন
NoUninstallWarningTitle=কম্পোনেন্ট বিদ্যমান
NoUninstallWarning=সেটআপ সনাক্ত করেছে যে নিচের কম্পোনেন্টগুলো ইতিমধ্যে আপনার কম্পিউটারে ইনস্টল আছে:%n%n%1%n%nএই কম্পোনেন্টগুলো আনসিলেক্ট করলে সেগুলো আনইনস্টল হবে না।%n%nআপনি কি তবুও চালিয়ে যেতে চান?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceGBLabel=বর্তমান নির্বাচনের জন্য কমপক্ষে [gb] GB ডিস্ক স্পেস প্রয়োজন।
ComponentsDiskSpaceMBLabel=বর্তমান নির্বাচনের জন্য কমপক্ষে [mb] MB ডিস্ক স্পেস প্রয়োজন।

; *** "অতিরিক্ত কাজ নির্বাচন করুন" উইজার্ড পেজ
WizardSelectTasks=অতিরিক্ত কাজ নির্বাচন করুন
SelectTasksDesc=কোন অতিরিক্ত কাজগুলো সম্পাদন করা হবে?
SelectTasksLabel2=[name] ইনস্টল করার সময় সেটআপ যে অতিরিক্ত কাজগুলো করবে তা নির্বাচন করুন, তারপর Next ক্লিক করুন।

; *** "স্টার্ট মেনু ফোল্ডার নির্বাচন করুন" উইজার্ড পেজ
WizardSelectProgramGroup=স্টার্ট মেনু ফোল্ডার নির্বাচন করুন
SelectStartMenuFolderDesc=সেটআপ প্রোগ্রামের শর্টকাটগুলো কোথায় রাখবে?
SelectStartMenuFolderLabel3=সেটআপ নিচের স্টার্ট মেনু ফোল্ডারে প্রোগ্রামের শর্টকাটগুলো তৈরি করবে।
SelectStartMenuFolderBrowseLabel=চালিয়ে যেতে Next ক্লিক করুন। আপনি যদি অন্য একটি ফোল্ডার নির্বাচন করতে চান, তাহলে Browse ক্লিক করুন।
MustEnterGroupName=আপনাকে অবশ্যই একটি ফোল্ডারের নাম লিখতে হবে।
GroupNameTooLong=ফোল্ডারের নাম বা পাথ অনেক বড়।
InvalidGroupName=ফোল্ডারের নামটি বৈধ নয়।
BadGroupName=ফোল্ডারের নামে নিচের কোনো অক্ষর থাকতে পারবে না:%n%n%1
NoProgramGroupCheck2=স্টার্ট মেনু ফোল্ডার &তৈরি করবেন না

; *** "ইনস্টল করতে প্রস্তুত" উইজার্ড পেজ
WizardReady=ইনস্টল করতে প্রস্তুত
ReadyLabel1=সেটআপ এখন আপনার কম্পিউটারে [name] ইনস্টল করা শুরু করতে প্রস্তুত।
ReadyLabel2a=ইনস্টলেশন চালিয়ে যেতে Install ক্লিক করুন, অথবা কোনো সেটিং পর্যালোচনা বা পরিবর্তন করতে Back ক্লিক করুন।
ReadyLabel2b=ইনস্টলেশন চালিয়ে যেতে Install ক্লিক করুন।
ReadyMemoUserInfo=ব্যবহারকারীর তথ্য:
ReadyMemoDir=গন্তব্য স্থান:
ReadyMemoType=সেটআপের ধরন:
ReadyMemoComponents=নির্বাচিত কম্পোনেন্ট:
ReadyMemoGroup=স্টার্ট মেনু ফোল্ডার:
ReadyMemoTasks=অতিরিক্ত কাজ:

; *** TDownloadWizardPage উইজার্ড পেজ এবং DownloadTemporaryFile
DownloadingLabel2=ফাইল ডাউনলোড হচ্ছে...
ButtonStopDownload=ডাউনলোড &বন্ধ করুন
StopDownload=আপনি কি নিশ্চিতভাবে ডাউনলোড বন্ধ করতে চান?
ErrorDownloadAborted=ডাউনলোড বাতিল হয়েছে
ErrorDownloadFailed=ডাউনলোড ব্যর্থ হয়েছে: %1 %2
ErrorDownloadSizeFailed=আকার পাওয়া ব্যর্থ হয়েছে: %1 %2
ErrorProgress=অবৈধ প্রগ্রেস: %2-এর মধ্যে %1
ErrorFileSize=অবৈধ ফাইল আকার: প্রত্যাশিত %1, পাওয়া গেছে %2

; *** TExtractionWizardPage উইজার্ড পেজ এবং ExtractArchive
ExtractingLabel=ফাইল এক্সট্র্যাক্ট হচ্ছে...
ButtonStopExtraction=এক্সট্র্যাকশন &বন্ধ করুন
StopExtraction=আপনি কি নিশ্চিতভাবে এক্সট্র্যাকশন বন্ধ করতে চান?
ErrorExtractionAborted=এক্সট্র্যাকশন বাতিল হয়েছে
ErrorExtractionFailed=এক্সট্র্যাকশন ব্যর্থ হয়েছে: %1

; *** আর্কাইভ এক্সট্র্যাকশন ব্যর্থতার বিবরণ
ArchiveIncorrectPassword=পাসওয়ার্ডটি সঠিক নয়
ArchiveIsCorrupted=আর্কাইভটি দূষিত
ArchiveUnsupportedFormat=আর্কাইভ ফরম্যাটটি সমর্থিত নয়

; *** "ইনস্টল করার প্রস্তুতি" উইজার্ড পেজ
WizardPreparing=ইনস্টল করার প্রস্তুতি নিচ্ছে
PreparingDesc=সেটআপ আপনার কম্পিউটারে [name] ইনস্টল করার প্রস্তুতি নিচ্ছে।
PreviousInstallNotCompleted=একটি পূর্ববর্তী প্রোগ্রামের ইনস্টলেশন/অপসারণ সম্পন্ন হয়নি। সেই ইনস্টলেশন সম্পন্ন করতে আপনাকে আপনার কম্পিউটার পুনরায় চালু করতে হবে।%n%nকম্পিউটার পুনরায় চালু করার পরে, [name]-এর ইনস্টলেশন সম্পন্ন করতে আবার সেটআপ চালান।
CannotContinue=সেটআপ চালিয়ে যেতে পারছে না। প্রস্থান করতে Cancel ক্লিক করুন।
ApplicationsFound=নিচের অ্যাপ্লিকেশনগুলো এমন ফাইল ব্যবহার করছে যেগুলো সেটআপ আপডেট করতে হবে। সেটআপকে স্বয়ংক্রিয়ভাবে এই অ্যাপ্লিকেশনগুলো বন্ধ করার অনুমতি দেওয়ার পরামর্শ দেওয়া হচ্ছে।
ApplicationsFound2=নিচের অ্যাপ্লিকেশনগুলো এমন ফাইল ব্যবহার করছে যেগুলো সেটআপ আপডেট করতে হবে। সেটআপকে স্বয়ংক্রিয়ভাবে এই অ্যাপ্লিকেশনগুলো বন্ধ করার অনুমতি দেওয়ার পরামর্শ দেওয়া হচ্ছে। ইনস্টলেশন সম্পন্ন হওয়ার পরে, সেটআপ অ্যাপ্লিকেশনগুলো পুনরায় চালু করার চেষ্টা করবে।
CloseApplications=অ্যাপ্লিকেশনগুলো &স্বয়ংক্রিয়ভাবে বন্ধ করুন
DontCloseApplications=অ্যাপ্লিকেশনগুলো &বন্ধ করবেন না
ErrorCloseApplications=সেটআপ স্বয়ংক্রিয়ভাবে সকল অ্যাপ্লিকেশন বন্ধ করতে পারেনি। চালিয়ে যাওয়ার আগে সেটআপ আপডেট করতে হবে এমন ফাইল ব্যবহার করছে এমন সকল অ্যাপ্লিকেশন বন্ধ করার পরামর্শ দেওয়া হচ্ছে।
PrepareToInstallNeedsRestart=সেটআপকে অবশ্যই আপনার কম্পিউটার পুনরায় চালু করতে হবে। কম্পিউটার পুনরায় চালু করার পরে, [name]-এর ইনস্টলেশন সম্পন্ন করতে আবার সেটআপ চালান।%n%nআপনি কি এখন পুনরায় চালু করতে চান?

; *** "ইনস্টল হচ্ছে" উইজার্ড পেজ
WizardInstalling=ইনস্টল হচ্ছে
InstallingLabel=সেটআপ আপনার কম্পিউটারে [name] ইনস্টল করার সময় অপেক্ষা করুন।

; *** "সেটআপ সম্পন্ন" উইজার্ড পেজ
FinishedHeadingLabel=[name] সেটআপ উইজার্ড সম্পন্ন করা হচ্ছে
FinishedLabelNoIcons=সেটআপ আপনার কম্পিউটারে [name] ইনস্টল করা সম্পন্ন করেছে।
FinishedLabel=সেটআপ আপনার কম্পিউটারে [name] ইনস্টল করা সম্পন্ন করেছে। ইনস্টল করা শর্টকাটগুলো নির্বাচন করে অ্যাপ্লিকেশনটি চালু করা যাবে।
ClickFinish=সেটআপ থেকে প্রস্থান করতে Finish ক্লিক করুন।
FinishedRestartLabel=[name]-এর ইনস্টলেশন সম্পন্ন করতে, সেটআপকে অবশ্যই আপনার কম্পিউটার পুনরায় চালু করতে হবে। আপনি কি এখন পুনরায় চালু করতে চান?
FinishedRestartMessage=[name]-এর ইনস্টলেশন সম্পন্ন করতে, সেটআপকে অবশ্যই আপনার কম্পিউটার পুনরায় চালু করতে হবে।%n%nআপনি কি এখন পুনরায় চালু করতে চান?
ShowReadmeCheck=হ্যাঁ, আমি README ফাইলটি দেখতে চাই
YesRadio=&হ্যাঁ, এখনই কম্পিউটার পুনরায় চালু করুন
NoRadio=&না, আমি পরে কম্পিউটার পুনরায় চালু করব
; উদাহরণস্বরূপ 'Run MyProg.exe' হিসেবে ব্যবহৃত
RunEntryExec=%1 চালান
; উদাহরণস্বরূপ 'View Readme.txt' হিসেবে ব্যবহৃত
RunEntryShellExec=%1 দেখুন

; *** "সেটআপের পরবর্তী ডিস্ক প্রয়োজন" সংক্রান্ত বিষয়
ChangeDiskTitle=সেটআপের পরবর্তী ডিস্ক প্রয়োজন
SelectDiskLabel2=অনুগ্রহ করে ডিস্ক %1 প্রবেশ করান এবং OK ক্লিক করুন।%n%nযদি এই ডিস্কের ফাইলগুলো নিচে প্রদর্শিত ফোল্ডার ছাড়া অন্য কোথাও পাওয়া যায়, তাহলে সঠিক পাথ লিখুন বা Browse ক্লিক করুন।
PathLabel=&পাথ:
FileNotInDir2="%2"-এ "%1" ফাইলটি খুঁজে পাওয়া যায়নি। অনুগ্রহ করে সঠিক ডিস্ক প্রবেশ করান বা অন্য একটি ফোল্ডার নির্বাচন করুন।
SelectDirectoryLabel=অনুগ্রহ করে পরবর্তী ডিস্কের অবস্থান নির্দিষ্ট করুন।

; *** ইনস্টলেশন পর্যায়ের বার্তা
SetupAborted=সেটআপ সম্পন্ন হয়নি।%n%nঅনুগ্রহ করে সমস্যাটি সমাধান করুন এবং আবার সেটআপ চালান।
AbortRetryIgnoreSelectAction=ক্রিয়া নির্বাচন করুন
AbortRetryIgnoreRetry=আবার &চেষ্টা করুন
AbortRetryIgnoreIgnore=ত্রুটিটি &উপেক্ষা করুন এবং চালিয়ে যান
AbortRetryIgnoreCancel=ইনস্টলেশন বাতিল করুন
RetryCancelSelectAction=ক্রিয়া নির্বাচন করুন
RetryCancelRetry=আবার &চেষ্টা করুন
RetryCancelCancel=বাতিল

; *** ইনস্টলেশন স্ট্যাটাস বার্তা
StatusClosingApplications=অ্যাপ্লিকেশন বন্ধ করা হচ্ছে...
StatusCreateDirs=ডিরেক্টরি তৈরি করা হচ্ছে...
StatusExtractFiles=ফাইল এক্সট্র্যাক্ট করা হচ্ছে...
StatusDownloadFiles=ফাইল ডাউনলোড হচ্ছে...
StatusCreateIcons=শর্টকাট তৈরি করা হচ্ছে...
StatusCreateIniEntries=INI এন্ট্রি তৈরি করা হচ্ছে...
StatusCreateRegistryEntries=রেজিস্ট্রি এন্ট্রি তৈরি করা হচ্ছে...
StatusRegisterFiles=ফাইল নিবন্ধন করা হচ্ছে...
StatusSavingUninstall=আনইনস্টল তথ্য সংরক্ষণ করা হচ্ছে...
StatusRunProgram=ইনস্টলেশন সম্পন্ন করা হচ্ছে...
StatusRestartingApplications=অ্যাপ্লিকেশন পুনরায় চালু করা হচ্ছে...
StatusRollback=পরিবর্তনগুলো রোলব্যাক করা হচ্ছে...

; *** বিবিধ ত্রুটি
ErrorInternal2=অভ্যন্তরীণ ত্রুটি: %1
ErrorFunctionFailedNoCode=%1 ব্যর্থ হয়েছে
ErrorFunctionFailed=%1 ব্যর্থ হয়েছে; কোড %2
ErrorFunctionFailedWithMessage=%1 ব্যর্থ হয়েছে; কোড %2.%n%3
ErrorExecutingProgram=ফাইলটি চালানো সম্ভব হয়নি:%n%1

; *** রেজিস্ট্রি ত্রুটি
ErrorRegOpenKey=রেজিস্ট্রি কী খুলতে ত্রুটি:%n%1\%2
ErrorRegCreateKey=রেজিস্ট্রি কী তৈরি করতে ত্রুটি:%n%1\%2
ErrorRegWriteKey=রেজিস্ট্রি কী-তে লিখতে ত্রুটি:%n%1\%2

; *** INI ত্রুটি
ErrorIniEntry="%1" ফাইলে INI এন্ট্রি তৈরি করতে ত্রুটি।

; *** ফাইল কপি করার ত্রুটি
FileAbortRetryIgnoreSkipNotRecommended=এই ফাইলটি &এড়িয়ে যান (প্রস্তাবিত নয়)
FileAbortRetryIgnoreIgnoreNotRecommended=ত্রুটিটি &উপেক্ষা করুন এবং চালিয়ে যান (প্রস্তাবিত নয়)
SourceIsCorrupted=উৎস ফাইলটি দূষিত
SourceDoesntExist=উৎস ফাইল "%1" বিদ্যমান নেই
SourceVerificationFailed=উৎস ফাইলের যাচাইকরণ ব্যর্থ হয়েছে: %1
VerificationSignatureDoesntExist=সিগনেচার ফাইল "%1" বিদ্যমান নেই
VerificationSignatureInvalid=সিগনেচার ফাইল "%1" অবৈধ
VerificationKeyNotFound=সিগনেচার ফাইল "%1" একটি অজানা কী ব্যবহার করে
VerificationFileNameIncorrect=ফাইলটির নাম সঠিক নয়
VerificationFileTagIncorrect=ফাইলটির ট্যাগ সঠিক নয়
VerificationFileSizeIncorrect=ফাইলটির আকার সঠিক নয়
VerificationFileHashIncorrect=ফাইলটির হ্যাশ সঠিক নয়
ExistingFileReadOnly2=বিদ্যমান ফাইলটি প্রতিস্থাপন করা সম্ভব হয়নি কারণ এটি রিড-অনলি হিসেবে চিহ্নিত।
ExistingFileReadOnlyRetry=রিড-অনলি &অ্যাট্রিবিউট সরান এবং আবার চেষ্টা করুন
ExistingFileReadOnlyKeepExisting=বিদ্যমান ফাইলটি &রাখুন
ErrorReadingExistingDest=বিদ্যমান ফাইলটি পড়ার চেষ্টা করার সময় একটি ত্রুটি ঘটেছে:
FileExistsSelectAction=ক্রিয়া নির্বাচন করুন
FileExists2=ফাইলটি ইতিমধ্যে বিদ্যমান।
FileExistsOverwriteExisting=বিদ্যমান ফাইলটি &ওভাররাইট করুন
FileExistsKeepExisting=বিদ্যমান ফাইলটি &রাখুন
FileExistsOverwriteOrKeepAll=পরবর্তী দ্বন্দ্বগুলোর জন্য &এটি করুন
ExistingFileNewerSelectAction=ক্রিয়া নির্বাচন করুন
ExistingFileNewer2=বিদ্যমান ফাইলটি সেটআপ যে ফাইলটি ইনস্টল করার চেষ্টা করছে তার চেয়ে নতুন।
ExistingFileNewerOverwriteExisting=বিদ্যমান ফাইলটি &ওভাররাইট করুন
ExistingFileNewerKeepExisting=বিদ্যমান ফাইলটি &রাখুন (প্রস্তাবিত)
ExistingFileNewerOverwriteOrKeepAll=পরবর্তী দ্বন্দ্বগুলোর জন্য &এটি করুন
ErrorChangingAttr=বিদ্যমান ফাইলের অ্যাট্রিবিউট পরিবর্তন করার চেষ্টা করার সময় একটি ত্রুটি ঘটেছে:
ErrorCreatingTemp=গন্তব্য ডিরেক্টরিতে একটি ফাইল তৈরি করার চেষ্টা করার সময় একটি ত্রুটি ঘটেছে:
ErrorReadingSource=উৎস ফাইলটি পড়ার চেষ্টা করার সময় একটি ত্রুটি ঘটেছে:
ErrorCopying=একটি ফাইল কপি করার চেষ্টা করার সময় একটি ত্রুটি ঘটেছে:
ErrorDownloading=একটি ফাইল ডাউনলোড করার চেষ্টা করার সময় একটি ত্রুটি ঘটেছে:
ErrorExtracting=একটি আর্কাইভ এক্সট্র্যাক্ট করার চেষ্টা করার সময় একটি ত্রুটি ঘটেছে:
ErrorReplacingExistingFile=বিদ্যমান ফাইলটি প্রতিস্থাপন করার চেষ্টা করার সময় একটি ত্রুটি ঘটেছে:
ErrorRestartReplace=RestartReplace ব্যর্থ হয়েছে:
ErrorRenamingTemp=গন্তব্য ডিরেক্টরিতে একটি ফাইলের নাম পরিবর্তন করার চেষ্টা করার সময় একটি ত্রুটি ঘটেছে:
ErrorRegisterServer=DLL/OCX নিবন্ধন করা সম্ভব হয়নি: %1
ErrorRegSvr32Failed=RegSvr32 এক্সিট কোড %1 দিয়ে ব্যর্থ হয়েছে
ErrorRegisterTypeLib=টাইপ লাইব্রেরি নিবন্ধন করা সম্ভব হয়নি: %1

; *** আনইনস্টল ডিসপ্লে নাম চিহ্নিতকরণ
; উদাহরণস্বরূপ 'My Program (32-bit)' হিসেবে ব্যবহৃত
UninstallDisplayNameMark=%1 (%2)
; উদাহরণস্বরূপ 'My Program (32-bit, All users)' হিসেবে ব্যবহৃত
UninstallDisplayNameMarks=%1 (%2, %3)
UninstallDisplayNameMark32Bit=32-বিট
UninstallDisplayNameMark64Bit=64-বিট
UninstallDisplayNameMarkAllUsers=সকল ব্যবহারকারী
UninstallDisplayNameMarkCurrentUser=বর্তমান ব্যবহারকারী

; *** পোস্ট-ইনস্টলেশন ত্রুটি
ErrorOpeningReadme=README ফাইলটি খোলার চেষ্টা করার সময় একটি ত্রুটি ঘটেছে।
ErrorRestartingComputer=সেটআপ কম্পিউটার পুনরায় চালু করতে পারেনি। অনুগ্রহ করে এটি ম্যানুয়ালি করুন।

; *** আনইনস্টলার বার্তা
UninstallNotFound="%1" ফাইলটি বিদ্যমান নেই। আনইনস্টল করা সম্ভব নয়।
UninstallOpenError="%1" ফাইলটি খোলা সম্ভব হয়নি। আনইনস্টল করা সম্ভব নয়
UninstallUnsupportedVer=আনইনস্টল লগ ফাইল "%1" এমন একটি ফরম্যাটে আছে যা আনইনস্টলারের এই সংস্করণ সনাক্ত করতে পারে না। আনইনস্টল করা সম্ভব নয়
UninstallUnknownEntry=আনইনস্টল লগে একটি অজানা এন্ট্রি (%1) পাওয়া গেছে
ConfirmUninstall=আপনি কি নিশ্চিতভাবে %1 এবং এর সমস্ত কম্পোনেন্ট সম্পূর্ণরূপে সরাতে চান?
UninstallOnlyOnWin64=এই ইনস্টলেশনটি শুধুমাত্র 64-বিট Windows-এ আনইনস্টল করা যাবে।
OnlyAdminCanUninstall=এই ইনস্টলেশনটি শুধুমাত্র প্রশাসনিক সুবিধাসম্পন্ন ব্যবহারকারী দ্বারা আনইনস্টল করা যাবে।
UninstallStatusLabel=আপনার কম্পিউটার থেকে %1 সরানোর সময় অপেক্ষা করুন।
UninstalledAll=%1 আপনার কম্পিউটার থেকে সফলভাবে সরানো হয়েছে।
UninstalledMost=%1 আনইনস্টল সম্পন্ন।%n%nকিছু উপাদান সরানো সম্ভব হয়নি। এগুলো ম্যানুয়ালি সরানো যাবে।
UninstalledAndNeedsRestart=%1-এর আনইনস্টলেশন সম্পন্ন করতে, আপনার কম্পিউটার অবশ্যই পুনরায় চালু করতে হবে।%n%nআপনি কি এখন পুনরায় চালু করতে চান?
UninstallDataCorrupted="%1" ফাইলটি দূষিত। আনইনস্টল করা সম্ভব নয়

; *** আনইনস্টলেশন পর্যায়ের বার্তা
ConfirmDeleteSharedFileTitle=শেয়ার্ড ফাইল সরাবেন?
ConfirmDeleteSharedFile2=সিস্টেম ইঙ্গিত করছে যে নিচের শেয়ার্ড ফাইলটি আর কোনো প্রোগ্রাম ব্যবহার করছে না। আপনি কি আনইনস্টলকে এই শেয়ার্ড ফাইলটি সরাতে দিতে চান?%n%nযদি কোনো প্রোগ্রাম এখনও এই ফাইলটি ব্যবহার করে এবং এটি সরানো হয়, তাহলে সেই প্রোগ্রামগুলো সঠিকভাবে কাজ নাও করতে পারে। আপনি যদি নিশ্চিত না হন, তাহলে No নির্বাচন করুন। ফাইলটি আপনার সিস্টেমে রাখলে কোনো ক্ষতি হবে না।
SharedFileNameLabel=ফাইলের নাম:
SharedFileLocationLabel=অবস্থান:
WizardUninstalling=আনইনস্টল স্ট্যাটাস
StatusUninstalling=%1 আনইনস্টল হচ্ছে...

; *** শাটডাউন ব্লক কারণ
ShutdownBlockReasonInstallingApp=%1 ইনস্টল হচ্ছে।
ShutdownBlockReasonUninstallingApp=%1 আনইনস্টল হচ্ছে।

; নিচের কাস্টম বার্তাগুলো Setup নিজে ব্যবহার করে না, কিন্তু আপনি যদি
; আপনার স্ক্রিপ্টে এগুলো ব্যবহার করেন, তাহলে আপনি এগুলো অনুবাদ করতে চাইবেন।

[CustomMessages]

NameAndVersion=%1 সংস্করণ %2
AdditionalIcons=অতিরিক্ত শর্টকাট:
CreateDesktopIcon=একটি &ডেস্কটপ শর্টকাট তৈরি করুন
CreateQuickLaunchIcon=একটি &Quick Launch শর্টকাট তৈরি করুন
ProgramOnTheWeb=ওয়েবে %1
UninstallProgram=%1 আনইনস্টল করুন
LaunchProgram=%1 চালু করুন
AssocFileExtension=%2 ফাইল এক্সটেনশনের সাথে %1 &যুক্ত করুন
AssocingFileExtension=%2 ফাইল এক্সটেনশনের সাথে %1 যুক্ত করা হচ্ছে...
AutoStartProgramGroupDescription=স্টার্টআপ:
AutoStartProgram=স্বয়ংক্রিয়ভাবে %1 চালু করুন
AddonHostProgramNotFound=আপনি যে ফোল্ডারটি নির্বাচন করেছেন তাতে %1 খুঁজে পাওয়া যায়নি।%n%nআপনি কি তবুও চালিয়ে যেতে চান?

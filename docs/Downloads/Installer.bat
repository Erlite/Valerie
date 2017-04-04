@echo off
color 0c
title Discord Bot Setup
echo ----------------------------------------------------
echo         Welcome to Discord Bot Setup!
echo ----------------------------------------------------
echo.
echo GETTING READY TO SETUP YOUR BOT!
::timeout /t 5 /nobreak >nul
echo Creating Directories ...
md DiscordBot\Configs\Guilds\Default
timeout /t 5 /nobreak >nul
echo Creating Bot Config.json ...
echo {"BotToken":"","DefaultCommandPrefix":"","BingAPI":"","EnableDebugMode":false}>>"DiscordBot\Configs\config.json"
timeout /t 5 /nobreak >nul
echo Creating Guild Config.json ...
echo {"CommandPrefix":"","WelcomeMessage":"","ModChannelID":1234567890,"AutoRespond":{"Enabled":true},"EventsLog":{"JoinLog":true,"LeaveLog":true,"BanLog":true,"EventTextChannel":1234567890}}>>"DiscordBot\Configs\Guilds\Default\config.json"
timeout /t 5 /nobreak >nul
echo Creating Responses.json ...
echo {"Hello":"Hey!","Hi":"Ahoy!"}>>"DiscordBot\Configs\Guilds\Default\Responses.json"
timeout /t 5 /nobreak >nul
echo Done! Please fill up the details in created configs!
echo.
set /p=Once done please press enter!
echo Fetching Bot files ...
powershell -command "(new-object System.Net.WebClient).DownloadFile('https://exceptiondev.github.io/DiscordExampleBot/Downloads/DiscordBot.zip', '%cd%\DiscordBot\DiscordBot.zip')"
timeout /t 5 /nobreak >nul
echo Bot files succesfully fetched!
echo Performing extraction process ...
Call :UnZipFile "%cd%\DiscordBot\" "%cd%\DiscordBot\DiscordBot.zip"
timeout /t 5 /nobreak >nul
del "%cd%\DiscordBot\DiscordBot.zip"
start %cd%\DiscordBot\DiscordBot.exe
timeout /t 5 /nobreak >nul
echo Bot started!
echo Press any key to exit setup ...
pause >nul

:UnZipFile <ExtractTo> <newzipfile>
set vbs="%temp%\_.vbs"
if exist %vbs% del /f /q %vbs%
>%vbs%  echo Set fso = CreateObject("Scripting.FileSystemObject")
>>%vbs% echo If NOT fso.FolderExists(%1) Then
>>%vbs% echo fso.CreateFolder(%1)
>>%vbs% echo End If
>>%vbs% echo set objShell = CreateObject("Shell.Application")
>>%vbs% echo set FilesInZip=objShell.NameSpace(%2).items
>>%vbs% echo objShell.NameSpace(%1).CopyHere(FilesInZip)
>>%vbs% echo Set fso = Nothing
>>%vbs% echo Set objShell = Nothing
cscript //nologo %vbs%
if exist %vbs% del /f /q %vbs%
echo off

set OpenHomeDir=..\..\openhome
set release=1
set buildOhNet=1

if 1==%buildOhNet% ( call :Make %OpenHomeDir% %~dp0 ohNet %release% )
call :Make %OpenHomeDir% %~dp0 ohGit %release%
call :Make %OpenHomeDir% %~dp0 ohXen %release%
call :Make %OpenHomeDir% %~dp0 ohXapp %release%
call :GoWaf %OpenHomeDir% %~dp0 ohTopology %release%
call :Make %OpenHomeDir% %~dp0 ohPlaylistManager %release%
call :Make %OpenHomeDir% %~dp0 ohMediaToolbox %release%
REM call :GoWafohOs %OpenHomeDir% %~dp0 ohOs %release%

call :CopyLibraries %OpenHomeDir% %~dp0 %release%

GOTO:EOF


:Make
echo make %~3 release=%~4
cd %~1
cd %~3
call make clean release=%~4
call make release=%~4
cd %~2
GOTO:EOF


:GoWaf
echo waf %~3
cd %~1
cd %~3
set debug=""
if 0==%~4 ( set debug=--debug )
call go fetch --clean --all
call waf configure --dest-platform=Windows-x86 --ohnet=../ohNet %debug%
call waf clean build
cd %~2
GOTO:EOF

:GoWafohOs
echo go waf %~3
cd %~1
cd %~3
call go fetch
call waf configure
call waf clean build
cd %~2
GOTO:EOF

:CopyLibraries
echo Copying Libraries
cd %~1
set variant=Release
if 0==%~3 (set variant=Debug)

REM === SONGBOX ===
copy ohNet\Build\Obj\Windows\%variant%\ohNet.dll %~2\Songbox\OpenHome\Windows
copy ohNet\Build\Obj\Windows\%variant%\ohNet.net.dll %~2\Songbox\OpenHome\Windows
copy ohNet\Build\Obj\Windows\%variant%\DvAvOpenhomeOrgPlaylistManager1.net.dll %~2\Songbox\OpenHome\Windows
copy ohNet\Build\Obj\Windows\%variant%\DvUpnpOrgConnectionManager1.net.dll %~2\Songbox\OpenHome\Windows
copy ohNet\Build\Obj\Windows\%variant%\DvUpnpOrgContentDirectory1.net.dll %~2\Songbox\OpenHome\Windows
copy ohGit\Build\Obj\Windows\%variant%\ohGit.dll %~2\Songbox\OpenHome\Windows
copy ohXapp\Build\Obj\Windows\%variant%\ohXapp.dll %~2\Songbox\OpenHome\Windows
copy ohXapp\Build\Obj\Windows\%variant%\ohXappViewer.dll %~2\Songbox\OpenHome\Windows	
copy ohXen\Build\Obj\Windows\%variant%\ohXen.dll %~2\Songbox\OpenHome\Windows
copy ohPlaylistManager\Build\Obj\Windows\%variant%\ohPlaylistManager.dll %~2\Songbox\OpenHome\Windows
copy ohMediaToolbox\Build\Obj\Windows\%variant%\ohMediaServer.dll %~2\Songbox\OpenHome\Windows

REM === SONGCAST ===
copy ohXen\Build\Obj\Windows\%variant%\ohXen.dll %~2\Songcast\OpenHome\Windows
copy ohXapp\Build\Obj\Windows\%variant%\ohXapp.dll %~2\Songcast\OpenHome\Windows
copy ohXapp\Build\Obj\Windows\%variant%\ohXappViewer.dll %~2\Songcast\OpenHome\Windows
	
REM === WIZARD ===
copy ohXen\Build\Obj\Windows\%variant%\ohXen.dll %~2\Wizard\OpenHome\Windows
copy ohXapp\Build\Obj\Windows\%variant%\ohXapp.dll %~2\Wizard\OpenHome\Windows
copy ohXapp\Build\Obj\Windows\%variant%\ohXappViewer.dll %~2\Wizard\OpenHome\Windows

REM === KONFIG ===
copy ohNet\Build\Obj\Windows\%variant%\ohNet.dll %~2\Konfig\OpenHome\Windows
copy ohNet\Build\Obj\Windows\%variant%\ohNet.net.dll %~2\Konfig\OpenHome\Windows
copy ohNet\Build\Obj\Windows\%variant%\CpAvOpenhomeOrgProduct1.net.dll %~2\Konfig\OpenHome\Windows
REM TODO copy ohNet\Build\Obj\Windows\%variant%\CpLinnCoUkDiagnostics1.net.dll %~2\Konfig\OpenHome\Windows
REM TODO copy ohNet\Build\Obj\Windows\%variant%\CpLinnCoUkVolkano1.net.dll %~2\Konfig\OpenHome\Windows
copy ohXapp\Build\Obj\Windows\%variant%\ohXapp.dll %~2\Konfig\OpenHome\Windows
copy ohXapp\Build\Obj\Windows\%variant%\ohXappViewer.dll %~2\Konfig\OpenHome\Windows	
copy ohXen\Build\Obj\Windows\%variant%\ohXen.dll %~2\Konfig\OpenHome\Windows


cd %~2
GOTO:EOF

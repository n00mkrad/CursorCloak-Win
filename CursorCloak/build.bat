@ECHO OFF
CD /D "%~dp0"

:: Get name of the directory the script is in (not full path, only name)
for %%D in ("%~dp0.") do SET "PROJNAME=%%~nD"
ECHO Building %PROJNAME%

:: Check if process is running, abort if it is
TASKLIST /FI "IMAGENAME eq %PROJNAME%.exe" 2>NUL | find /I /N "%PROJNAME%.exe">NUL
if "%ERRORLEVEL%"=="0" (
	ECHO %PROJNAME% is running, can't build.
	exit /b
)

if "%1"=="sc" (
	SET "SC=true"
	ECHO Building as self-contained executable.
) else (
	SET "SC=false"
	ECHO Building as framework-dependent executable.
)

SET "SHARED_ARGS=dotnet publish ./%PROJNAME%.csproj -c Release -p:WarningLevel=0 -p:DebugType=none -p:PublishSingleFile=true -p:SelfContained=%SC%"
SET "PUBLISH_WIN=%SHARED_ARGS% -r win-x64 -o bin/BuildWin"

ECHO Building Windows executable...
%PUBLISH_WIN%

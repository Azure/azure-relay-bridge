rem @echo off

echo %~1 %~2 %~3 %~4 %~5 %~6

if not "%~1" == "" set Operation=%~1
if not "%~2" == "" set ImageName=%~2
if not "%~3" == "" set ImageSuffix=%~3
if not "%~4" == "" set VersionPrefix=%~4
if not "%~5" == "" set VersionSuffix=%~5
if not "%~6" == "" set TargetFramework=%~6

if "%Operation%"=="" set Operation=build
if "%ImageName%"=="" set ImageName=linux-x64
if "%ImageSuffix%"=="" set ImageSuffix=deb
if "%VersionSuffix%"=="" set VersionSuffix=preview
if "%VersionPrefix%"=="" set VersionPrefix=1.0.0
if "%TargetFramework%"=="" set TargetFramework=net8.0

pushd "%~dp0"

set PackageName=azbridge.%VersionPrefix%-%VersionSuffix%.%ImageName%.%ImageSuffix%
set DirName=azbridge.%VersionPrefix%-%VersionSuffix%.%ImageName%
set _MOUNTPATH=%cd%

if "%Operation%"=="build" (
    
    if NOT exist ..\..\artifacts\build\%TargetFramework%\%PackageName% (
        echo Cannot find ..\..\artifacts\build\%TargetFramework%\%PackageName% 
        exit 2
    )
    if not exist %DirName% mkdir %DirName%    
    unzip -d %DirName% ..\..\artifacts\build\%TargetFramework%\%PackageName%
    npm install
    popd
    exit /b
)

if "%Operation%"=="test" (
    goto test
)

if "%Operation%"=="clean" (
   goto clean
)
else (
    echo "Unknown operation"
    popd
)
exit

:test 

echo on
set _CHK=%AZBRIDGE_TEST_CXNSTRING:"=%
if "%_CHK%" == "" ( 
    echo AZBRIDGE_TEST_CXNSTRING environment variable must be set to valid relay connection string
    exit /b
)

powershell ./run-test.ps1 %DirName%

if ERRORLEVEL 1 (
    set _RESULT=%ERRORLEVEL%
    echo Error %_RESULT%
)
if ERRORLEVEL 0 (
    set _RESULT=0
    echo OK
)
popd
exit _RESULT

:clean

if exist %DirName% rd /s /q %DirName%
if exist node_modules rd /s /q node_modules
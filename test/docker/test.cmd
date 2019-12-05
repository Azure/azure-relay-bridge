rem @echo off

echo %~1 %~2 %~3 %~4 %~5 %~6

if not "%~1" == "" set Operation=%~1
if not "%~2" == "" set ImageName=%~2
if not "%~3" == "" set ImageSuffix=%~3
if not "%~4" == "" set VersionPrefix=%~4
if not "%~5" == "" set VersionSuffix=%~5
if not "%~6" == "" set TargetFramework=%~6

if "%Operation%"=="" set Operation=build
if "%ImageName%"=="" set ImageName=debian.9-x64
if "%ImageSuffix%"=="" set ImageSuffix=deb
if "%VersionSuffix%"=="" set VersionSuffix=preview
if "%VersionPrefix%"=="" set VersionPrefix=1.0.0
if "%TargetFramework%"=="" set TargetFramework=netcoreapp3.0

set PackageName=azbridge.%VersionPrefix%-%VersionSuffix%.%ImageName%.%ImageSuffix%

if "%Operation%"=="build" (
    pushd "%~dp0"
    if NOT exist ..\..\artifacts\build\%TargetFramework%\%PackageName% (
        echo Cannot find ..\..\artifacts\build\%TargetFramework%\%PackageName% 
        exit 2
    )
    if not exist "tmp" mkdir tmp    
    copy /y ..\..\artifacts\build\%TargetFramework%\%PackageName% tmp > NUL
    docker build -f %ImageName%.dockerfile . --tag azbridge-test-%ImageName% --build-arg package_name=%PackageName%
    rd /s /q tmp
    popd
    exit /b
)

if "%Operation%"=="test" (
    
    if %AZBRIDGE_TEST_CXNSTRING% == "" ( 
        echo AZBRIDGE_TEST_CXNSTRING environment variable must be set to valid relay connection string
        exit /b
    )

    set _IMAGE_ID=

    FOR /F %%i IN ("%cd%") DO set _MOUNTPATH=%%~fi
    set _TESTNAME=test_nc_ping_pong
    call runtest.cmd
    if NOT "%_RESULT%"=="0" exit /b %_RESULT%

    set _TESTNAME=test_nc_config_ping_pong
    call runtest.cmd
    exit /b %_RESULT%
)

if "%Operation%"=="clean" (
    set IMAGE_NAME=azbridge-test-%ImageName%
    for /f %%i in ('docker images %IMAGE_NAME% -q') do set _IMAGE_ID=%%i
    if "%_IMAGE_ID%"=="" exit /b
    docker rmi -f %IMAGE_NAME%:latest
)
else (
    echo "Unknown operation"
)
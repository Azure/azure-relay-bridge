rem @echo off

echo %~1 %~2 %~3 %~4 %~5 %~6

if not "%~1" == "" set Operation=%~1
if not "%~2" == "" set ImageName=%~2
if not "%~3" == "" set ImageSuffix=%~3
if not "%~4" == "" set VersionPrefix=%~4
if not "%~5" == "" set VersionSuffix=%~5
if not "%~6" == "" set TargetFramework=%~6

if "%Operation%"=="" set Operation=build
if "%ImageName%"=="" set ImageName=ubuntu.18.04-x64
if "%ImageSuffix%"=="" set ImageSuffix=deb
if "%VersionSuffix%"=="" set VersionSuffix=preview
if "%VersionPrefix%"=="" set VersionPrefix=1.0.0
if "%TargetFramework%"=="" set TargetFramework=net6.0

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

if '%AZBRIDGE_TEST_CXNSTRING%' == "" ( 
    echo AZBRIDGE_TEST_CXNSTRING environment variable must be set to valid relay connection string
    exit /b
)

rem start the web server
start node index.js
start %DirName%\azbridge.exe -R a1:3000 -c %AZBRIDGE_TEST_CXNSTRING%
for /f %%i in ( srvrun.log ) do set _SERVER_NAME=%%i
rem run the client
docker run -v %_MOUNTPATH%:/tests --rm -e AZBRIDGE_TEST_CXNSTRING=%AZBRIDGE_TEST_CXNSTRING% azbridge-nginx-client-%ImageName%:latest bash /tests/run_client.sh
rem stop the web server
docker stop %_SERVER_NAME%

fc /L downloaded.txt index.html > NUL
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

set IMAGE_NAME=%ImageName%

for /f %%i in ('docker images azbridge-nginx-server-%IMAGE_NAME% -q') do set _IMAGE_ID=%%i
if "%_IMAGE_ID%"=="" exit /b
docker rmi -f azbridge-nginx-server-%ImageName%:latest

for /f %%i in ('docker images azbridge-nginx-client-%IMAGE_NAME% -q') do set _IMAGE_ID=%%i
if "%_IMAGE_ID%"=="" exit /b
docker rmi -f azbridge-nginx-client-%ImageName%:latest
popd

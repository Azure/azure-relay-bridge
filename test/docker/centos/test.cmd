@echo off

SET _CXNSTRING=
FOR /F "delims=" %%i IN (%RELAY_CXNSTRING%) DO SET _CXNSTRING=%%i

if not "%*" == "" (
   set _CXNSTRING="%*"
) else if "%_CXNSTRING%" == "" ( 
    echo RELAY_CXNSTRING environment variable must be set to valid relay connection string
    exit /b
)

SET IMAGE_NAME=azbridge_centos_test

pushd "%~dp0"
set _IMAGE_ID=

for /f %%i in ('docker images %IMAGE_NAME% -q') do set _IMAGE_ID=%%i
if "%_IMAGE_ID%"=="" call build.cmd

FOR /F %%i IN ("%cd%\..") DO set _MOUNTPATH=%%~fi
docker run -it -v %_MOUNTPATH%:/tests -e RELAY_CXNSTRING="%_CXNSTRING%" --rm %IMAGE_NAME%:latest sh /tests/test_nc_ping_pong.sh

set _IMAGE_ID=
set _CXNSTRING=
popd
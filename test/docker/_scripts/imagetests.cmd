@echo off

SET _CXNSTRING=
FOR /F "delims=" %%i IN (%AZBRIDGE_TEST_CXNSTRING%) DO SET _CXNSTRING=%%i

if "%_CXNSTRING%" == "" ( 
    echo AZBRIDGE_TEST_CXNSTRING environment variable must be set to valid relay connection string
    exit /b
)

set _IMAGE_ID=

rem for /f %%i in ('docker images %IMAGE_NAME% -q') do set _IMAGE_ID=%%i
rem if "%_IMAGE_ID%"=="" call build.cmd

FOR /F %%i IN ("%cd%\..") DO set _MOUNTPATH=%%~fi
set _TESTNAME=test_nc_ping_pong
call ../_scripts/runtest.cmd
if NOT "%_RESULT%"=="0" exit /b %_RESULT%

set _TESTNAME=test_nc_config_ping_pong
call ../_scripts/runtest.cmd
exit /b %_RESULT%

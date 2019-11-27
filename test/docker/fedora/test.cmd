@echo off

pushd "%~dp0"
SET IMAGE_NAME=azbridge_fedora_test
call ../_scripts/imagetests.cmd %*
popd
exit /b %ERRORLEVEL%

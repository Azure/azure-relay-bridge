rem @echo off

if not "%1" == "" set BuildNumber=%1
if not "%2" == "" set VersionPrefix=%2
if not "%3" == "" set VersionSuffix=%3
if not "%4" == "" set TargetFramework=%4

if "%BuildNumber%"=="" set BuildNumber=0000
if "%VersionSuffix%"=="" set VersionSuffix=preview
if "%VersionPrefix%"=="" set VersionPrefix=1.0.0
if "%TargetFramework%"=="" set TargetFramework=net462

pushd "%~dp0"
if not exist "tmp" mkdir tmp
set MsiFile=azbridge_installer.%VersionPrefix%-%VersionSuffix%.win10-x64.msi
set MsiFilePath=..\..\..\artifacts\build\%TargetFramework%\%MsiFile%
if not exist %MsiFilePath% (
    echo %MsiFilePath% does not exist
    exit /b 1
)
copy /y %MsiFilePath% . > NUL
az acr build --os windows --timeout 3600 --registry azbridgetests --image azbridge_winsrv462_test --build-arg msi_package=%MsiFile% %cd%
del /q %MsiFile%

popd
@echo off

if not "%1" == "" set VersionPrefix=%1
if not "%2" == "" set VersionSuffix=%2
if not "%3" == "" set TargetFramework=%3

if "%VersionSuffix%"=="" set VersionSuffix=preview
if "%VersionPrefix%"=="" set VersionPrefix=1.0.0
if "%TargetFramework%"=="" set TargetFramework="netcoreapp3.0"

pushd "%~dp0"
if not exist "tmp" mkdir tmp
set RpmFile=azbridge.%VersionPrefix%-%VersionSuffix%.fedora-x64.rpm
copy /y ..\..\..\artifacts\build\%TargetFramework%\%RpmFile% tmp > NUL
docker build -f Dockerfile . --tag azbridge_fedora_test --build-arg rpm_package=%RpmFile%
rd /s /q tmp
popd
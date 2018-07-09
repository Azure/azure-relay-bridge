@echo off
if "%BuildNumber%"=="" set BuildNumber=0000
if "%VersionSuffix%"=="" set VersionSuffix=preview
if "%VersionPrefix%"=="" set VersionPrefix=1.0.0
if "%TargetFramework%"=="" set TargetFramework="netcoreapp2.1"

pushd "%~dp0"
if not exist "tmp" mkdir tmp
set RpmFile=azbridge.%VersionPrefix%-%VersionSuffix%-%BuildNumber%.centos-x64.rpm
copy /y ..\..\..\artifacts\build\%TargetFramework%\%RpmFile% tmp > NUL
docker build -f Dockerfile . --tag azbridge_centos_test --build-arg rpm_package=%RpmFile%
rd /s /q tmp
popd
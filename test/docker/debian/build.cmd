@echo off
if "%BuildNumber%"=="" set BuildNumber=0000
if "%VersionSuffix%"=="" set VersionSuffix=preview
if "%VersionPrefix%"=="" set VersionPrefix=1.0.0
if "%TargetFramework%"=="" set TargetFramework="netcoreapp2.1"

pushd "%~dp0"
if not exist "tmp" mkdir tmp
set DebFile=azbridge.%VersionPrefix%-%VersionSuffix%-%BuildNumber%.debian.8-x64.deb
copy /y ..\..\..\artifacts\build\%TargetFramework%\%DebFile% tmp > NUL
docker build -f Dockerfile . --tag azbridge_debian8_test --build-arg deb_package=%DebFile%
rd /s /q tmp

popd
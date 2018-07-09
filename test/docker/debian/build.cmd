@echo off

if not "%1" == "" set BuildNumber=%1
if not "%2" == "" set VersionPrefix=%2
if not "%3" == "" set VersionSuffix=%3
if not "%4" == "" set TargetFramework=%4

if "%BuildNumber%"=="" set BuildNumber=0000
if "%VersionSuffix%"=="" set VersionSuffix=preview
if "%VersionPrefix%"=="" set VersionPrefix=1.0.0
if "%TargetFramework%"=="" set TargetFramework="netcoreapp2.1"

pushd "%~dp0"
if not exist "tmp" mkdir tmp
set DebFile=azbridge.%VersionPrefix%-%VersionSuffix%.debian.8-x64.deb
copy /y ..\..\..\artifacts\build\%TargetFramework%\%DebFile% tmp > NUL
docker build -f Dockerfile . --tag azbridge_debian8_test --build-arg deb_package=%DebFile%
rd /s /q tmp

popd
@echo off

if not "%1" == "" set VersionPrefix=%1
if not "%2" == "" set VersionSuffix=%2
if not "%3" == "" set TargetFramework=%3

if "%VersionSuffix%"=="" set VersionSuffix=preview
if "%VersionPrefix%"=="" set VersionPrefix=1.0.0
if "%TargetFramework%"=="" set TargetFramework="netcoreapp3.0"

pushd "%~dp0"
if not exist "tmp" mkdir tmp
set DebFile=azbridge.%VersionPrefix%-%VersionSuffix%.ubuntu.16.04-x64.deb
copy /y ..\..\..\artifacts\build\%TargetFramework%\%DebFile% tmp > NUL
docker build -f Dockerfile . --tag azbridge_ubuntu1604_test --build-arg deb_package=%DebFile%
rd /s /q tmp

popd
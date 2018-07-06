@echo off
if "%BuildNumber%"=="" set BuildNumber=0000
if "%VersionSuffix%"=="" set VersionSuffix=preview
if "%VersionPrefix%"=="" set VersionPrefix=1.0.0

pushd "%~dp0"
if not exist "tmp" mkdir tmp
set DebFile=azbridge.%VersionPrefix%-%VersionSuffix%-%BuildNumber%.ubuntu.16.04-x64.deb
copy /y ..\..\..\artifacts\build\%DebFile% tmp > NUL
docker build -f Dockerfile . --tag azbridge_ubuntu1604_test --build-arg deb_package=%DebFile%
rd /s /q tmp

popd
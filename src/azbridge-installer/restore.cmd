REM 
REM The WIX project MUST be built using the NETFX project format and 
REM using the 32-bit runtime and we can't use the project-embedded 
REM <PackageReference> format. Therefore we do a nuget package restore
REM here. This is called from the build process in the package step.
REM

@echo off
pushd "%~dp0"
if not exist ".\.nuget" mkdir ".\.nuget"
if not exist ".\.nuget\nuget.exe" powershell -Command "Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile .\.nuget\nuget.exe"
if not exist ".\.nuget\nuget.config" (
    echo ^<?xml version="1.0" encoding="utf-8"?^> > .\.nuget\nuget.config
    echo ^<configuration^>^<packageSources^> >> .\.nuget\nuget.config
    echo ^<add key="NuGet" value="https://api.nuget.org/v3/index.json" protocolVersion="3" /^> >> .\.nuget\nuget.config
    echo ^</packageSources^>^</configuration^> >> .\.nuget\nuget.config
)

".nuget\NuGet.exe" restore packages.config -PackagesDirectory packages -ConfigFile .nuget\nuget.config  -Verbosity detailed
popd
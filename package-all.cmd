@echo off
SET _DOCKER_BUILD="true"
docker -v > NUL
if not errorlevel 0 (
    echo Linux RPM and DEB packaging requires a docker install
    SET _DOCKER_BUILD="false"
)
echo *** Sanity check Windows
dotnet msbuild /t:restore /p:WindowsOnly=true
dotnet test -f net48 %*
if not errorlevel 0 exit /b 1
echo *** Building and packaging Windows Targets

if %_DOCKER_BUILD% == "true" (
   echo *** Windows only
   dotnet msbuild /t:clean,restore,package /p:WindowsOnly=true;Configuration=Release;RuntimeIdentifier=win10-x64 %*
) else (
    echo *** All platforms
    dotnet msbuild /t:Package /p:Configuration=Release /p:WindowsOnly=true /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=win10-x64 %*
    dotnet msbuild /t:Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=osx-x64 %*
    dotnet msbuild /t:Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=debian.9-x64 %*
    dotnet msbuild /t:Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=debian.10-x64 %*
    dotnet msbuild /t:Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=ubuntu.18.04-x64 %*
    dotnet msbuild /t:Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=ubuntu.18.04-arm64 %*
    dotnet msbuild /t:Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=ubuntu.20.04-x64 %*
    dotnet msbuild /t:Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=ubuntu.20.04-arm64 %*
    dotnet msbuild /t:Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=opensuse.15.0-x64 %*
    dotnet msbuild /t:Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=fedora.30-x64 %*
)

if not errorlevel 0 exit /b 1
if %_DOCKER_BUILD% == "true" (
  echo *** Building and packaging Unix/Linux Targets
  docker run --rm -v %cd%:/build mcr.microsoft.com/dotnet/core/sdk:5.0 /build/package.sh %*
)
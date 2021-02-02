@echo off
SET _DOCKER_BUILD="true"
docker -v > NUL
if not errorlevel 0 (
    echo Linux RPM and DEB packaging requires a docker install
    SET _DOCKER_BUILD="false"
)
echo *** Sanity check Windows
dotnet msbuild /t:restore /p:WindowsOnly=false /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=win10-x64
dotnet test -f net5.0 %*
if not errorlevel 0 exit /b 1
echo *** Building and packaging Windows Targets

if %_DOCKER_BUILD% == "true" (
   echo *** Windows only
   dotnet msbuild /t:clean,restore,package /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=win10-x64 /p:WindowsOnly=false;Configuration=Release %*
) else (
    echo *** All platforms
    dotnet msbuild /t:clean,restore,package /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=win10-x64 /p:WindowsOnly=false;Configuration=Release %*
    dotnet msbuild /t:clean,restore,package /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=osx-x64 /p:WindowsOnly=false;Configuration=Release %*
    dotnet msbuild /t:clean,restore,package /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=ubuntu.18.04-x64 /p:WindowsOnly=false;Configuration=Release %*
    dotnet msbuild /t:clean,restore,package /p:TargetFramework=netcoreapp5.0 /p:RuntimeIdentifier=debian.10-x64 /p:WindowsOnly=false;Configuration=Release %*
)

if not errorlevel 0 exit /b 1
if %_DOCKER_BUILD% == "true" (
  echo *** Building and packaging Unix/Linux Targets
  docker run --rm -v %cd%:/build mcr.microsoft.com/dotnet/sdk:5.0 /build/package.sh %*
)
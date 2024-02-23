@echo off
SET _DOCKER_BUILD="true"
docker -v > NUL
if not errorlevel 0 (
    echo Linux RPM and DEB packaging requires a docker install
    SET _DOCKER_BUILD="false"
)
if not errorlevel 0 exit /b 1
echo *** Building and packaging Windows Targets

if %_DOCKER_BUILD% == "true" (
   echo *** Windows only
   dotnet msbuild /t:restore,package /p:Configuration=Release /p:WindowsOnly=true /p:TargetFramework=net8.0 /p:RuntimeIdentifier=win-x64 %*
   dotnet msbuild /t:restore,Package /p:Configuration=Release /p:WindowsOnly=true /p:TargetFramework=net8.0 /p:RuntimeIdentifier=win-arm64 %*
   dotnet msbuild /t:restore,Package /p:Configuration=Release /p:WindowsOnly=true /p:TargetFramework=net8.0 /p:RuntimeIdentifier=win-x86 %*
) else (
    echo *** All platforms
    dotnet msbuild /t:restore,Package /p:Configuration=Release /p:WindowsOnly=true /p:TargetFramework=net8.0 /p:RuntimeIdentifier=win-x64 %*
    dotnet msbuild /t:restore,Package /p:Configuration=Release /p:WindowsOnly=true /p:TargetFramework=net8.0 /p:RuntimeIdentifier=win-arm64 %*
    dotnet msbuild /t:restore,Package /p:Configuration=Release /p:WindowsOnly=true /p:TargetFramework=net8.0 /p:RuntimeIdentifier=win-x86 %*
    dotnet msbuild /t:restore,Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=net8.0 /p:RuntimeIdentifier=osx-x64 %*
    dotnet msbuild /t:restore,Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=net8.0 /p:RuntimeIdentifier=osx-arm64 %*
    dotnet msbuild /t:restore,Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=net8.0 /p:RuntimeIdentifier=linux-x64 %*
    dotnet msbuild /t:restore,Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=net8.0 /p:RuntimeIdentifier=linux-arm64 %*
)

if not errorlevel 0 exit /b 1
if %_DOCKER_BUILD% == "true" (
  echo *** Building and packaging Unix/Linux Targets
  docker run --rm -v %cd%:/build mcr.microsoft.com/dotnet/sdk:8.0 /build/package.sh %*
)
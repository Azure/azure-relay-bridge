@echo off
SET _DOCKER_BUILD=true
docker -v > NUL
if not errorlevel 0 (
    echo Linux RPM and DEB packaging requires a docker install
    SET _DOCKER_BUILD=false
)
echo *** Sanity check Windows
dotnet restore 
dotnet test %*
if not errorlevel 0 exit /b 1
echo *** Building and packaging Windows Targets
if "%_DOCKER_BUILD%" == "true" (
   msbuild /t:clean,restore,package /p:WindowsOnly=true;Configuration=Release %*
)
else (
    msbuild /t:clean,restore,package /p:WindowsOnly=false;Configuration=Release %*
)
if not errorlevel 0 exit /b 1
echo *** Sanity check Unix
docker run --rm -v %cd%:/build microsoft/dotnet:2.1-sdk dotnet restore /build/Microsoft.Azure.Relay.Bridge.sln
docker run --rm -v %cd%:/build microsoft/dotnet:2.1-sdk dotnet test /build/Microsoft.Azure.Relay.Bridge.sln %*
if not errorlevel 0 exit /b 1
echo *** Building and packaging Unix/Linux Targets
docker run --rm -v %cd%:/build microsoft/dotnet:2.1-sdk /build/package.sh %*

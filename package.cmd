@echo off
SET _DOCKER_BUILD=true
docker -v > NUL
if not errorlevel 0 (
    echo Linux RPM and DEB packaging requires a docker install
    SET _DOCKER_BUILD=false
)
if "%_DOCKER_BUILD%" == "true" (
    msbuild /t:clean,restore,package /p:WindowsOnly=true;Configuration=Release %*
    docker run --rm -v %cd%:/build microsoft/dotnet:2.1-sdk /build/package.sh %*
)
else
(
    msbuild /t:clean,restore,package /p:WindowsOnly=false;Configuration=Release %*
)

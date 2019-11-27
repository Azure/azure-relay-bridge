#!/bin/bash
pushd "${0%/*}" > /dev/null 
if [ ! -z $APPVEYOR_BUILD_VERSION ]; then _VersionProp="/p:VersionPrefix=$APPVEYOR_BUILD_VERSION"; fi
dotnet restore
dotnet msbuild /t:clean,restore,build /p:Configuration=Debug /p:TargetFramework=netcoreapp3.0 $_BuildProp $_VersionProp $@
dotnet msbuild /t:clean,restore,build,package /p:Configuration=Release /p:TargetFramework=netcoreapp3.0 $_BuildProp $_VersionProp $@
popd

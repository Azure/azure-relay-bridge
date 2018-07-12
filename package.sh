#!/bin/bash
pushd "${0%/*}" > /dev/null 
if [ ! -z not $APPVEYOR_BUILD_NUMBER ]; then _BuildProp="/p:BuildNumber=$APPVEYOR_BUILD_NUMBER"; fi
if [ ! -z not $APPVEYOR_BUILD_VERSION ]; then _VersionProp="/p:VersionPrefix=$APPVEYOR_BUILD_VERSION"; fi
dotnet msbuild /t:clean,restore,build,package /p:Configuration=Release $_BuildProp $_VersionProp $@
popd

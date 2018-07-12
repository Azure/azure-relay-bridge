#!/bin/bash
pushd "${0%/*}" > /dev/null 
if [ ! -z not $APPVEYOR_BUILD_NUMBER ]; then _BuildProp="/p:BuildNumber=$APPVEYOR_BUILD_NUMBER"; fi
dotnet msbuild /t:clean,restore,build,package /p:Configuration=Release $_BuildProp $@
popd

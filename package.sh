#!/bin/bash
pushd "${0%/*}" > /dev/null 
if [ ! -z not $BuildNumber ]; then _BuildProp="BuildNumber=$BuildNumber"; fi
dotnet msbuild /t:clean,restore,build,package /p:Configuration=Release;$_BuildProp $@
popd

#!/bin/bash
pushd "${0%/*}" > /dev/null 
dotnet msbuild /t:clean,restore,package /p:Configuration=Release $@
popd

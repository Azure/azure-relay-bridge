#!/bin/bash
pushd "${0%/*}" > /dev/null 
dotnet msbuild /t:clean,restore,build,package $@
popd

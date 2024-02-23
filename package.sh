#!/bin/bash
pushd "${0%/*}" > /dev/null 
if [ ! -z $BUILDVERSION ]; then _VersionProp="/p:VersionPrefix=$BUILDVERSION"; fi
dotnet restore
dotnet msbuild /t:Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=net8.0 /p:RuntimeIdentifier=osx-x64 $_BuildProp $_VersionProp $@
dotnet msbuild /t:Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=net8.0 /p:RuntimeIdentifier=osx-arm64 $_BuildProp $_VersionProp $@
dotnet msbuild /t:Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=net8.0 /p:RuntimeIdentifier=linux-x64 $_BuildProp $_VersionProp $@
dotnet msbuild /t:Package /p:Configuration=Release /p:WindowsOnly=false /p:TargetFramework=net8.0 /p:RuntimeIdentifier=linux-arm64 $_BuildProp $_VersionProp $@
popd

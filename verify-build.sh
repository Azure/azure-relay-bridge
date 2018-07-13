#!/bin/bash

if [ ! -z $APPVEYOR_BUILD_NUMBER ]; then _BuildProp="/p:BuildNumber=$APPVEYOR_BUILD_NUMBER"; fi
if [ ! -z $APPVEYOR_BUILD_VERSION ]; then _VersionProp="/p:VersionPrefix=$APPVEYOR_BUILD_VERSION"; fi

dotnet test --verbosity=normal
cd test/docker
dotnet clean --verbosity=normal /p:Configuration=Debug /p:TargetFramework=netcoreapp2.1 $_BuildProp $_VersionProp $@
dotnet build --verbosity=normal /p:Configuration=Debug /p:TargetFramework=netcoreapp2.1 $_BuildProp $_VersionProp $@
dotnet test --verbosity=normal /p:Configuration=Debug /p:TargetFramework=netcoreapp2.1 $_BuildProp $_VersionProp $@

#!/bin/bash

if [ ! -z $APPVEYOR_BUILD_VERSION ]; then _VersionProp="/p:VersionPrefix=$APPVEYOR_BUILD_VERSION"; fi

dotnet test --verbosity=normal
#cd test/docker
#dotnet clean --verbosity=normal /p:Configuration=Debug /p:TargetFramework=netcoreapp3.0 $_BuildProp $_VersionProp $@
#dotnet build --verbosity=normal /p:Configuration=Debug /p:TargetFramework=netcoreapp3.0 $_BuildProp $_VersionProp $@
#dotnet test --verbosity=normal /p:Configuration=Debug /p:TargetFramework=netcoreapp3.0 $_BuildProp $_VersionProp $@

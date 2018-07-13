#!/bin/bash

if [ ! -z $APPVEYOR_BUILD_NUMBER ]; then _BuildProp="/p:BuildNumber=$APPVEYOR_BUILD_NUMBER"; fi
if [ ! -z $APPVEYOR_BUILD_VERSION ]; then _VersionProp="/p:VersionPrefix=$APPVEYOR_BUILD_VERSION"; fi

dotnet ~/.nuget/packages/dotnet-xunit/2.3.1/tools/netcoreapp2.0/xunit.console.dll test/unit/Microsoft.Azure.Relay.Bridge.Tests/bin/Debug/netcoreapp2.1/Microsoft.Azure.Relay.Bridge.Tests.dll -appveyor 
cd test/docker
dotnet clean /p:Configuration=Debug /p:TargetFramework=netcoreapp2.1 $_BuildProp $_VersionProp $@
dotnet build /p:Configuration=Debug /p:TargetFramework=netcoreapp2.1 $_BuildProp $_VersionProp $@
dotnet test /p:Configuration=Debug /p:TargetFramework=netcoreapp2.1 $_BuildProp $_VersionProp $@

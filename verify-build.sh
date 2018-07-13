#!/bin/bash
dotnet ~/.nuget/packages/dotnet-xunit/2.3.1/tools/netcoreapp2.0/xunit.console.dll test/unit/Microsoft.Azure.Relay.Bridge.Tests/bin/Debug/netcoreapp2.1/Microsoft.Azure.Relay.Bridge.Tests.dll -appveyor 
cd test/docker
dotnet clean
dotnet build
dotnet test

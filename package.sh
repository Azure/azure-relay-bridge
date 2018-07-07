#! /bin/sh
cd "$(dirname "$0")"
dotnet msbuild /t:clean,restore,package

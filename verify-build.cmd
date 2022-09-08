pushd "%~dp0"


pushd test\nginx
dotnet msbuild /t:build,vstest
popd





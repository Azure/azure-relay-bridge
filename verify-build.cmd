pushd "%~dp0"

pushd test\node
dotnet msbuild /t:clean,build,vstest
popd

popd
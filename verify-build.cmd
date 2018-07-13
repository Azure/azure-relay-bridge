pushd "%~dp0"
%userprofile%\.nuget\packages\dotnet-xunit\2.3.1\tools\netcoreapp2.0\xunit.console.dll test\unit\Microsoft.Azure.Relay.Bridge.Tests\bin\Debug\net462\Microsoft.Azure.Relay.Bridge.Tests.dll -appveyor 
cd test\docker
msbuild /t:clean,build,vstest
popd

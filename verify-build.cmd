pushd "%~dp0"

set xunitrunner=xunit.console.exe
where %xunitrunner%  2>&1 
if ERRORLEVEL 1 set xunitrunner=C:\Tools\xUnit20\xunit.console.exe
"%xunitrunner%" "test\unit\Microsoft.Azure.Relay.Bridge.Tests\bin\Debug\net462\Microsoft.Azure.Relay.Bridge.Tests.dll" -appveyor

cd test\docker
msbuild /t:clean,build,vstest
popd

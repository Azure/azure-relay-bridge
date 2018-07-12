msbuild /t:clean,restore,package /p:WindowsOnly=true %*
set xunitrunner=xunit.console.exe
%xunitrunner% -? 2>&1 > NUL 
if ERRORLEVEL 1 set xunitrunner=%userprofile%\.nuget\packages\dotnet-xunit\2.3.1\tools\net452\xunit.console.exe

%xunitrunner%  test\unit\Microsoft.Azure.Relay.Bridge.Tests\bin\Debug\net462\Microsoft.Azure.Relay.Bridge.Tests.dll -appveyor
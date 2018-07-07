@echo off
if not exist ".\.nuget" mkdir ".\.nuget"
if not exist ".\.nuget\nuget.exe" powershell -Command "Invoke-WebRequest https://www.nuget.org/nuget.exe -OutFile .\.nuget\nuget.exe"
".nuget\NuGet.exe" restore -PackagesDirectory packages
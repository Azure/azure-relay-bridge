if not "%APPVEYOR_BUILD_VERSION%"=="" set _VersionProp="/p:VersionPrefix=%APPVEYOR_BUILD_VERSION%"
if not "%BUILDVERSION%"=="" set _VersionProp="/p:VersionPrefix=%BUILDVERSION%"
msbuild /t:clean,build,package /p:WindowsOnly=false /p:RuntimeIdentifier=win10-x64 /p:RuntimeIdentifier=win10-x64 /p:Configuration=Release %_BuildProp% %_VersionProp% %*
if not "%APPVEYOR_BUILD_NUMBER%"=="" set _BuildProp="/p:BuildNumber=%APPVEYOR_BUILD_NUMBER%"
if not "%APPVEYOR_BUILD_VERSION%"=="" set _VersionProp="/p:VersionPrefix=%APPVEYOR_BUILD_VERSION%"
msbuild /t:clean,restore,build /p:WindowsOnly=true /p:Configuration=Debug %_BuildProp% %_VersionProp% %*
msbuild /t:clean,restore,build,package /p:WindowsOnly=true /p:Configuration=Release %_BuildProp% %_VersionProp% %*
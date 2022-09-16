if not "%BUILDVERSION%"=="" set _VersionProp="/p:VersionPrefix=%BUILDVERSION%"
msbuild /t:clean,build,package /p:WindowsOnly=true /p:Configuration=Release %_BuildProp% %_VersionProp% %*
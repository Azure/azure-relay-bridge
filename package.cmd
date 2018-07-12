if not "%APPVEYOR_BUILD_NUMBER%"=="" set _BuildProp="/p:BuildNumber=%APPVEYOR_BUILD_NUMBER%"
msbuild /t:clean,restore,build,package /p:WindowsOnly=true;Configuration=Release %_BuildProp% %*
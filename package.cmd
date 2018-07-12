if not "%BuildNumber%"=="" set _BuildProp="/p:BuildNumber=%BuildNumber%"
msbuild /t:clean,restore,build,package /p:WindowsOnly=true;Configuration=Release %_BuildProp% %*
if not "%BuildNumber%"=="" then set _BuildProp="BuildNumber=%BuildNumber%"
msbuild /t:clean,restore,build,package /p:WindowsOnly=true;Configuration=Release;%_BuildProp% %*
pushd "%~dp0"
pushd test\node
dotnet msbuild /t:clean,build,vstest
if ERRORLEVEL 1 (
    set _RESULT=%ERRORLEVEL%
    echo Error %_RESULT%
)
if ERRORLEVEL 0 (
    set _RESULT=0
    echo OK
)
popd
popd
exit _RESULT
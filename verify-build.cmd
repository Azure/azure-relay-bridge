pushd "%~dp0"
cd test/docker
msbuild /t:vstest
popd

pushd "%~dp0"
cd test/docker
msbuild /t:clean,build,vstest
popd

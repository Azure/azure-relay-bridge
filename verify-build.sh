#!/bin/bash

if [ ! -z $APPVEYOR_BUILD_VERSION ]; then _VersionProp="/p:VersionPrefix=$APPVEYOR_BUILD_VERSION"; fi

pushd test/mysql
dotnet msbuild /t:build /p:Configuration=Debug /p:TargetFramework=net6.0 $_BuildProp $_VersionProp $@
_RESULT=$?
if [ $_RESULT -ne 0 ]; then 
    popd
    exit $_RESULT
fi
dotnet test --verbosity=normal /p:Configuration=Debug /p:TargetFramework=net6.0 $_BuildProp $_VersionProp $@
_RESULT=$?
if [ $_RESULT -ne 0 ]; then 
    popd
    exit $_RESULT
fi
popd
pushd test/nginx
dotnet msbuild /t:clean,build /p:Configuration=Debug /p:TargetFramework=net6.0 $_BuildProp $_VersionProp $@
_RESULT=$?
if [ $_RESULT -ne 0 ]; then 
    popd
    exit $_RESULT
fi
dotnet test --verbosity=normal /p:Configuration=Debug /p:TargetFramework=net6.0 $_BuildProp $_VersionProp $@
_RESULT=$?
if [ $_RESULT -ne 0 ]; then
    popd
    exit $_RESULT
fi
popd
#!/bin/bash

if [ -z $AZBRIDGE_TEST_CXNSTRING ]; then 
  echo "Skipping integration tests due to missing AZBRIDGE_TEST_CXNSTRING secret"
  exit 0
fi

pushd test/mysql
dotnet msbuild /t:build /p:Configuration=Debug /p:TargetFramework=net8.0 $_BuildProp $_VersionProp $@
_RESULT=$?
if [ $_RESULT -ne 0 ]; then 
    popd
    exit $_RESULT
fi
dotnet test --verbosity=normal /p:Configuration=Debug /p:TargetFramework=net8.0 $_BuildProp $_VersionProp $@
_RESULT=$?
if [ $_RESULT -ne 0 ]; then 
    popd
    exit $_RESULT
fi
popd
pushd test/nginx
dotnet msbuild /t:clean,build /p:Configuration=Debug /p:TargetFramework=net8.0 $_BuildProp $_VersionProp $@
_RESULT=$?
if [ $_RESULT -ne 0 ]; then 
    popd
    exit $_RESULT
fi
dotnet test --verbosity=normal /p:Configuration=Debug /p:TargetFramework=net8.0 $_BuildProp $_VersionProp $@
_RESULT=$?
if [ $_RESULT -ne 0 ]; then
    popd
    exit $_RESULT
fi
popd
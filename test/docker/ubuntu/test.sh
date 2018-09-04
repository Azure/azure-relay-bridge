#!/bin/bash

if [ ! -z $1 ]; then BuildNumber=$1; fi
if [ ! -z $2 ]; then VersionPrefix=$2; fi
if [ ! -z $3 ]; then VersionSuffix=$3; fi
if [ ! -z $4 ]; then TargetFramework=$4; fi


if [ -z ${BuildNumber+x} ]; then BuildNumber='0000'; fi
if [ -z ${VersionSuffix+x} ]; then VersionSuffix='preview'; fi
if [ -z ${VersionPrefix+x} ]; then VersionPrefix='1.0.0'; fi
if [ -z ${TargetFramework+x} ]; then TargetFramework='netcoreapp2.1'; fi

pushd "${0%/*}" > /dev/null 

IMAGE_NAME=azbridge_ubuntu1604_test
source ../_scripts/imagetests.sh
popd
exit $_RESULT

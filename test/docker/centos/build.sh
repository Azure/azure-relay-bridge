#!/bin/bash

if [ ! -z $1 ]; then BuildNumber=$1; fi
if [ ! -z $2 ]; then VersionSuffix=$2; fi
if [ ! -z $3 ]; then VersionPrefix=$3; fi
if [ ! -z $4 ]; then TargetFramework=$4; fi

if [ -z ${BuildNumber+x} ]; then BuildNumber='0000'; fi
if [ -z ${VersionSuffix+x} ]; then VersionSuffix='preview'; fi
if [ -z ${VersionPrefix+x} ]; then VersionPrefix='1.0.0'; fi
if [ -z ${TargetFramework+x} ]; then TargetFramework='netcoreapp2.1'; fi

pushd "${0%/*}" > /dev/null 
if [ ! -d "tmp" ]; then mkdir tmp; fi
RpmFile=azbridge.$VersionPrefix-$VersionSuffix-$BuildNumber.centos-x64.rpm

cp ../../../artifacts/build/$TargetFramework/$RpmFile tmp/ > /dev/null
docker build -f Dockerfile . --tag azbridge_centos_test --build-arg rpm_package=$RpmFile
rm -rf tmp
popd
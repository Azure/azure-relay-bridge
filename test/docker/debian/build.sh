#!/bin/bash

if [ -z ${BuildNumber+x} ]; then BuildNumber='0000'; fi
if [ -z ${VersionSuffix+x} ]; then VersionSuffix='preview'; fi
if [ -z ${VersionPrefix+x} ]; then VersionPrefix='1.0.0'; fi
if [ -z ${TargetFramework+x} ]; then TargetFramework='2.1'; fi

pushd "${0%/*}" > /dev/null 
if [ ! -d "tmp" ]; then mkdir tmp; fi
DebFile=azbridge.$VersionPrefix-$VersionSuffix-$BuildNumber.debian.8-x64.deb

cp ../../../artifacts/build/$TargetFramework/$DebFile tmp/ > /dev/null
docker build -f Dockerfile . --tag azbridge_debian8_test --build-arg deb_package=$DebFile
rm -rf tmp
popd
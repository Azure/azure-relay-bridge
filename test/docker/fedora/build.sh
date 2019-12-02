#!/bin/bash

if [ ! -z $1 ]; then VersionPrefix=$1; fi
if [ ! -z $2 ]; then VersionSuffix=$2; fi
if [ ! -z $3 ]; then TargetFramework=$3; fi

if [ -z ${VersionSuffix+x} ]; then VersionSuffix='preview'; fi
if [ -z ${VersionPrefix+x} ]; then VersionPrefix='1.0.0'; fi
if [ -z ${TargetFramework+x} ]; then TargetFramework='netcoreapp3.0'; fi

pushd "${0%/*}" > /dev/null 
if [ ! -d "tmp" ]; then mkdir tmp; fi
RpmFile=azbridge.$VersionPrefix-$VersionSuffix.fedora-x64.rpm

cp ../../../artifacts/build/$TargetFramework/$RpmFile tmp/ > /dev/null
docker build -f Dockerfile . --tag azbridge_fedora_test --build-arg rpm_package=$RpmFile
rm -rf tmp
popd
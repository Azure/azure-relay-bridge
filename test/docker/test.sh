#!/bin/bash

if [ ! -z $1 ]; then Operation=$1; fi
if [ ! -z $2 ]; then ImageName=$2; fi
if [ ! -z $3 ]; then ImageSuffix=$3; fi
if [ ! -z $4 ]; then VersionPrefix=$4; fi
if [ ! -z $5 ]; then VersionSuffix=$5; fi
if [ ! -z $6 ]; then TargetFramework=$6; fi

if [ -z ${Operation+x} ]; then Operation='build'; fi
if [ -z ${ImageName+x} ]; then ImageName='debian.8-x64'; fi
if [ -z ${ImageSuffix+x} ]; then VersionSuffix='deb'; fi
if [ -z ${VersionSuffix+x} ]; then VersionSuffix='preview'; fi
if [ -z ${VersionPrefix+x} ]; then VersionPrefix='1.0.0'; fi
if [ -z ${TargetFramework+x} ]; then TargetFramework='netcoreapp3.0'; fi

if [ "${Operation}" == "build" ]; then
    pushd "${0%/*}" > /dev/null 
    if [ ! -d "tmp" ]; then mkdir tmp; fi
    PackageName=azbridge.$VersionPrefix-$VersionSuffix.$ImageName.$ImageSuffix

    cp ../../artifacts/build/$TargetFramework/$PackageName tmp/ > /dev/null
    docker build -f $ImageName.dockerfile . --tag azbridge-test-$ImageName --build-arg package_name=$PackageName
    rm -rf tmp
    popd
else 
    if [ "${Operation}" == "test" ]; then
        pushd "${0%/*}" > /dev/null 
        IMAGE_NAME=azbridge-test-$ImageName
        _CXNSTRING=$AZBRIDGE_TEST_CXNSTRING
        if [ -z $_CXNSTRING ]; then 
            echo AZBRIDGE_TEST_CXNSTRING environment variable must be set to valid relay connection string
            exit 
        fi

        _MOUNTPATH=$(pwd)
        _TESTNAME=test_nc_ping_pong
        source runtest.sh 
        #if [ $_RESULT -ne 0 ]; then exit $_RESULT; fi

        _TESTNAME=test_nc_config_ping_pong
        source runtest.sh
        popd
        #exit $_RESULT
        exit
    else 
        if [ "${Operation}" == "clean" ]; then
            if [ ! -z `docker images "azbridge-test-$ImageName" -q` ]; then
                docker rmi -f azbridge_test_$ImageName:latest
            fi
        else
            echo "Unknown command"
        fi
    fi
fi 
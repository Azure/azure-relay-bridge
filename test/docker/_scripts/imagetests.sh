#!/bin/bash
_CXNSTRING=$AZBRIDGE_TEST_CXNSTRING
if [ -z $_CXNSTRING ] && [ -n ${@+x} ]; then
   _CXNSTRING="$@"
fi

if [ -z $_CXNSTRING ]; then 
    echo AZBRIDGE_TEST_CXNSTRING environment variable must be set to valid relay connection string
    exit 
fi

_IMAGE_ID=`docker images $IMAGE_NAME -q`
if [ -z $_IMAGE_ID ]; then 
    source build.sh
fi

_MOUNTPATH=$(dirname $(pwd))
_TESTNAME=test_nc_ping_pong
source ../_scripts/runtest.sh
if [ $_RESULT -ne 0 ]; then exit $_RESULT; fi

_TESTNAME=test_nc_config_ping_pong
source ../_scripts/runtest.sh
exit $_RESULT

#!/bin/bash
pushd "${0%/*}" > /dev/null 

IMAGE_NAME=azbridge_ubuntu1604_test
source ../_scripts/imagetests.sh
popd
exit $_RESULT

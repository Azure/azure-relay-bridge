#!/bin/bash

pushd "${0%/*}" > /dev/null 
cd test/docker
dotnet test
popd

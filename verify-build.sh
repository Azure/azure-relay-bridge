#!/bin/bash

pushd "${0%/*}"
echo verifying build starting in $(pwd)
cd test/docker
dotnet clean
dotnet build
dotnet test
popd

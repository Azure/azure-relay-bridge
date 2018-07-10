#!/bin/bash

cd test/docker
dotnet clean
dotnet build
dotnet test

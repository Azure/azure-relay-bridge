#!/bin/bash

set -euo pipefail

dotnet restore
dotnet test $@
dotnet msbuild $@
#!/bin/bash

set -euo pipefail

dotnet restore
dotnet msbuild $@

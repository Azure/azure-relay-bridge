#!/bin/bash

set -euo pipefail

dotnet restore
dotnet test $@
dotnet build -c Release $@
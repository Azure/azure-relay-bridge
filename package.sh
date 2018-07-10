#!/bin/bash

dotnet msbuild /t:clean,restore,package /p:Configuration=Release $@

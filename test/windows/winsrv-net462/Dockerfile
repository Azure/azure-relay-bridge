FROM microsoft/dotnet-framework:4.6.2 AS build
ARG msi_package
COPY .\tmp\$msi_package c:\tmp
RUN msiexec.exe /i  c:\tmp\$msi_package /qn

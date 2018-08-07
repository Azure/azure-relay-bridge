# escape=`

FROM microsoft/windowsservercore:ltsc2016 AS build
ARG msi_package
ADD ${msi_package} .
RUN msiexec.exe /qn /i %msi_package%

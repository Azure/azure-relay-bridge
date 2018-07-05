FROM microsoft/dotnet:2.1-sdk-alpine AS build
WORKDIR /install
COPY azbridge.1.0.0-preview-0000.debian-x64.deb .
RUN apt update && apt install -y ./azbridge.1.0.0-preview-0000.debian-x64.deb

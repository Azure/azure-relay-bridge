FROM mcr.microsoft.com/dotnet/sdk:8.0 as publish

# ENV http_proxy=http://proxy.corporation.example:8080
# ENV https_proxy=http://proxy.corporation.example:8080
COPY . /azure-relay-bridge/
WORKDIR /azure-relay-bridge/src/azbridge
RUN dotnet publish azbridge.csproj -c Release -f net8.0 -p:SelfContained=false -r linux-x64  -p:PublishTrimmed=false -o /app

FROM mcr.microsoft.com/dotnet/runtime:8.0
ARG REVISION=0.9.0
ARG VERSION=0.9
LABEL org.opencontainers.image.documentation="https://github.com/Azure/azure-relay-bridge/blob/master/README.md"
LABEL org.opencontainers.image.source="https://github.com/Azure/azure-relay-bridge"
LABEL org.opencontainers.image.url="https://github.com/Azure/azure-relay-bridge"
LABEL org.opencontainers.image.authors="askservicebus@microsoft.com"
LABEL org.opencontainers.image.title="Microsoft Azure Relay Bridge"
LABEL org.opencontainers.image.description="CLI tool to create TCP, UDP, Sockets, and HTTP tunnels via proxies and firewalls using the Azure Relay service."
LABEL org.opencontainers.image.base.name="mcr.microsoft.com/dotnet/runtime:8.0"
LABEL org.opencontainers.image.revision=${REVISION}
LABEL org.opencontainers.image.revision=${VERSION}
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT [ "/app/azbridge" ]

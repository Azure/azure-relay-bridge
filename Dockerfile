FROM mcr.microsoft.com/dotnet/sdk:6.0 as publish

COPY . /azure-relay-bridge/
WORKDIR /azure-relay-bridge/src/azbridge
RUN dotnet publish azbridge.csproj -c Release -f net6.0 -p:SelfContained=false -r ubuntu-x64  -p:PublishTrimmed=false -o /app

FROM mcr.microsoft.com/dotnet/runtime:6.0
RUN apt-get update
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT [ "/app/azbridge" ]

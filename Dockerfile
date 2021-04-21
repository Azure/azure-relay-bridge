FROM mcr.microsoft.com/dotnet/core/sdk:5.0 as publish

COPY . /azure-relay-bridge/
WORKDIR /azure-relay-bridge/src/azbridge
RUN dotnet publish azbridge.csproj -c Release -f netcoreapp5.0 -p:SelfContained=false -r ubuntu-x64  -p:PublishTrimmed=false -o /app

FROM mcr.microsoft.com/dotnet/core/runtime:5.0
RUN apt-get update
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT [ "/app/azbridge" ]

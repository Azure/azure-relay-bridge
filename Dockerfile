FROM mcr.microsoft.com/dotnet/core/sdk:2.1 as build

WORKDIR /azure-relay-bridge/src

COPY src/azbridge/azbridge.csproj /azure-relay-bridge/src/azbridge/
COPY src/Microsoft.Azure.Relay.Bridge/Microsoft.Azure.Relay.Bridge.csproj /azure-relay-bridge/src/Microsoft.Azure.Relay.Bridge/

RUN dotnet restore /azure-relay-bridge/src/azbridge/azbridge.csproj

COPY . /azure-relay-bridge/

WORKDIR /azure-relay-bridge/src/azbridge
RUN dotnet build azbridge.csproj


FROM build AS publish
WORKDIR /azure-relay-bridge/src/azbridge
RUN dotnet publish azbridge.csproj -c Release -f netcoreapp2.1 -o /app


FROM mcr.microsoft.com/dotnet/core/runtime:2.1
WORKDIR /app
COPY --from=publish /app .

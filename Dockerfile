FROM mcr.microsoft.com/dotnet/core/sdk:3.0 as build

WORKDIR /azure-relay-bridge/src

COPY src/azbridge/azbridge.csproj /azure-relay-bridge/src/azbridge/
COPY src/Microsoft.Azure.Relay.Bridge/Microsoft.Azure.Relay.Bridge.csproj /azure-relay-bridge/src/Microsoft.Azure.Relay.Bridge/

RUN dotnet restore /azure-relay-bridge/src/azbridge/azbridge.csproj

COPY . /azure-relay-bridge/

WORKDIR /azure-relay-bridge/src/azbridge
RUN dotnet build azbridge.csproj


FROM build AS publish
WORKDIR /azure-relay-bridge/src/azbridge
RUN dotnet publish azbridge.csproj -c Release -f netcoreapp3.0 -o /app


FROM mcr.microsoft.com/dotnet/core/runtime:3.0
WORKDIR /app
COPY --from=publish /app .

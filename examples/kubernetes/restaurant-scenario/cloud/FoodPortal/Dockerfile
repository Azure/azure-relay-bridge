FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
COPY . /restaurant/
WORKDIR /restaurant

RUN curl -sL https://deb.nodesource.com/setup_12.x |bash -
RUN apt-get install -y nodejs
RUN npm install 

RUN dotnet build FoodPortal.csproj

FROM build AS publish
WORKDIR /restaurant
RUN dotnet publish FoodPortal.csproj -c Release -f net6.0 -o /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0
RUN apt-get update
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT [ "/app/FoodPortal" ]
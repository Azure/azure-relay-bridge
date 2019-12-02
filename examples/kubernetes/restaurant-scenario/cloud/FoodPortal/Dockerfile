FROM mcr.microsoft.com/dotnet/core/sdk:3.0 as build
COPY . /restaurant/
WORKDIR /restaurant

RUN curl -sL https://deb.nodesource.com/setup_12.x |bash -
RUN apt-get install -y nodejs
RUN npm install 

RUN dotnet build FoodPortal.csproj

FROM build AS publish
WORKDIR /restaurant
RUN dotnet publish FoodPortal.csproj -c Release -f netcoreapp3.0 -o /app

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0
RUN apt-get update
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT [ "/app/FoodPortal" ]
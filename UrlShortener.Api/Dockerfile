FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore UrlShortener.Api/UrlShortener.Api.csproj
RUN dotnet publish UrlShortener.Api/UrlShortener.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN apt-get update && apt-get install -y libkrb5-3

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]
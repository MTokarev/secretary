# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# Build backend.
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Secretary.csproj", "./"]
RUN dotnet restore "./Secretary.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Secretary.csproj" -c Release -o /app/build

# Configuring addresses, please specify FQDN for your site.
# Do not forget to put '/' at the end.
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS pwsh
SHELL ["pwsh", "-Command"]
WORKDIR /src
COPY ["./client/src/assets/config/config.prod.json", "./appsettings.json", "dockerbuild", "./"]
RUN ./Set-Config.ps1 -siteFqdn "https://get-secret.com/" -configFilePath "config.prod.json"

# Build frontend.
FROM node:16 AS node
WORKDIR /src
COPY ["client", "./"]
RUN npm install -g @angular/cli; npm install; ng build

# Publish backend.
FROM build AS publish
RUN dotnet publish "Secretary.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Copy Backend and FrontEnd.
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=node /src/wwwroot ./wwwroot
COPY --from=pwsh /src/config.prod.json ./wwwroot/assets/config/
COPY --from=pwsh /src/appsettings.json .
EXPOSE 8080 
ENV ASPNETCORE_URLS "http://*:8080"
ENTRYPOINT ["dotnet", "Secretary.dll"]

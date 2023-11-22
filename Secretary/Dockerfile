# OS tag is required here where your targed runtime is x64, but the docker build runs on Apple silicone
FROM mcr.microsoft.com/dotnet/aspnet:8.0.0-bookworm-slim-amd64 AS base
WORKDIR /app

# Build backend 
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Secretary.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build "Secretary.csproj" -c Release -o /app/build -r linux-x64

# Build frontend
FROM node:20 AS node
WORKDIR /src
COPY ["client", "./"]
RUN npm install -g @angular/cli; npm install; ng build

# Publish backend
FROM build AS publish
RUN dotnet publish "Secretary.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Copy Backend and FrontEnd
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=node /src/wwwroot ./wwwroot
EXPOSE 8080
ENV ASPNETCORE_URLS "http://*:8080"
ENTRYPOINT ["dotnet", "Secretary.dll"]

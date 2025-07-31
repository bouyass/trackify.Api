# Étape 1 : Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copier le projet et restaurer les dépendances
COPY *.csproj .
RUN dotnet restore

# Copier le reste des fichiers et publier
COPY . .
RUN dotnet publish -c Release -o /app

# Étape 2 : Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Exposer le port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Trackify.Api.dll"]
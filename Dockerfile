FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj Backend/BankProductsRegistry.Api/
RUN dotnet restore Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj

COPY Backend/BankProductsRegistry.Api/ Backend/BankProductsRegistry.Api/
WORKDIR /src/Backend/BankProductsRegistry.Api
# Publicar y empaquetar el entrypoint dentro de /app/publish (la etapa final solo copia del build).
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false \
    && install -m 0755 docker-entrypoint.sh /app/publish/docker-entrypoint.sh

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["/app/docker-entrypoint.sh"]

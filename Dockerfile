FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj Backend/BankProductsRegistry.Api/
RUN dotnet restore Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj

COPY Backend/BankProductsRegistry.Api/ Backend/BankProductsRegistry.Api/
WORKDIR /src/Backend/BankProductsRegistry.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
COPY Backend/BankProductsRegistry.Api/docker-entrypoint.sh /app/docker-entrypoint.sh
RUN chmod +x /app/docker-entrypoint.sh

EXPOSE 8080

ENTRYPOINT ["/app/docker-entrypoint.sh"]

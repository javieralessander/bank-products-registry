FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY BankProductsRegistry.Frontend.csproj ./
RUN dotnet restore BankProductsRegistry.Frontend.csproj

COPY . ./
RUN dotnet publish BankProductsRegistry.Frontend.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

# Railway inyecta PORT; si no existe, usa 8080 por defecto.
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet BankProductsRegistry.Frontend.dll"]

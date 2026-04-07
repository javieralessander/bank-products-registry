FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY BankProductsRegistry.Frontend.csproj ./
RUN dotnet restore BankProductsRegistry.Frontend.csproj

COPY . ./
RUN dotnet publish BankProductsRegistry.Frontend.csproj -c Release -o /app/publish /p:UseAppHost=false \
    && install -m 0755 docker-entrypoint.sh /app/publish/docker-entrypoint.sh

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

EXPOSE 8080

ENTRYPOINT ["/app/docker-entrypoint.sh"]

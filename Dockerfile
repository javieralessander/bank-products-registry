FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj Backend/BankProductsRegistry.Api/
RUN dotnet restore Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj

COPY Backend/BankProductsRegistry.Api/ Backend/BankProductsRegistry.Api/
WORKDIR /src/Backend/BankProductsRegistry.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BankProductsRegistry.Api.dll"]

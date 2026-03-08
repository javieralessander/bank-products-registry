# Sistema de Registro de Productos Bancarios

Backend RESTful API desarrollado con .NET 9, Entity Framework Core y SQL Server.

El proyecto actual incluye solo el backend.

## Requisitos

- Git
- Docker Desktop
- .NET SDK 9 o superior
- Terminal o PowerShell

## Despues de clonar el repositorio

### 1. Entrar al proyecto

```bash
cd bank-products-registry
```

### 2. Crear el contenedor de SQL Server

Ejecuta este comando solo la primera vez:

```bash
docker run -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=Unapec@2026" \
  -p 1433:1433 \
  --name bank-products-sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Nota:
- Si usas Mac con chip Apple Silicon y sale un warning de plataforma `amd64`, es normal.

### 3. Si el contenedor ya existe, solo inícialo

Si ya lo habías creado antes, no vuelvas a usar `docker run`. Usa esto:

```bash
docker start bank-products-sqlserver
```

### 4. Verificar que la base de datos este encendida

```bash
docker ps
```

Debes ver un contenedor llamado `bank-products-sqlserver` en estado `Up`.

### 5. Restaurar dependencias del backend

```bash
dotnet restore Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

### 6. Ejecutar el backend con la conexion correcta

#### Opcion A: Mac o Linux

```bash
ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=BankProductsRegistryDb;User Id=sa;Password=Unapec@2026;Encrypt=False;TrustServerCertificate=True;" dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

#### Opcion B: Windows PowerShell

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=BankProductsRegistryDb;User Id=sa;Password=Unapec@2026;Encrypt=False;TrustServerCertificate=True;"
dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

#### Opcion C: Windows CMD

```cmd
set ConnectionStrings__DefaultConnection=Server=localhost,1433;Database=BankProductsRegistryDb;User Id=sa;Password=Unapec@2026;Encrypt=False;TrustServerCertificate=True;
dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

Cuando todo este bien, veras algo parecido a esto:

```text
Now listening on: http://localhost:5039
```

La base de datos se crea automaticamente al iniciar por primera vez.

### 7. Abrir Swagger

```text
http://localhost:5039/swagger
```

## Pruebas rapidas

Abre otra terminal y prueba:

```bash
curl http://localhost:5039/health
curl http://localhost:5039/api/clients
curl http://localhost:5039/api/employees
curl http://localhost:5039/api/financial-products
```

## Comandos utiles

### Detener el backend

En la terminal donde esta corriendo:

```bash
Ctrl + C
```

### Detener SQL Server

```bash
docker stop bank-products-sqlserver
```

### Volver a iniciar SQL Server

```bash
docker start bank-products-sqlserver
```

### Ver logs de SQL Server

```bash
docker logs bank-products-sqlserver
```

### Borrar el contenedor y crearlo de nuevo

```bash
docker rm -f bank-products-sqlserver
```

Luego vuelve a ejecutar el comando del paso 2.

## Estructura del backend

```text
Backend/
  BankProductsRegistry.Api/
    Controllers/
    Data/
    Dtos/
    Models/
    Services/
    Utilities/
```

## Endpoints principales

- `GET /api/clients`
- `POST /api/clients`
- `GET /api/employees`
- `POST /api/employees`
- `GET /api/financial-products`
- `POST /api/financial-products`
- `GET /api/account-products`
- `POST /api/account-products`
- `GET /api/transactions`
- `POST /api/transactions`
- `GET /api/reports/clients/{clientId}/portfolio`

## Resumen corto

Si quieres la version mas corta posible en Mac o Linux, despues de clonar ejecuta esto:

```bash
docker run -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=Unapec@2026" \
  -p 1433:1433 \
  --name bank-products-sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest

dotnet restore Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj

ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=BankProductsRegistryDb;User Id=sa;Password=Unapec@2026;Encrypt=False;TrustServerCertificate=True;" dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

Si usas Windows PowerShell, el ultimo paso cambia a este:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=BankProductsRegistryDb;User Id=sa;Password=Unapec@2026;Encrypt=False;TrustServerCertificate=True;"
dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

Si usas Windows CMD, el ultimo paso cambia a este:

```cmd
set ConnectionStrings__DefaultConnection=Server=localhost,1433;Database=BankProductsRegistryDb;User Id=sa;Password=Unapec@2026;Encrypt=False;TrustServerCertificate=True;
dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

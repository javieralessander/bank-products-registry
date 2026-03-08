# Sistema de Registro de Productos Bancarios

Proyecto academico dividido en dos partes:

- [Backend](#backend)
- [Frontend](#frontend)

Actualmente solo esta implementado el backend.

## Estructura del proyecto

```text
bank-products-registry/
  Backend/
    BankProductsRegistry.Api/
  Frontend/
  docker-compose.yml
```

## Backend

API RESTful desarrollada con .NET 9, Entity Framework Core y MySQL.

### Requisitos

- Git
- Docker Desktop
- .NET SDK 9 o superior
- Terminal o PowerShell

### Pasos despues de clonar el repositorio

#### 1. Entrar al proyecto

```bash
cd bank-products-registry
```

#### 2. Levantar MySQL con Docker

```bash
docker compose up -d mysql
```

Este comando crea e inicia un contenedor llamado `bank-products-mysql`.

Si vienes de una version anterior con PostgreSQL o con otra base local vieja, limpia primero los datos locales una sola vez:

```bash
docker compose down -v
docker compose up -d mysql
```

#### 3. Verificar que MySQL este encendido

```bash
docker ps
```

Debes ver `bank-products-mysql` en estado `Up`.

#### 4. Restaurar dependencias

```bash
dotnet restore Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

#### 5. Ejecutar el backend

##### Mac o Linux

```bash
ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=bank_products_registry_db;User=bank_user;Password=bank_password;SslMode=None;AllowPublicKeyRetrieval=True" dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

##### Windows PowerShell

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=bank_products_registry_db;User=bank_user;Password=bank_password;SslMode=None;AllowPublicKeyRetrieval=True"
dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

##### Windows CMD

```cmd
set ConnectionStrings__DefaultConnection=Server=localhost;Port=3306;Database=bank_products_registry_db;User=bank_user;Password=bank_password;SslMode=None;AllowPublicKeyRetrieval=True
dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

Si todo sale bien, veras algo parecido a esto:

```text
Now listening on: http://localhost:5039
```

La base de datos se crea automaticamente la primera vez.

#### 6. Abrir Swagger

```text
http://localhost:5039/swagger
```

### Pruebas rapidas

En otra terminal:

```bash
curl http://localhost:5039/health
curl http://localhost:5039/api/clients
curl http://localhost:5039/api/employees
curl http://localhost:5039/api/financial-products
```

### Comandos utiles

#### Detener el backend

```bash
Ctrl + C
```

#### Detener MySQL

```bash
docker compose stop mysql
```

#### Volver a iniciar MySQL

```bash
docker compose start mysql
```

#### Ver logs de MySQL

```bash
docker compose logs mysql
```

#### Borrar el contenedor y el volumen

```bash
docker compose down -v
```

### Variables de entorno compatibles

El backend acepta cualquiera de estas opciones:

#### Opcion 1: Connection string directa

```text
ConnectionStrings__DefaultConnection=Server=localhost;Port=3306;Database=bank_products_registry_db;User=bank_user;Password=bank_password;SslMode=None;AllowPublicKeyRetrieval=True
```

#### Opcion 2: MYSQL_URL

```text
MYSQL_URL=mysql://usuario:clave@host:3306/base_de_datos
```

#### Opcion 3: Variables MYSQL

```text
MYSQLHOST=localhost
MYSQLPORT=3306
MYSQLDATABASE=bank_products_registry_db
MYSQLUSER=bank_user
MYSQLPASSWORD=bank_password
```

### Railway

Para Railway, usa MySQL nativo.

1. Crea un servicio MySQL dentro del proyecto en Railway.
2. Copia las variables `MYSQLHOST`, `MYSQLPORT`, `MYSQLDATABASE`, `MYSQLUSER` y `MYSQLPASSWORD`, o usa `MYSQL_URL` si Railway te la muestra.
3. En el servicio del backend agrega esas variables.
4. Despliega el backend con el `Dockerfile` de la raiz del repositorio.

### Endpoints principales

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

### Resumen corto

#### Mac o Linux

```bash
docker compose up -d mysql
dotnet restore Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=bank_products_registry_db;User=bank_user;Password=bank_password;SslMode=None;AllowPublicKeyRetrieval=True" dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

#### Windows PowerShell

```powershell
docker compose up -d mysql
dotnet restore Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
$env:ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=bank_products_registry_db;User=bank_user;Password=bank_password;SslMode=None;AllowPublicKeyRetrieval=True"
dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

#### Windows CMD

```cmd
docker compose up -d mysql
dotnet restore Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
set ConnectionStrings__DefaultConnection=Server=localhost;Port=3306;Database=bank_products_registry_db;User=bank_user;Password=bank_password;SslMode=None;AllowPublicKeyRetrieval=True
dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

## Frontend

La carpeta `Frontend/` existe para la segunda parte del proyecto.

Estado actual:

- No esta implementado todavia.
- No tiene dependencias configuradas.
- No afecta la ejecucion del backend.

Cuando se desarrolle el frontend, debera consumir el backend en:

```text
http://localhost:5039
```

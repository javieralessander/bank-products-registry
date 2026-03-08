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
```

## Backend

API RESTful desarrollada con .NET 9, Entity Framework Core y MySQL.

### Requisitos

- Git
- .NET SDK 9 o superior
- MySQL disponible en tu maquina o en un servidor remoto
- Terminal o PowerShell

### Pasos despues de clonar el repositorio

#### 1. Entrar al proyecto

```bash
cd bank-products-registry
```

#### 2. Tener una base MySQL disponible

Puedes usar una instalacion local de MySQL o una base remota.

Debes tener una base vacia, por ejemplo:

```text
bank_products_registry_db
```

Si la base ya tiene tablas viejas de pruebas, borralas o usa una base nueva antes de correr el backend.

#### 3. Restaurar dependencias

```bash
dotnet restore Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

#### 4. Configurar la conexion

Puedes usar una sola de estas opciones.

##### Opcion 1: Variable de entorno temporal

Usa una cadena de conexion parecida a esta:

```text
Server=localhost;Port=3306;Database=bank_products_registry_db;User=bank_user;Password=bank_password;SslMode=None;AllowPublicKeyRetrieval=True
```

##### Opcion 2: appsettings.json o appsettings.Development.json

Edita la clave:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=bank_products_registry_db;User=bank_user;Password=bank_password;SslMode=None;AllowPublicKeyRetrieval=True"
}
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
Las migraciones se aplican automaticamente al arrancar el backend.

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

#### Opcion 4: Railway MySQL (desarrollo local contra la nube)

Para conectar desde tu maquina al MySQL de Railway:

```text
MYSQL_PUBLIC_URL=mysql://root:TU_PASSWORD@gondola.proxy.rlwy.net:19286/railway
MYSQL_SERVER_VERSION=9.4.0-mysql
```

Reemplaza `TU_PASSWORD` con la contraseña de Railway (Variables o Database > Credentials).

Para desplegar el Backend en Railway: agrega el servicio MySQL como Variable Reference a tu Backend. Railway inyectara `MYSQL_URL` automaticamente.

Con tu base actual en Railway usa tambien:

```text
MYSQL_SERVER_VERSION=9.4.0-mysql
```

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

### Conexion a Railway MySQL

**Desarrollo local conectando a Railway:**

```bash
dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj --launch-profile Railway
```

La conexion esta guardada en User Secrets (no se sube al repo). Para cambiarla:

```bash
dotnet user-secrets set "MYSQL_PUBLIC_URL" "mysql://root:TU_PASSWORD@gondola.proxy.rlwy.net:19286/railway" --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
dotnet user-secrets set "MYSQL_SERVER_VERSION" "9.4.0-mysql" --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

**Backend desplegado en Railway:** En el servicio del Backend, Variables > Add Variable Reference > selecciona el MySQL y vincula `MYSQL_URL`. Ademas agrega esta variable manual:

```text
MYSQL_SERVER_VERSION=9.4.0-mysql
```

Con la configuracion que mostraste, tu backend debe usar estas variables:

```text
MYSQL_URL=${{MySQL.MYSQL_URL}}
MYSQL_SERVER_VERSION=9.4.0-mysql
```

### Resumen corto

#### Mac o Linux

```bash
dotnet restore Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=bank_products_registry_db;User=bank_user;Password=bank_password;SslMode=None;AllowPublicKeyRetrieval=True" dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

#### Windows PowerShell

```powershell
dotnet restore Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
$env:ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=bank_products_registry_db;User=bank_user;Password=bank_password;SslMode=None;AllowPublicKeyRetrieval=True"
dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

#### Windows CMD

```cmd
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

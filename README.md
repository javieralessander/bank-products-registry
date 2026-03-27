# Sistema de Registro de Productos Bancarios

Proyecto academico dividido en dos partes:

- [Backend](#backend)
- [Frontend](#frontend)

El backend ya esta implementado. El frontend se encuentra en fase de definicion e implementacion inicial.

## Enlaces principales

- Repositorio GitHub: `https://github.com/javieralessander/bank-products-registry`
- URL de produccion en Railway: `https://bank-products-registry-production.up.railway.app`
- Swagger en produccion: `https://bank-products-registry-production.up.railway.app/swagger`
- Documento de requerimientos cumplidos: [Backend/Documento-Requerimientos-ejemplo.md](/Users/comunicaciones/Desktop/ALL/UNAPEC/bank-products-registry/Backend/Documento-Requerimientos-ejemplo.md)
- Documento de descripcion del proyecto: [Backend/Documento-Descripcion-Proyecto.md](/Users/comunicaciones/Desktop/ALL/UNAPEC/bank-products-registry/Backend/Documento-Descripcion-Proyecto.md)

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

Si usas Railway MySQL, agrega tambien:

```json
"MYSQL_SERVER_VERSION": "9.4.0-mysql"
```

#### 5. Ejecutar el backend

Si ya dejaste la conexion en `appsettings.Development.json`, usa:

```bash
dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

Si todo sale bien, veras algo parecido a esto:

```text
Now listening on: http://localhost:5039
```

La base de datos se crea automaticamente la primera vez.
Las migraciones se aplican automaticamente al arrancar el backend.
Si las tablas principales estan vacias, el sistema carga datos iniciales de prueba.

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

```bash
dotnet restore Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
dotnet run --project Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
```

## Frontend

El frontend corresponde a la segunda parte del proyecto y consumira la API REST desarrollada en el backend.

### Tecnologias definidas para el frontend

- HTML5 para la estructura base de las vistas.
- CSS3 para estilos complementarios.
- Bootstrap 5 como framework principal de interfaz.
- JavaScript para validaciones en cliente, interacciones y consumo de endpoints HTTP.
- DataTables como libreria de apoyo para tablas filtrables, busquedas y paginacion en el catalogo de productos.

### Enfoque de uso

- La interfaz sera responsive para escritorio, tablet y movil.
- Se utilizaran componentes de Bootstrap como formularios, tablas, modales, alerts y cards.
- El catalogo de productos podra consultarse y filtrarse desde la interfaz para reducir duplicidad de registros.
- El formulario de registro y edicion de productos aplicara validaciones en tiempo real antes de enviar la informacion al backend.
- El frontend se mantendra ligero y enfocado en la capa de presentacion, delegando la logica de negocio y la persistencia al API en .NET.

### Estado actual del frontend

- La carpeta `Frontend/` contiene por ahora una maqueta base en `Frontend/index.html`.
- Todavia no hay dependencias instaladas ni un flujo de build configurado.
- La seleccion tecnologica del frontend ya esta definida, pero su implementacion completa sigue pendiente.

Cuando se desarrolle el frontend, debera consumir el backend en:

```text
http://localhost:5039
```

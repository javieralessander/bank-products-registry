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

Para ejecutar el backend localmente contra Railway, puedes usar una conexion publica como esta:

```text
Server=centerbeam.proxy.rlwy.net;Port=52379;Database=railway;User=root;Password=TU_PASSWORD;SslMode=Required
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

Si estas en Railway y el backend corre dentro del mismo proyecto, normalmente conviene usar la URL interna:

```text
MYSQL_URL=mysql://root:TU_PASSWORD@mysql.railway.internal:3306/railway
```

Si estas corriendo el backend desde tu maquina local, usa la URL publica:

```text
MYSQL_PUBLIC_URL=mysql://root:TU_PASSWORD@centerbeam.proxy.rlwy.net:52379/railway
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

- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/revoke`
- `GET /api/auth/me`
- `GET /api/users`
- `POST /api/users`
- `PATCH /api/users/{id}/status`
- `PATCH /api/users/{id}/role`
- `POST /api/users/{id}/reset-password`
- `GET /api/clients`
- `POST /api/clients`
- `GET /api/employees`
- `POST /api/employees`
- `GET /api/financial-products`
- `POST /api/financial-products`
- `GET /api/account-products`
- `POST /api/account-products`
- `GET /api/account-products/{id}`
- `GET /api/account-products/{accountProductId}/blocks`
- `POST /api/account-products/{accountProductId}/blocks`
- `POST /api/account-products/{accountProductId}/blocks/{blockId}/release`
- `GET /api/account-products/{accountProductId}/audits`
- `GET /api/account-products/{accountProductId}/limits`
- `PUT /api/account-products/{accountProductId}/limits`
- `POST /api/account-products/{accountProductId}/limits/temporary-adjustments`
- `GET /api/account-products/{accountProductId}/limits/history`
- `GET /api/account-products/{accountProductId}/travel-notices`
- `POST /api/account-products/{accountProductId}/travel-notices`
- `POST /api/account-products/{accountProductId}/travel-notices/{noticeId}/cancel`
- `GET /api/transactions`
- `POST /api/transactions`
- `GET /api/reports/clients/{clientId}/portfolio`
- `GET /api/reports/clients/{clientId}/credit-history`
- `GET /api/reports/clients/{clientId}/credit-score`

### Seguridad del API

- La API usa autenticacion con usuario y contrasena.
- El login devuelve un `access token` JWT y un `refresh token`.
- Todos los endpoints quedan protegidos por defecto.
- `POST /api/auth/login`, `POST /api/auth/refresh` y `GET /health` permiten acceso anonimo.
- Los roles disponibles son `Admin`, `Operador` y `Consulta`.
- Los endpoints de escritura requieren `Admin` o `Operador`.
- Los endpoints de eliminacion y la gestion de empleados requieren `Admin`.
- La gestion de usuarios del sistema se realiza solo desde endpoints administrativos protegidos para `Admin`.
- Un producto contratado con bloqueo activo no acepta nuevas transacciones, actualizaciones de transacciones ni reversos.
- Las transacciones ahora consideran `transactionChannel` y `countryCode` para validar limites por transaccion, diarios, ATM e internacionales.
- Las transacciones internacionales de consumo requieren un aviso de viaje vigente para el pais y la fecha de la operacion.
- El sistema expone un historial crediticio interno y un score interno calculado desde productos, pagos, mora, bloqueos y uso de limites.

#### Resumen de proteccion de endpoints

- Publicos: `POST /api/auth/login`, `POST /api/auth/refresh`, `GET /health`
- Requieren cualquier usuario autenticado: todos los `GET` de clientes, empleados, productos, cuentas, transacciones, reportes y `GET /api/auth/me`
- Requieren cualquier usuario autenticado: `GET /api/account-products/{accountProductId}/blocks`, `GET /api/account-products/{accountProductId}/audits`, `GET /api/account-products/{accountProductId}/limits`, `GET /api/account-products/{accountProductId}/limits/history` y `GET /api/account-products/{accountProductId}/travel-notices`
- Requieren cualquier usuario autenticado: `GET /api/reports/clients/{clientId}/credit-history` y `GET /api/reports/clients/{clientId}/credit-score`
- Requieren `Admin` u `Operador`: todos los `POST`, `PUT` y `PATCH` de clientes, productos financieros, productos contratados y transacciones, incluyendo `POST /api/account-products/{accountProductId}/blocks`, `POST /api/account-products/{accountProductId}/travel-notices` y `POST /api/account-products/{accountProductId}/travel-notices/{noticeId}/cancel`
- Requieren solo `Admin`: todos los `POST`, `PUT`, `PATCH` y `DELETE` de empleados, todos los `DELETE` del resto de entidades, toda la administracion de usuarios en `/api/users`, `POST /api/account-products/{accountProductId}/blocks/{blockId}/release`, `PUT /api/account-products/{accountProductId}/limits` y `POST /api/account-products/{accountProductId}/limits/temporary-adjustments`

#### Limites de consumo

- Los limites se configuran por producto contratado.
- El sistema soporta limite total, limite diario, limite por transaccion, limite ATM y limite internacional.
- Se pueden programar ajustes temporales con fecha de inicio y fin.
- Cada cambio queda registrado con usuario, motivo, vigencia y valores anteriores/nuevos.

#### Avisos de viaje

- Cada aviso de viaje se registra por producto contratado, rango de fechas y uno o mas paises en formato ISO.
- Los avisos pueden cancelarse y conservan su historial.
- Si una transaccion internacional de consumo no tiene un aviso vigente para ese pais, el API responde `409 Conflict`.

#### Historial y score interno

- `GET /api/reports/clients/{clientId}/credit-history` devuelve resumen crediticio, productos, eventos recientes y score interno.
- `GET /api/reports/clients/{clientId}/credit-score` devuelve el score, banda de riesgo y los factores que impactaron el calculo.
- La metodologia actual es `interna_v1`.
- El score es interno y heuristico; no reemplaza un score de buró externo como TransUnion.

### Usuarios semilla en desarrollo

Si ejecutas el proyecto con `ASPNETCORE_ENVIRONMENT=Development`, el sistema crea usuarios base para pruebas:

- `admin` / `Admin2026`
- `operador` / `Operador2026`
- `consulta` / `Consulta2026`

Estas credenciales son solo para desarrollo y deben reemplazarse en ambientes reales.

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

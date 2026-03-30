# Documento de Requerimientos Cumplidos

## Backend

### Proyecto

- **Nombre:** Sistema de Registro de Productos Bancarios
- **Descripcion:** Sistema digital para la administracion y registro centralizado de productos financieros como tarjetas de credito, prestamos, inversiones y certificados.
- **Alcance de este documento:** Backend

### Tecnologias utilizadas

- .NET 9 Web API
- Entity Framework Core
- MySQL
- Swagger / OpenAPI
- GitHub

### Requerimientos cumplidos

| Requerimiento | Estado | Evidencia |
|---|---|---|
| Modelado de datos del sistema | Cumplido | Se definieron las entidades `Clients`, `Employees`, `FinancialProducts`, `AccountProducts` y `Transactions`. |
| Implementacion del backend con .NET | Cumplido | API desarrollada en `Backend/BankProductsRegistry.Api`. |
| Endpoints RESTful por entidad | Cumplido | Se implementaron endpoints `GET`, `POST`, `PUT`, `PATCH` y `DELETE` por entidad. |
| Conexion del backend con base de datos MySQL | Cumplido | El proyecto soporta MySQL local y MySQL en Railway. |
| Migraciones de base de datos | Cumplido | Existe la migracion inicial en `Backend/BankProductsRegistry.Api/Data/Migrations`. |
| Carga inicial de datos de prueba | Cumplido | El sistema inserta datos base si las tablas principales estan vacias. |
| Prueba de ejecucion local del backend | Cumplido | El API fue ejecutado y probado mediante Swagger y consultas HTTP. |
| Configuracion del proyecto en GitHub dentro de `/Backend` | Cumplido | El backend esta organizado dentro de la carpeta `Backend/`. |
| Documentacion basica del backend | Cumplido | El proyecto incluye `README.md` y este documento de requerimientos. |
| Despliegue del API con URL publica final | Pendiente | La base de datos ya fue probada en Railway; el despliegue final del API depende del proveedor elegido (Railway o Azure). |

### Entidades implementadas

- Clientes
- Empleados
- Productos financieros
- Productos contratados / cuentas
- Transacciones

### Tabla de endpoints implementados

#### Clientes

- `GET /api/clients`
- `GET /api/clients/{id}`
- `POST /api/clients`
- `PUT /api/clients/{id}`
- `PATCH /api/clients/{id}`
- `DELETE /api/clients/{id}`

#### Empleados

- `GET /api/employees`
- `GET /api/employees/{id}`
- `POST /api/employees`
- `PUT /api/employees/{id}`
- `PATCH /api/employees/{id}`
- `DELETE /api/employees/{id}`

#### Productos financieros

- `GET /api/financial-products`
- `GET /api/financial-products/{id}`
- `POST /api/financial-products`
- `PUT /api/financial-products/{id}`
- `PATCH /api/financial-products/{id}`
- `DELETE /api/financial-products/{id}`

#### Productos contratados

- `GET /api/account-products`
- `GET /api/account-products/{id}`
- `POST /api/account-products`
- `PUT /api/account-products/{id}`
- `PATCH /api/account-products/{id}`
- `DELETE /api/account-products/{id}`

#### Transacciones

- `GET /api/transactions`
- `GET /api/transactions/{id}`
- `POST /api/transactions`
- `PUT /api/transactions/{id}`
- `PATCH /api/transactions/{id}`
- `DELETE /api/transactions/{id}`

#### Reporte adicional

- `GET /api/reports/clients/{clientId}/portfolio`

### Base de datos

- Motor utilizado: MySQL
- Conexion soportada: local y Railway
- Migracion inicial: creada y funcional
- Carga inicial de datos: habilitada para productos, clientes, empleados, cuentas y transacciones

### Estado actual del backend

- El backend compila correctamente.
- La base de datos se crea o actualiza automaticamente al arrancar.
- Si las tablas principales estan vacias, el sistema carga datos iniciales de prueba.
- El proyecto ya esta listo para continuar con la logica de negocio y las pruebas de integracion.

### Requerimientos funcionales propuestos para la siguiente iteracion

Estas funcionalidades no forman parte del alcance ya implementado, pero representan la evolucion natural del sistema hacia un escenario bancario mas realista.

| Requerimiento | Estado | Descripcion |
|---|---|---|
| RF-06 Bloqueo temporal de productos | Propuesto | Permitir bloquear temporalmente una tarjeta o producto contratado por un periodo definido, con motivo, fecha de inicio y fecha de fin. |
| RF-07 Bloqueo permanente de productos | Propuesto | Permitir cancelar definitivamente una tarjeta o producto para impedir nuevas operaciones. |
| RF-08 Bloqueo por fraude | Propuesto | Activar bloqueos preventivos por patrones sospechosos o alertas de fraude. |
| RF-09 Gestion de limites de credito y consumo | Propuesto | Configurar limite de credito total, limite diario, limite por transaccion, limite de retiros ATM y limite internacional. |
| RF-10 Ajuste temporal de limites | Propuesto | Permitir aumentar o reducir limites de uso por una vigencia determinada y con autorizacion administrativa. |
| RF-11 Aviso de viaje | Propuesto | Registrar viajes del cliente con fechas y paises autorizados para evitar rechazos por uso internacional legitimo. |
| RF-12 Validacion internacional | Propuesto | Evaluar operaciones internacionales tomando en cuenta aviso de viaje, pais de uso, limites y reglas de riesgo. |
| RF-13 Historial de bloqueos y auditoria | Propuesto | Registrar quien bloqueo, desbloqueo o modifico el estado de un producto, cuando lo hizo y por cual motivo. |
| RF-14 Historial de cambios de limites | Propuesto | Registrar el valor anterior, nuevo valor, motivo, vigencia y usuario aprobador de cada cambio de limite. |
| RF-15 Historial crediticio interno | Propuesto | Consolidar el comportamiento financiero del cliente con sus productos activos e historicos, pagos, mora y uso del credito. |
| RF-16 Score crediticio interno | Propuesto | Calcular un puntaje interno basado en endeudamiento, cumplimiento de pago, antiguedad y uso del credito. |
| RF-17 Reportes bancarios avanzados | Propuesto | Generar reportes de mora, concentracion, nuevos contratos, bloqueos y exportarlos a PDF, Excel o CSV. |

### Requerimientos no funcionales asociados a la siguiente iteracion

| Requerimiento | Estado | Descripcion |
|---|---|---|
| RNF-06 Trazabilidad operativa | Propuesto | Todo bloqueo, ajuste de limite, aprobacion y desbloqueo debe quedar auditado con usuario, fecha, motivo y canal. |
| RNF-07 Disponibilidad de consultas criticas | Propuesto | La consulta de historial crediticio, limites y estados de bloqueo debe responder en tiempos adecuados para operacion bancaria. |
| RNF-08 Seguridad reforzada | Propuesto | Los cambios sensibles deben requerir autorizacion por roles y, en escenarios criticos, aprobacion adicional de supervisor. |
| RNF-09 Exportacion confiable de reportes | Propuesto | Las exportaciones a PDF, Excel y CSV deben mantener integridad, formato consistente y trazabilidad de quien genero el reporte. |

### Entidades sugeridas para soportar la siguiente iteracion

- `ProductBlocks`
- `BlockHistory`
- `TravelNotices`
- `ProductLimits`
- `LimitChangeHistory`
- `FraudAlerts`
- `CreditHistorySnapshots`
- `CreditScores`
- `GeneratedReports`

### Paso a paso para replicar la logica de negocio en Swagger

Esta secuencia sirve para presentar el backend al profesor desde Swagger y demostrar el flujo principal del sistema.

#### 1. Levantar el backend

- Ejecutar el proyecto y abrir Swagger en `http://localhost:5039/swagger`.
- Verificar primero `GET /api/clients`, `GET /api/employees` y `GET /api/financial-products`.
- Si la base estaba vacia, estos endpoints deben mostrar datos de prueba cargados automaticamente.

#### 2. Crear un cliente nuevo

Usar `POST /api/clients` con un cuerpo como este:

```json
{
  "firstName": "Pedro",
  "lastName": "Sanchez",
  "nationalId": "40200019999",
  "email": "pedro.sanchez@demo.local",
  "phone": "8095550199",
  "isActive": true
}
```

- Guardar el `id` retornado del cliente.
- Si se quiere validar el registro, ejecutar `GET /api/clients/{id}`.

#### 3. Consultar los catalogos base

- Ejecutar `GET /api/financial-products` y elegir un producto activo.
- Ejecutar `GET /api/employees` y elegir un empleado activo.
- Guardar el `id` del producto financiero y el `id` del empleado.

Para una demo simple se recomienda usar:

- un producto tipo `cuenta_ahorro` o `prestamo`
- un empleado existente

#### 4. Registrar un producto contratado para el cliente

Usar `POST /api/account-products` con un cuerpo como este:

```json
{
  "clientId": 0,
  "financialProductId": 0,
  "employeeId": 0,
  "accountNumber": "0001002099",
  "amount": 25000,
  "openDate": "2026-03-09T00:00:00Z",
  "maturityDate": null,
  "status": "activo"
}
```

- Reemplazar `clientId`, `financialProductId` y `employeeId` con los ids reales obtenidos en los pasos anteriores.
- Guardar el `id` del producto contratado.
- Confirmar el registro con `GET /api/account-products/{id}`.

#### 5. Registrar una transaccion valida

Usar `POST /api/transactions` con un cuerpo como este:

```json
{
  "accountProductId": 0,
  "transactionType": "deposito",
  "amount": 5000,
  "transactionDate": "2026-03-09T00:00:00Z",
  "description": "Deposito inicial de presentacion",
  "referenceNumber": "DEMO-0001"
}
```

- Reemplazar `accountProductId` con el id del producto contratado creado.
- Luego ejecutar `GET /api/account-products/{id}` para comprobar que el monto fue actualizado.
- Tambien se puede revisar `GET /api/transactions` para ver la transaccion registrada.

#### 6. Demostrar una regla de negocio

Para mostrar validaciones del sistema, usar `POST /api/transactions` con un retiro o pago mayor al balance actual:

```json
{
  "accountProductId": 0,
  "transactionType": "retiro",
  "amount": 999999,
  "transactionDate": "2026-03-09T00:00:00Z",
  "description": "Prueba de validacion de balance",
  "referenceNumber": "DEMO-ERROR-01"
}
```

Resultado esperado:

- el sistema responde con error `400`
- el mensaje indica que la operacion deja el balance en negativo

#### 7. Consultar el reporte consolidado del cliente

Usar `GET /api/reports/clients/{clientId}/portfolio`.

- Reemplazar `clientId` por el id del cliente creado.
- Este endpoint permite demostrar la consolidacion de productos y montos del cliente en un solo reporte.

#### 8. Cierre de la demostracion

Para cerrar la presentacion se recomienda mostrar:

- `GET /api/clients/{id}` para confirmar los datos del cliente
- `GET /api/account-products/{id}` para confirmar el producto contratado
- `GET /api/reports/clients/{clientId}/portfolio` para mostrar la informacion consolidada

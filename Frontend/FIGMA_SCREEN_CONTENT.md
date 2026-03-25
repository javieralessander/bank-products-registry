# Contenido de pantallas para Figma

Este archivo contiene textos y estructura de referencia para crear tus frames en Figma rapidamente.

## SCR-01 Login

Titulo:
- `Bank Products Registry`

Subtitulo:
- `Gestion centralizada de clientes, productos y transacciones`

Campos:
- `Correo corporativo`
- `Contrasena`

Botones:
- Primario: `Entrar`
- Secundario (texto): `Recuperar acceso`

Mensaje inferior:
- `Acceso autorizado solo para personal de la entidad.`

---

## SCR-02 Dashboard

Titulo:
- `Dashboard Operativo`

KPI:
- `Clientes activos` -> `1,248`
- `Productos activos` -> `42`
- `Cuentas activas` -> `3,501`
- `Transacciones hoy` -> `286`

Tabla "Ultimas transacciones" columnas:
- Fecha
- Cuenta
- Tipo
- Monto
- Estado

Alertas:
- `12 cuentas en mora`
- `3 productos suspendidos`
- `7 transacciones manuales`

CTA principal:
- `Nueva transaccion`

---

## SCR-03 Clientes

Titulo:
- `Clientes`

Toolbar:
- Buscador placeholder: `Buscar por nombre, cedula o email`
- Filtro estado: `Todos | Activos | Inactivos`
- Boton: `Nuevo cliente`

Tabla columnas:
- Nombre completo
- Cedula
- Email
- Telefono
- Estado
- Acciones

Modal "Nuevo cliente":
- FirstName
- LastName
- NationalId
- Email
- Phone
- IsActive (switch)

Botones modal:
- `Cancelar`
- `Guardar cliente`

---

## SCR-04 Empleados

Titulo:
- `Empleados`

Toolbar:
- Buscador placeholder: `Buscar por nombre, codigo o departamento`
- Boton: `Nuevo empleado`

Tabla columnas:
- Nombre completo
- Codigo
- Email
- Departamento
- Estado
- Acciones

Modal "Nuevo empleado":
- FirstName
- LastName
- EmployeeCode
- Email
- Department
- IsActive

Botones:
- `Cancelar`
- `Guardar empleado`

---

## SCR-05 Productos Financieros

Titulo:
- `Productos financieros`

Filtros:
- Tipo de producto
- Estado
- Moneda

Tabla columnas:
- Nombre
- Tipo
- Tasa
- Monto minimo
- Moneda
- Estado
- Acciones

Modal "Nuevo producto":
- ProductName
- ProductType (`tarjeta_credito`, `prestamo`, `inversion`, `certificado`, `cuenta_ahorro`)
- InterestRate
- Description
- Status (`borrador`, `activo`, `suspendido`, `cerrado`)
- Currency (default `DOP`)
- MinimumOpeningAmount

Botones:
- `Cancelar`
- `Guardar producto`

---

## SCR-06 Cuentas de Producto

Titulo:
- `Cuentas de producto`

Toolbar:
- Buscador placeholder: `Buscar por numero de cuenta o cliente`
- Filtro estado
- Boton: `Nueva cuenta`

Tabla columnas:
- Cuenta
- Cliente
- Producto
- Ejecutivo
- Monto
- Fecha apertura
- Estado
- Acciones

Modal "Nueva cuenta":
- ClientId (select)
- FinancialProductId (select)
- EmployeeId (select)
- AccountNumber
- Amount
- OpenDate
- MaturityDate
- Status (`pendiente`, `activo`, `en_mora`, `cerrado`, `cancelado`)

---

## SCR-07 Transacciones

Titulo:
- `Transacciones`

Filtros:
- Tipo
- Rango de fecha
- Cuenta

Tabla columnas:
- Fecha
- Cuenta
- Tipo
- Monto
- Referencia
- Descripcion
- Acciones

Modal "Nueva transaccion":
- AccountProductId
- TransactionType (`deposito`, `retiro`, `pago`, `transferencia`, `cargo`)
- Amount
- TransactionDate
- Description
- ReferenceNumber

Botones:
- `Cancelar`
- `Registrar transaccion`

---

## SCR-08 Reporte de Portafolio

Titulo:
- `Reporte de portafolio por cliente`

Control superior:
- Select: `Seleccionar cliente`
- Boton: `Consultar`

KPI:
- `TotalProducts`
- `CurrentBalance`
- `TotalDeposits`
- `TotalWithdrawals`

Tabla cuentas:
- AccountNumber
- ProductName
- Status
- Amount
- OpenDate
- TotalTransactions
- Deposits
- Withdrawals

Botones secundarios:
- `Exportar CSV` (opcional UI)
- `Imprimir` (opcional UI)

---

## Sidebar de navegacion sugerida

Items:
- Dashboard
- Clientes
- Empleados
- Productos
- Cuentas
- Transacciones
- Reportes
- Configuracion (placeholder)

Footer sidebar:
- Usuario actual
- Rol
- Cerrar sesion


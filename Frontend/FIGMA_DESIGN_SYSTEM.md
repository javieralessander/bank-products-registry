# Figma Design Kit - Bank Products Registry

Este documento define un diseno listo para construir en Figma usando el backend actual del proyecto.

## 1) Objetivo del diseno

Crear una interfaz admin para gestionar:

- Clientes
- Empleados
- Productos financieros
- Cuentas de productos
- Transacciones
- Reporte de portafolio por cliente

El diseno esta orientado a escritorio (dashboard administrativo) con base mobile opcional.

---

## 2) Estructura recomendada del archivo Figma

Crear un archivo llamado: `Bank Products Registry - Admin UI`

Paginas:

1. `00 Foundations`
2. `01 Components`
3. `02 Screens - Desktop`
4. `03 Prototype Flow`

---

## 3) Foundations (00 Foundations)

### 3.1 Grid y layout

- Desktop frame base: `1440 x 1024`
- Grid: `12 columnas`
- Margin horizontal: `80`
- Gutter: `24`
- Sidebar fija: `264 px`
- Contenido: ancho fluido con maximo `1080 px`

### 3.2 Espaciado (8pt system)

Usar variable de espaciado:

- `space-4 = 4`
- `space-8 = 8`
- `space-12 = 12`
- `space-16 = 16`
- `space-24 = 24`
- `space-32 = 32`
- `space-40 = 40`
- `space-48 = 48`

### 3.3 Border radius

- `radius-sm = 8`
- `radius-md = 12`
- `radius-lg = 16`
- `radius-xl = 20`
- `radius-pill = 999`

### 3.4 Tipografia

Fuente sugerida: `Inter`

Estilos:

- `Display/32/Semibold` -> 32 / 40 / 600
- `Heading/24/Semibold` -> 24 / 32 / 600
- `Title/20/Semibold` -> 20 / 28 / 600
- `Body/16/Regular` -> 16 / 24 / 400
- `Body/16/Medium` -> 16 / 24 / 500
- `Body/14/Regular` -> 14 / 20 / 400
- `Body/14/Medium` -> 14 / 20 / 500
- `Caption/12/Medium` -> 12 / 16 / 500

### 3.5 Colores (variables)

Neutrales:

- `neutral/0 = #FFFFFF`
- `neutral/50 = #F8FAFC`
- `neutral/100 = #F1F5F9`
- `neutral/200 = #E2E8F0`
- `neutral/400 = #94A3B8`
- `neutral/600 = #475569`
- `neutral/800 = #1E293B`
- `neutral/900 = #0F172A`

Brand:

- `brand/50 = #EEF2FF`
- `brand/100 = #E0E7FF`
- `brand/500 = #4F46E5`
- `brand/600 = #4338CA`
- `brand/700 = #3730A3`

Feedback:

- `success/50 = #ECFDF3`
- `success/600 = #16A34A`
- `warning/50 = #FFFBEB`
- `warning/600 = #D97706`
- `danger/50 = #FEF2F2`
- `danger/600 = #DC2626`
- `info/50 = #EFF6FF`
- `info/600 = #2563EB`

### 3.6 Sombras

- `shadow-sm = 0 1px 2px rgba(15, 23, 42, 0.06)`
- `shadow-md = 0 8px 20px rgba(15, 23, 42, 0.08)`
- `shadow-lg = 0 18px 40px rgba(15, 23, 42, 0.12)`

---

## 4) Components (01 Components)

Crear componentes con variantes.

### 4.1 Button

Variantes:

- `Type: Primary | Secondary | Ghost | Danger`
- `Size: Md | Sm`
- `State: Default | Hover | Disabled`

Medidas:

- Md: alto 44, padding 12/16
- Sm: alto 36, padding 8/12

### 4.2 Input text

Variantes:

- `State: Default | Focus | Error | Disabled`
- `HasIcon: true/false`

Partes:

- Label
- Field (alto 44)
- Helper/Error text

### 4.3 Select

Mismo patron que Input + caret a la derecha.

### 4.4 Badge de estado

Usar por entidad:

- ProductStatus: borrador, activo, suspendido, cerrado
- AccountProductStatus: pendiente, activo, en_mora, cerrado, cancelado
- IsActive (clientes/empleados): activo, inactivo

Color mapping:

- `activo`: fondo success/50, texto success/600
- `pendiente|borrador`: fondo warning/50, texto warning/600
- `en_mora|suspendido`: fondo danger/50, texto danger/600
- `cerrado|cancelado|inactivo`: fondo neutral/100, texto neutral/600

### 4.5 Table

Estructura:

- Header row (fondo neutral/50)
- Data rows (alto 56)
- Zebra opcional
- Paginacion inferior

Columnas recomendadas por modulo:

- Clientes: Nombre, Cedula, Email, Telefono, Estado, Acciones
- Empleados: Nombre, Codigo, Email, Departamento, Estado, Acciones
- Productos: Nombre, Tipo, Tasa, Monto minimo, Moneda, Estado, Acciones
- Cuentas: Cuenta, Cliente, Producto, Ejecutivo, Monto, Estado, Acciones
- Transacciones: Fecha, Cuenta, Tipo, Monto, Referencia, Descripcion, Acciones

### 4.6 KPI Card

Partes:

- Label
- Valor principal
- Delta/auxiliar
- Icono

---

## 5) Screens (02 Screens - Desktop)

### 5.1 Login (`SCR-01`)

Objetivo: acceso de operador.

Elementos:

- Logo / nombre del sistema
- Input email
- Input password
- Button "Entrar"
- Link "Recuperar acceso" (opcional)

### 5.2 Dashboard (`SCR-02`)

Objetivo: resumen operativo.

Bloques:

1. Header con titulo y filtros (fecha, estado)
2. Fila de KPI cards:
   - Clientes activos
   - Productos activos
   - Cuentas activas
   - Transacciones del dia
3. Tabla "Ultimas transacciones"
4. Widget lateral "Alertas":
   - Cuentas en mora
   - Productos suspendidos

### 5.3 Clientes (`SCR-03`)

Objetivo: CRUD de clientes.

Bloques:

- Toolbar con buscador y boton `Nuevo cliente`
- Tabla de clientes
- Drawer/modal para crear o editar cliente con campos:
  - FirstName
  - LastName
  - NationalId
  - Email
  - Phone
  - IsActive

### 5.4 Empleados (`SCR-04`)

Objetivo: CRUD de empleados.

Formulario:

- FirstName
- LastName
- EmployeeCode
- Email
- Department
- IsActive

### 5.5 Productos financieros (`SCR-05`)

Objetivo: CRUD de catalogo de productos.

Formulario:

- ProductName
- ProductType
- InterestRate
- Description
- Status
- Currency
- MinimumOpeningAmount

### 5.6 Cuentas de producto (`SCR-06`)

Objetivo: asignar productos a clientes.

Formulario:

- ClientId (select searchable)
- FinancialProductId
- EmployeeId
- AccountNumber
- Amount
- OpenDate
- MaturityDate
- Status

### 5.7 Transacciones (`SCR-07`)

Objetivo: registro de movimientos.

Formulario:

- AccountProductId
- TransactionType
- Amount
- TransactionDate
- Description
- ReferenceNumber

### 5.8 Reporte de portafolio (`SCR-08`)

Objetivo: vista analitica por cliente (`/api/reports/clients/{clientId}/portfolio`).

Bloques:

- Selector de cliente
- KPI:
  - TotalProducts
  - CurrentBalance
  - TotalDeposits
  - TotalWithdrawals
- Tabla de cuentas:
  - AccountNumber
  - ProductName
  - Status
  - Amount
  - OpenDate
  - TotalTransactions
  - Deposits
  - Withdrawals

---

## 6) Flujos de prototipo (03 Prototype Flow)

Conectar en Figma:

1. Login -> Dashboard
2. Dashboard -> Clientes -> Modal Nuevo cliente -> Guardar -> Toast de exito
3. Dashboard -> Productos -> Nuevo producto -> Guardar
4. Dashboard -> Cuentas -> Nueva cuenta -> Guardar
5. Dashboard -> Transacciones -> Nueva transaccion -> Guardar
6. Dashboard -> Reportes -> Portafolio de cliente

Animacion sugerida:

- `Smart Animate`, 200ms, Ease out.

---

## 7) Responsive minimo

Agregar frames:

- Tablet: `1024 x 1366`
- Mobile: `390 x 844`

Prioridad responsive:

- Sidebar colapsada en tablet
- Sidebar en drawer en mobile
- Tablas con horizontal scroll en mobile

---

## 8) Checklist rapido para construir en Figma

1. Crear variables de color, spacing, radius y sombras.
2. Crear text styles.
3. Construir componentes base (Button/Input/Badge/Table/KPI).
4. Construir layout shell (Sidebar + Topbar + Content).
5. Duplicar shell y completar `SCR-02` a `SCR-08`.
6. Conectar prototipo principal.
7. Publicar pagina de handoff para desarrollo.

---

## 9) Nombre de frames sugeridos

- `SCR-01 Login`
- `SCR-02 Dashboard`
- `SCR-03 Clientes`
- `SCR-04 Empleados`
- `SCR-05 Productos`
- `SCR-06 Cuentas`
- `SCR-07 Transacciones`
- `SCR-08 Reporte Portafolio`

Con esto tienes una base consistente para construir el diseno visual en Figma alineado a los endpoints y DTOs reales del proyecto.

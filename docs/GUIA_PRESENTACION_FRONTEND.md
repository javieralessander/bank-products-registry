# Guía paso a paso — presentación del Frontend (BankRED)

Documento independiente del README: sirve para ensayar y mostrar al profesor el flujo en la aplicación web.

---

## 1. Antes de empezar

1. Tener **MySQL** en ejecución y la base creada según el proyecto (cadena de conexión en `Backend/.../appsettings.Development.json`).
2. Levantar la **API** (`BankProductsRegistry.Api`) en el puerto configurado, por defecto **http://localhost:5039** (perfil `http` en `launchSettings.json`).
3. Levantar el **Frontend** (`BankProductsRegistry.Frontend`), por defecto **http://localhost:5160**.
4. Confirmar que en `Frontend/.../appsettings.Development.json` la URL `Api:BaseUrl` apunta a la API (p. ej. `http://localhost:5039/`).

Si algo no carga, revisa que la API responda (por ejemplo `/health` o Swagger si está habilitado).

---

## 2. Usuarios de demostración (desarrollo)

Con `Authentication:SeedUsers:Enabled` en `true` (típico en `appsettings.Development.json`), suelen existir:

| Rol        | Usuario sugerido | Contraseña (ejemplo dev) |
|-----------|-------------------|---------------------------|
| Admin     | `admin`           | `Admin2026`               |
| Operador  | `operador`        | `Operador2026`            |
| Consulta  | `consulta`        | `Consulta2026`            |

Las contraseñas exactas están en tu `appsettings.Development.json` del API.

**Cliente:** puedes **registrar** un usuario nuevo desde la pantalla de registro del Frontend y luego (en un entorno real) vincularlo a un cliente; para la demo, usa un flujo que ya tengas con cliente vinculado o crea datos de prueba según tu base.

---

## 3. Orden sugerido de la demo (rápido, ~15 minutos)

### A. Personal interno — Admin u Operador

1. Abre el Frontend (**http://localhost:5160**), entra a **Iniciar sesión**.
2. Inicia sesión con **admin** u **operador**.
3. **Dashboard:** panorama general.
4. **Clientes:** listado; entrar a un cliente y ver detalle/portafolio si aplica.
5. **Prod. Financieros:** catálogo de productos.
6. **Prod. Contratados:** contratos; botón **Solicitudes pendientes** si hay solicitudes de clientes.
7. **Transacciones:** historial global (staff).
8. **Empleados, Bloqueos, Notif. de Viaje, Notificaciones** según tiempo; **Usuarios** solo si entras como **Admin**.

### B. Solo lectura — Consulta

1. Cierra sesión e inicia con **consulta**.
2. Navega **Clientes**, **Prod. Financieros**, **Prod. Contratados**, **Transacciones**: debe poder **ver** listados.
3. Comprueba que **no** aparezcan botones de crear/editar/eliminar donde corresponda (solo lectura).

### C. Portal cliente

1. Cierra sesión. Entra con un usuario **Cliente** (registrado y con perfil vinculado a cliente, según tu datos).
2. **Mi Resumen:** saldo y productos resumidos.
3. **Transferencias:** formulario **Registrar movimiento** (simulación) y tabla de historial propio.
4. **Mis Productos:** listado; filtro **Pendiente** si solicitaste un producto.
5. **Solicitar producto:** envío de solicitud → vuelve a **Mis Productos** con mensaje y estado pendiente hasta que staff apruebe en **Solicitudes pendientes**.
6. **Avisos de Viaje** y **Seguridad (bloqueos):** según lo que quieras mostrar; en formularios de cliente el listado de cuentas suele estar filtrado a productos propios.

### D. Cierre

- Mencionar que la **API** valida roles y reglas (límites, bloqueos, pendientes).
- Si el profesor pregunta por **tarjeta de crédito** y transferencias: en la UI se aclara que es **simulación**; en producción los cargos vienen por la red de pagos.

---

## 4. Checklist mínimo el día de la presentación

- [ ] MySQL arriba y migraciones aplicadas.
- [ ] API corriendo sin errores en consola.
- [ ] Frontend corriendo y login carga.
- [ ] Probar al menos un login **Admin** y uno **Consulta** y uno **Cliente** (o el flujo de registro + login).
- [ ] Tener a mano esta guía o un guion con los 3 roles.

---

## 5. Commits recientes en `develop` (referencia)

Los cambios quedaron agrupados en commits con mensajes del estilo:

- `feat(api): transacciones desde portal cliente y reglas de viajes por rol`
- `feat(frontend): página Acceso denegado y ajustes de autenticación`
- `fix(frontend): autorización por rol y UI solo lectura para Consulta`
- `feat(frontend): movimientos para cliente, mensajes de solicitud y resumen de cuentas`
- `feat(frontend): helper para filtrar productos del cliente en bloqueos y viajes`

Para listarlos: `git log --oneline -15`

---

*Última actualización alineada con la rama `develop` del repositorio.*

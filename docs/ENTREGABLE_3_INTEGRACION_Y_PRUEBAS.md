# Entregable 3 — Integración y funcionalidades

**Proyecto:** BankRED — Registro de productos bancarios  
**Fecha:** 7 de abril de 2026  
**Objetivo de la entrega:** Documentar la comunicación front-end ↔ back-end, las funcionalidades principales, las pruebas realizadas y la corrección de errores.

---

## 1. Resumen ejecutivo

El sistema está integrado mediante una **API REST** (.NET 9) consumida por una aplicación web **ASP.NET Core MVC** (Frontend). La autenticación usa **JWT** (access + refresh); el front envía el token en las cabeceras HTTP y actúa como **cliente HTTP** hacia la API configurada en `Api:BaseUrl`.

**URL de referencia (despliegue):** `https://bank-products-registry-production.up.railway.app`  
**Documentación interactiva API:** `https://bank-products-registry-production.up.railway.app/swagger`

---

## 2. Integración front-end y back-end

### 2.1 Mecanismo

| Aspecto | Implementación |
|--------|----------------|
| Cliente HTTP | `HttpClient` registrado en DI del Frontend |
| Base URL | `appsettings.json` / `appsettings.Development.json` → `Api:BaseUrl` (p. ej. `http://localhost:5039/`) |
| Autenticación | Tras login, el token JWT se guarda en cookie de sesión y se reenvía como `Authorization: Bearer` |
| Formato | JSON (`application/json`); enums y fechas alineados con la serialización de la API |
| Errores | Respuestas `ProblemDetails` parseadas con `ApiErrorParser` para mostrar mensajes al usuario |
| PDFs | Rutas `ReportsExportController` que hacen de proxy a endpoints `GET .../pdf` de la API |

### 2.2 Sincronización de contexto de cliente

Si el usuario **Cliente** vincula su perfil después del login, el back-end **refresca el claim `client_id` en cada request** a partir de la base de datos, evitando inconsistencias con el JWT emitido antes del vínculo.

### 2.3 Módulos conectados a la API (evidencia de integración)

- **Auth:** login, registro, refresh, logout  
- **Dashboard** (staff): métricas vía API  
- **Clientes:** CRUD y detalle  
- **Empleados, Usuarios** (admin): gestión  
- **Productos financieros** y **productos contratados** (cuentas), límites, bloqueos, auditoría  
- **Solicitudes de producto** (cliente → staff)  
- **Transacciones** y **portal cliente** (resumen, movimientos simulados)  
- **Avisos de viaje**, **notificaciones**, **reportes** (JSON y exportación PDF: portafolio, historial/score crédito, **estado de cuenta por rango de fechas**)

---

## 3. Funcionalidades principales (por rol)

| Rol | Funcionalidades destacadas |
|-----|----------------------------|
| **Admin** | Usuarios, empleados, catálogo, contratos, límites, aprobaciones, reportes amplios |
| **Operador** | Clientes, productos, contratos, transacciones, bloqueos, solicitudes pendientes |
| **Consulta** | Consulta de datos (solo lectura donde aplique) |
| **Cliente** | Portal: resumen, productos propios, transferencias/movimientos, solicitudes, avisos de viaje, exportación de PDFs de estado de cuenta e informes de crédito |

---

## 4. Pruebas de integración realizadas

> Nota: no hay proyecto de tests automatizados `xUnit` con `WebApplicationFactory` en el repositorio; las pruebas de integración documentadas son **manuales y de verificación técnica**, complementadas con compilación y salud del servicio.

### 4.1 Pruebas de conectividad y salud

- `GET /health` — API responde OK (local y Railway)  
- Verificación de **login** y acceso a **Swagger** con token donde aplica  

### 4.2 Pruebas funcionales manuales (checklist)

- Login por rol (Admin, Operador, Consulta, Cliente)  
- Flujo cliente: ver productos contratados → registrar movimiento → ver historial  
- Flujo staff: listar solicitudes → aprobar/rechazar según reglas  
- Validación de **403/409** con mensaje legible (límites, bloqueos, aviso de viaje)  
- **Exportación PDF** (estado de cuenta, historial de crédito) desde el portal o rutas de exportación  

### 4.3 Pruebas de compilación

```bash
dotnet build Backend/BankProductsRegistry.Api/BankProductsRegistry.Api.csproj
dotnet build Frontend/BankProductsRegistry.Frontend/BankProductsRegistry.Frontend.csproj
```

Ejecutadas en desarrollo para asegurar que API y Frontend compilan sin errores tras cambios de integración.

---

## 5. Errores detectados y correcciones (integración)

| Problema | Causa | Solución |
|----------|--------|----------|
| Fallo al deserializar transacciones | La API enviaba `transactionChannel` (y a veces tipo) como **número** (enum), el modelo del front esperaba **solo string** | Convertidor JSON que acepta **string o número**; ajuste de totales cuando el tipo de transacción llega como `"1"` (depósito) |
| Token sin `client_id` actualizado | JWT emitido antes de vincular cliente | Middleware de validación JWT: **reinyectar `ClientId`** desde la BD |
| Mensajes genéricos en errores API | Cuerpo `ProblemDetails` no mostrado | `ApiErrorParser.ExtractMessageAsync` en controladores clave |

---

## 6. Optimización y calidad (breve)

- **Separación de responsabilidades:** lógica de negocio y persistencia en API; UI solo orquesta llamadas y presentación  
- **Menos ida y vuelta fallida:** mensajes de error extraídos del cuerpo de respuesta  
- **Exportaciones PDF** generadas en servidor (QuestPDF), descarga vía proxy sin duplicar lógica en el navegador  
- **UI portal cliente:** tema dedicado (`layout-client`) para legibilidad y coherencia  

---

## 7. Documentación técnica y manual de usuario

### 7.1 Documentación técnica (ubicación en el repo)

| Recurso | Contenido |
|---------|-----------|
| `README.md` (raíz) | Cómo levantar API y Frontend, variables de entorno, endpoints principales, seguridad |
| Swagger (API) | Contratos de endpoints, pruebas desde el navegador |
| `docs/GUIA_PRESENTACION_FRONTEND.md` | Guion rápido para demo con el profesor (orden sugerido por rol) |

### 7.2 Manual de usuario breve

1. **Acceso:** abrir la URL del Frontend; usar **Iniciar sesión** (usuarios de prueba en desarrollo: ver `README` y `appsettings.Development.json` del API).  
2. **Personal interno:** menú lateral: Dashboard, Clientes, Productos financieros, Productos contratados, Transacciones, etc., según permisos del rol.  
3. **Cliente:** portal con **Mi Resumen**, **Transferencias**, **Mis Productos**, **Solicitar producto**; puede **exportar PDFs** de estado de cuenta e informes de crédito donde estén habilitados.  
4. **Cierre de sesión** desde el menú de usuario.  

*(Para una demo guiada paso a paso, usar `docs/GUIA_PRESENTACION_FRONTEND.md`.)*

---

## 8. Conclusión

El entregable cumple con un **sistema parcialmente funcional** orientado a la integración real entre capas: el Frontend consume la API de forma autenticada, cubre los flujos principales por rol, incluye **pruebas manuales y de compilación** documentadas, y registra **correcciones** típicas de proyectos integrados (serialización JSON, claims JWT, mensajes de error). La documentación técnica y el manual breve permiten reproducir el despliegue y la demostración del sistema.

---

*Documento elaborado para el Entregable 3 — Integración y funcionalidades (comunicación FE-BE, pruebas y documentación).*

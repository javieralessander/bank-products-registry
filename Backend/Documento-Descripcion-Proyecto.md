# Documento de Descripcion del Proyecto

## 1. Datos generales

- **Nombre del proyecto:** Sistema de Registro de Productos Bancarios
- **Repositorio GitHub:** `https://github.com/javieralessander/bank-products-registry`
- **URL del aplicativo en produccion:** `https://bank-products-registry-production.up.railway.app`
- **Swagger en produccion:** `https://bank-products-registry-production.up.railway.app/swagger/index.html`
- **Alcance actual:** Backend

## 2. Descripcion del proyecto

El proyecto consiste en el desarrollo de una API RESTful para la administracion y el registro de productos bancarios. El sistema permite gestionar clientes, empleados, productos financieros, productos contratados por los clientes y transacciones asociadas.

La solucion fue diseñada para centralizar la informacion financiera operativa en un unico sistema, reducir errores manuales y permitir consultas consolidadas por cliente.

## 3. Problema identificado

En muchos entornos bancarios pequeños o en procesos administrativos internos, la información sobre los productos financieros suele manejarse de forma dispersa, a veces de manera manual o en diferentes sistemas. Esto puede generar desorden en los datos, retrasos en los procesos y dificultades para consultar la información cuando se necesita:

- duplicidad de datos
- retrasos administrativos
- dificultad para consolidar informacion financiera confiable
- problemas para dar seguimiento a cuentas, prestamos, inversiones y transacciones
- poca visibilidad del portafolio total de cada cliente

## 4. Modelo de arquitectura de software elegido

El backend implementa una **arquitectura cliente-servidor con API REST monolitica**, organizada por capas con una estructura cercana a **MVC por responsabilidades**.

### Estructura aplicada

- **Controllers:** exponen los endpoints REST
- **Services:** concentran la logica de reportes
- **Data:** maneja el `DbContext`, configuraciones, migraciones y carga inicial
- **Models:** representan las entidades del dominio
- **Dtos:** definen los contratos de entrada y salida del API

### Tipo de integracion

La comunicacion principal es **point-to-point** entre:

- cliente consumidor (Swagger o Postman)
- API REST
- base de datos MySQL

### Justificacion

Este modelo fue elegido porque:

- simplifica el desarrollo del parcial
- facilita el mantenimiento del backend
- permite separar responsabilidades tecnicas
- soporta crecimiento futuro hacia un frontend web o una arquitectura mas distribuida

## 5. Persistencia de base de datos elegida

La persistencia seleccionada es **MySQL**, utilizando **Entity Framework Core** con enfoque **Code First**.

### Caracteristicas implementadas

- migraciones automáticas al arrancar la aplicacion
- relaciones entre entidades
- auditoria basica con `CreatedAt` y `UpdatedAt`
- carga inicial de datos de prueba
- compatibilidad con base local y con Railway

### Entidades persistidas

- `Clients`
- `Employees`
- `FinancialProducts`
- `AccountProducts`
- `Transactions`

## 6. Funcionalidad principal del sistema

La API permite:

- registrar clientes
- registrar empleados
- crear productos financieros como tarjetas, prestamos, inversiones y certificados
- asignar productos contratados a clientes
- registrar transacciones financieras
- consultar el portafolio consolidado de un cliente

## 7. Presentacion de la funcionalidad del sistema

Para presentar el sistema se recomienda esta secuencia:

### Paso 1. Verificar que el API esta corriendo

- Abrir `https://bank-products-registry-production.up.railway.app/swagger`
- Confirmar que Swagger carga correctamente

### Paso 2. Mostrar datos base del sistema

- `GET /api/clients`
- `GET /api/employees`
- `GET /api/financial-products`

Esto permite evidenciar que el sistema ya tiene estructura funcional y datos iniciales de prueba.

### Paso 3. Registrar un cliente nuevo

Usar `POST /api/clients` con un cliente de prueba.

### Paso 4. Asociar un producto bancario al cliente

Usar `POST /api/account-products` para crear un producto contratado vinculado al cliente, al producto financiero y al empleado.

### Paso 5. Registrar una transaccion

Usar `POST /api/transactions` para registrar un deposito o pago.

### Paso 6. Demostrar una regla de negocio

Intentar registrar una transaccion que deje el balance en negativo para evidenciar que el sistema valida la operacion y responde con error controlado.

### Paso 7. Mostrar el reporte consolidado

Usar `GET /api/reports/clients/{clientId}/portfolio` para demostrar la consolidacion del portafolio del cliente.

## 8. Conclusion

El backend desarrollado cumple con los objetivos principales del parcial: modelado de datos, implementacion de API RESTful, conexion con base de datos, persistencia funcional, despliegue en Railway y presentacion de la funcionalidad principal del sistema.

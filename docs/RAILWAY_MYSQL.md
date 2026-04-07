# MySQL en Railway y en el repositorio

## Que significa “tener la DB en el repositorio”

En este proyecto la base de datos **no es codigo fuente**: los datos viven en el servidor MySQL. Lo que si esta en el repo es:

| Artefacto | Para que sirve |
|-----------|----------------|
| `docker-compose.yml` | Levantar **MySQL local** con el mismo nombre de base y usuario de ejemplo que el desarrollo. |
| `.env.example` | Plantilla de variables (sin secretos) para local y notas para Railway. |
| `Backend/.../MySqlConnectionResolver.cs` | Lee `MYSQL_URL`, `ConnectionStrings__DefaultConnection`, etc., y corrige nombre de base vacio en URLs de Railway. |

## Pantalla “Connect Repo” en el servicio MySQL

En **MySQL → Settings → Connect Source**, “Connect Repo” sirve para construir un servicio **desde un repositorio** (por ejemplo una imagen custom). El **plugin MySQL de Railway** es un servicio **gestionado**: **no hace falta** conectarlo a GitHub.

Lo que debes enlazar es el **servicio de la API** (`bank-products-registry`) al repo; el MySQL se **referencia** con variables.

## Vincular MySQL al API (recomendado)

1. Abre el proyecto en Railway y entra al servicio **del backend/API** (no al de MySQL).
2. Pestaña **Variables**.
3. **+ New Variable** → pestaña **Reference** (o “Variable Reference”).
4. Selecciona el recurso **MySQL** y variables como:
   - **`MYSQL_URL`** (preferido), o
   - `MYSQLHOST`, `MYSQLPORT`, `MYSQLUSER`, `MYSQLPASSWORD`, `MYSQLDATABASE`.
5. Guarda y **vuelve a desplegar** el API.

Asi el API “selecciona” la base desde el panel usando referencias; los secretos no van al codigo.

## URL publica vs interna

- **Public Networking** en MySQL (ej. `centerbeam.proxy.rlwy.net:52379`) sirve para conectar desde tu **PC** o herramientas externas.
- El **contenedor del API** en el mismo proyecto debe usar la **red interna** (`mysql.railway.internal` o lo que inyecte `MYSQL_URL` interno).

Si la URL no trae nombre de base en el path, el API usa `MYSQLDATABASE` o, por defecto en hosts Railway, la base `railway`.

## Desarrollo local con el mismo esquema

```bash
docker compose up -d
```

Espera a que el healthcheck de MySQL este verde. Luego configura la cadena como en `.env.example` y ejecuta el API con `dotnet run`.

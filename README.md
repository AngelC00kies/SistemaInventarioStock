# Sistema de Inventario

Sistema web para registrar, controlar y gestionar el ingreso, la salida y el stock de
productos en uno o varios almacenes.

- **Frontend:** Vue 3 + Vite + Tailwind CSS + Pinia + Vue Router + Chart.js (iconos Lucide)
- **Backend:** ASP.NET Core 8 Web API + Entity Framework Core + SQL Server + JWT
- **Reportes:** QuestPDF (PDF) y ClosedXML (Excel) generados en el servidor

---

## 1. Stack tecnológico

### Frontend

| Tecnología | Versión | Uso |
|---|---|---|
| [Vue](https://vuejs.org/) | 3.5 | Framework de interfaz |
| [Vite](https://vite.dev/) | 8.x | Servidor de desarrollo y build |
| [@vitejs/plugin-vue](https://github.com/vitejs/vite-plugin-vue) | 6.x | Compilación SFC de Vue |
| [Tailwind CSS](https://tailwindcss.com/) | 3.4 | Estilos utilitarios |
| [PostCSS](https://postcss.org/) / [Autoprefixer](https://github.com/postcss/autoprefixer) | 8.x / 10.x | Pipeline CSS |
| [Pinia](https://pinia.vuejs.org/) | 4.x | Gestión de estado |
| [Vue Router](https://router.vuejs.org/) | 4.x | Enrutamiento y guardas por rol |
| [Axios](https://axios-http.com/) | 1.x | Cliente HTTP |
| [Chart.js](https://www.chartjs.org/) + [vue-chartjs](https://vue-chartjs.org/) | 4.x / 5.x | Gráficas del dashboard |
| [@lucide/vue](https://lucide.dev/) | 1.x | Iconos |

### Backend

| Tecnología | Versión | Uso |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | 8.0 | Runtime y compilador C# |
| [ASP.NET Core](https://learn.microsoft.com/aspnet/core/) | 8.0 | Web API REST |
| [Entity Framework Core](https://learn.microsoft.com/ef/core/) (SqlServer) | 8.0.11 | ORM y migraciones |
| [Microsoft.EntityFrameworkCore.Design](https://learn.microsoft.com/ef/core/cli/dotnet) | 8.0.11 | Herramienta `dotnet ef` |
| [dotnet-ef](https://learn.microsoft.com/ef/core/cli/dotnet) (herramienta local) | 8.0.11 | Migraciones desde CLI |
| [JwtBearer](https://learn.microsoft.com/aspnet/core/security/authentication/) | 8.0.11 | Autenticación JWT |
| [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) (Swagger) | 6.4.0 | Documentación de la API |
| [QuestPDF](https://www.questpdf.com/) | 2024.12.3 | Generación de PDF |
| [ClosedXML](https://closedxml.github.io/) | 0.104.2 | Generación de Excel |

### Base de datos

| Tecnología | Versión | Uso |
|---|---|---|
| [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) | 2019 / 2022 (Express o Developer, gratis) | Base de datos |
| [SSMS](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) (opcional) | última | Administración gráfica |

### Herramientas de desarrollo

| Herramienta | Versión | Para qué |
|---|---|---|
| [Git](https://git-scm.com/downloads) | cualquier | Descargar el código |
| [Node.js](https://nodejs.org/) | 18+ (LTS, probado con 22) | Ejecutar el frontend |
| npm | incluido con Node.js | Instalar dependencias |
| [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/) (opcional) | — | Editar/correr el backend |

---

## 2. Instalación en otra máquina (paso a paso)

### Paso 1 — Descargar el código

```powershell
git clone https://github.com/AngelC00kies/SistemaInventarioStock.git
cd SistemaInventarioStock
```

Si no tiene Git, descargue el ZIP desde GitHub y extráigalo.

### Paso 2 — Instalar los requisitos

| Software | Enlace de descarga | Comando para verificar |
|---|---|---|
| .NET SDK 8.0 | <https://dotnet.microsoft.com/download/dotnet/8.0> | `dotnet --version` → debe imprimir `8.x` |
| Node.js LTS | <https://nodejs.org/> | `node -v` → `v18+` |
| npm | viene con Node.js | `npm -v` |
| SQL Server Express/Developer | <https://www.microsoft.com/sql-server/sql-server-downloads> | Abrir SSMS y conectarse |
| SSMS (opcional) | <https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms> | — |

> **SQL Server:** al instalar elija la edición **Express** o **Developer** (gratuitas) y
> marque **"Autenticación mixta"** (modo SQL Server + Windows) para poder usar cualquiera
> de las dos formas de conexión. Anote el nombre del servidor que aparece en el
> paso de configuración de la instancia (por ejemplo `MI-PC\SQLEXPRESS`).

### Paso 3 — Configurar la cadena de conexión

La conexión vive en dos archivos dentro de
`backend/src/SistemaInventario.Api/`:

| Archivo | Cuándo se usa |
|---|---|
| `appsettings.Development.json` | **Tiene prioridad** cuando se ejecuta en modo desarrollo (es lo normal con `dotnet run`). Viene con `Server=localhost` |
| `appsettings.json` | Valores por defecto / publicación |

**Caso A — SQL Server instalado en la misma máquina (lo más común):**
no hace falta editar nada. `Server=localhost` funciona siempre.

**Caso B — SQL Server en otra instancia o nombre distinto** (ej. `MI-PC\SQLEXPRESS`),
edite ambos archivos y cambie `Server=`:

```json
"DefaultConnection": "Server=MI-PC\\SQLEXPRESS;Database=SistemaInventarioDB;Trusted_Connection=True;TrustServerCertificate=True"
```

**Caso C — autenticación SQL Server (usuario `sa`)** en lugar de Windows:

```json
"DefaultConnection": "Server=MI-PC\\SQLEXPRESS;Database=SistemaInventarioDB;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

Parámetros explicados:

| Parámetro | Significado |
|---|---|
| `Server` | Nombre de la instancia. `localhost` = misma máquina. Instancias con nombre: `SERVIDOR\SQLEXPRESS` (en JSON escape la barra: `\\`) |
| `Database` | Nombre que tendrá la base de datos (la crea automáticamente) |
| `Trusted_Connection=True` | Autenticación de Windows (sin usuario/password) |
| `User Id` / `Password` | Autenticación SQL Server (reemplaza a `Trusted_Connection`) |
| `TrustServerCertificate=True` | Evita el error de certificado TLS en desarrollo |

> La base de datos **no existe todavía**: el backend la crea sola con las migraciones
> en el Paso 4.

### Paso 4 — Backend (API)

```powershell
cd backend
dotnet tool restore        # restaura dotnet-ef 8.0.11 (obligatorio)
dotnet restore             # descarga los paquetes NuGet
dotnet run --project src/SistemaInventario.Api
```

- `dotnet tool restore` instala la herramienta `dotnet-ef` declarada en
  `backend/.config/dotnet-tools.json`. **Sin este paso el comando `dotnet ef` no existe.**
- Al arrancar, la API aplica las migraciones pendientes, **crea la base de datos si no
  existe** y carga los datos de ejemplo (categorías, proveedores, almacenes, productos
  y movimientos históricos). No hay que ejecutar nada más.
- API: `http://localhost:5080`
- Swagger (desarrollo): `http://localhost:5080/swagger`

**Paso 4 opcional — aplicar migraciones a mano** (solo si quiere forzarlas antes de
arrancar):

```powershell
cd backend
dotnet tool restore
dotnet ef database update --project src/SistemaInventario.Infrastructure --startup-project src/SistemaInventario.Api
```

### Paso 5 — Frontend (Vue)

Abrir **una segunda terminal** (la del backend debe seguir abierta):

```powershell
cd frontend
npm install
npm run dev
```

- Aplicación: `http://localhost:5173`
- El servidor de desarrollo proxea `/api` hacia `http://localhost:5080`
  (configurado en `frontend/vite.config.js`).

También puede usar los accesos directos `iniciar-backend.bat` y `iniciar-frontend.bat`
en la raíz del proyecto (requieren haber ejecutado `npm install` y
`dotnet tool restore`/`dotnet restore` una primera vez).

### Paso 6 — Verificar

1. Abrir `http://localhost:5080/swagger` → debe cargar la documentación de la API.
2. Abrir `http://localhost:5173` → debe cargar la pantalla de login.
3. Iniciar sesión con un usuario de la tabla de abajo.
4. El dashboard debe mostrar gráficas y el conteo de productos.

---

## 3. Usuarios de demostración

| Usuario | Contraseña | Rol | Permisos |
|---|---|---|---|
| `admin` | `Admin123!` | Admin | Acceso total (usuarios, reportes, configuración) |
| `operador` | `Usuario123!` | Usuario | Gestión de productos, catálogos y movimientos |
| `auditor` | `Usuario123!` | Auditor | Solo lectura y exportación de reportes |

---

## 4. Errores comunes y soluciones

| Error / síntoma | Causa | Solución |
|---|---|---|
| `dotnet-ef` no se reconoce o `Cannot find command 'dotnet ef'` | No se restauró la herramienta local | Ejecute `dotnet tool restore` dentro de `backend/` |
| `A network-related or instance-specific error occurred... (localhost)` | SQL Server no está corriendo o el `Server` es incorrecto | Inicie el servicio **SQL Server (SQLEXPRESS)** y verifique `Server=` en `appsettings.Development.json` |
| `Login failed for user '...'` | Autenticación incorrecta | Use `Trusted_Connection=True` (Windows) o `User Id=sa;Password=...` correctamente |
| `The certificate chain was issued by an authority that is not trusted` | TLS del certificado SQL | Mantenga `TrustServerCertificate=True` en la cadena |
| `Unable to obtain lock...` o puerto `5080` ocupado | Otra instancia de la API ya está corriendo | Cierre el proceso anterior o cambie `applicationUrl` en `Properties/launchSettings.json` |
| `EADDRINUSE: address already in use :::5173` | Otra instancia de Vite corriendo | Cierre la terminal anterior o use `npm run dev -- --port 5174` |
| `npm ERR! Cannot find module ...` | Faltan dependencias | Borre la carpeta `node_modules` (deje `package-lock.json`) y ejecute `npm install` de nuevo |
| El frontend carga pero dice error 401/403 | Token o rol incorrecto | Vuelva a iniciar sesión; verifique el usuario y la tabla de permisos |
| El frontend carga pero `/api` da error de conexión | El backend no está corriendo | Arranque primero el backend (`iniciar-backend.bat`) y recargue |
| `error:0308010C:digital envelope routines::unsupported` (Node 17+) | Node demasiado antiguo para Vite | Use Node.js 18 o superior |
| El login funciona pero las llamadas fallan desde otra PC | CORS restringido a `localhost` | Ver nota en la sección 6 |

---

## 5. Estructura del proyecto

```
SistemaInventario/
├── backend/
│   ├── .config/dotnet-tools.json    Herramientas locales (dotnet-ef)
│   ├── src/
│   │   ├── SistemaInventario.Api/            Controladores, JWT, middleware, Program.cs
│   │   ├── SistemaInventario.Core/           Entidades, DTOs, contratos de servicios
│   │   ├── SistemaInventario.Infrastructure/ EF Core, migraciones, seed y servicios
│   │   └── SistemaInventario.Reporting/      Generación de PDF (QuestPDF) y Excel (ClosedXML)
│   └── SistemaInventario.sln
├── frontend/
│   ├── package.json                 Dependencias y scripts (dev/build/preview)
│   ├── vite.config.js               Puerto 5173 y proxy /api → :5080
│   └── src/
│       ├── api/        Cliente HTTP y módulos por recurso
│       ├── components/ Componentes UI reutilizables
│       ├── layout/     Sidebar, topbar y armazón de la aplicación
│       ├── pages/      Vistas (dashboard, productos, movimientos, reportes, …)
│       ├── router/     Rutas y guardas por rol
│       └── stores/     Pinia (sesión, notificaciones, almacenes, toasts)
├── iniciar-backend.bat
├── iniciar-frontend.bat
└── README.md
```

---

## 6. Ejecutar desde otra PC en la red local

Si otra PC necesita acceder a la API, hay tres ajustes:

1. **CORS** en `backend/src/SistemaInventario.Api/Program.cs` solo permite
   `http://localhost:5173`. Agregue el origen de la otra PC:

   ```csharp
   .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://192.168.1.100:5173")
   ```

2. **Vite** debe escuchar en todas las interfaces en `frontend/vite.config.js`:

   ```js
   server: { host: '0.0.0.0', port: 5173, proxy: { '/api': { target: 'http://192.168.1.10:5080', changeOrigin: true } } }
   ```

3. Abra los puertos **5080** y **5173** en el Firewall de Windows.

---

## 7. Funcionalidades

### Productos
Alta, modificación y baja lógica; código único, categoría, proveedor, precios de
compra/venta, unidad de medida, stock mínimo y stock desglosado por almacén.

### Control de inventario
Registro de entradas y salidas con actualización automática del stock, validación de
stock insuficiente, motivo, documento de referencia (factura/orden) y usuario responsable.
Se genera automáticamente una notificación cuando el stock iguala o baja del mínimo.

### Catálogos
CRUD de categorías, proveedores (con datos de contacto) y almacenes.

### Movimientos
Historial paginado con filtros por texto, tipo, almacén y rango de fechas; cada fila
muestra fecha, tipo, producto, almacén, cantidad, valor, stock resultante y usuario.

### Reportes con exportación
| Reporte | Formato |
|---|---|
| Stock actual | PDF / Excel |
| Stock crítico | PDF / Excel |
| Movimientos (filtros: producto, fechas, usuario, tipo) | PDF / Excel |
| Catálogo de productos | PDF / Excel |
| Categorías | PDF / Excel |
| Proveedores | PDF / Excel |
| Usuarios (solo admin) | PDF / Excel |

Los archivos se generan en el servidor con encabezado, filtros aplicados, totales y
formato listo para impresión/archivo contable.

### Seguridad
Autenticación JWT, contraseñas con PBKDF2 (SHA-256, 100.000 iteraciones + sal) y
autorización por políticas:

- `AdminOnly` → gestión de usuarios y exportación de usuarios.
- `ReadWrite` → altas y modificaciones de inventario (Admin y Usuario).
- `Authenticated` → consultas y exportaciones de reportes.

---

## 8. API resumen

```
POST   /api/auth/login                 GET /api/auth/me
GET|POST       /api/products           PUT|DELETE /api/products/{id}
GET|POST       /api/categories         PUT|DELETE /api/categories/{id}
GET|POST       /api/suppliers          PUT|DELETE /api/suppliers/{id}
GET|POST       /api/warehouses         PUT|DELETE /api/warehouses/{id}
GET|POST       /api/movements          GET /api/movements/{id}
GET    /api/dashboard
GET    /api/notifications              POST /api/notifications/{id}/read
GET|POST       /api/users              PUT|DELETE /api/users/{id}
GET    /api/reports/{report}/{format}  report = stock|critical-stock|movements|products|categories|suppliers|users
                                       format  = pdf|xlsx
```

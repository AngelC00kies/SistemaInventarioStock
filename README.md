# Sistema de Inventario

Sistema web para registrar, controlar y gestionar el ingreso, la salida y el stock de
productos en uno o varios almacenes.

- **Frontend:** Vue 3 + Vite + Tailwind CSS + Pinia + Vue Router + Chart.js (iconos Lucide)
- **Backend:** ASP.NET Core 8 Web API + Entity Framework Core + SQL Server + JWT
- **Reportes:** QuestPDF (PDF) y ClosedXML (Excel) generados en el servidor

---

## Requisitos

| Componente | Versión |
|---|---|
| .NET SDK | 8.0 o superior |
| Node.js | 18+ (probado con 22) |
| SQL Server | 2019/2022 (local o remoto) |

---

## Puesta en marcha

### 1. Base de datos

La cadena de conexión vive en
`backend/src/SistemaInventario.Api/appsettings.json`:

```json
"DefaultConnection": "Server=localhost;Database=SistemaInventarioDB;Trusted_Connection=True;TrustServerCertificate=True"
```

Ajuste el servidor si su instancia SQL Server usa otra autenticación (por ejemplo
`User Id=sa;Password=...`).

### 2. Backend (API)

```powershell
cd backend
dotnet restore
dotnet ef database update --project src/SistemaInventario.Infrastructure --startup-project src/SistemaInventario.Api
dotnet run --project src/SistemaInventario.Api
```

Al arrancar la API aplica las migraciones pendientes y carga los datos de ejemplo
(categorías, proveedores, almacenes, productos y movimientos históricos).

- API: `http://localhost:5080`
- Swagger (desarrollo): `http://localhost:5080/swagger`

### 3. Frontend (Vue)

```powershell
cd frontend
npm install
npm run dev
```

- Aplicación: `http://localhost:5173`
- El servidor de desarrollo proxea `/api` hacia `http://localhost:5080`
  (configurado en `frontend/vite.config.js`).

También puede usar los accesos directos `iniciar-backend.bat` y `iniciar-frontend.bat`
en la raíz del proyecto.

---

## Usuarios de demostración

| Usuario | Contraseña | Rol | Permisos |
|---|---|---|---|
| `admin` | `Admin123!` | Admin | Acceso total (usuarios, reportes, configuración) |
| `operador` | `Usuario123!` | Usuario | Gestión de productos, catálogos y movimientos |
| `auditor` | `Usuario123!` | Auditor | Solo lectura y exportación de reportes |

---

## Estructura del proyecto

```
SistemaInventario/
├── backend/
│   ├── src/
│   │   ├── SistemaInventario.Api/            Controladores, JWT, middleware, Program.cs
│   │   ├── SistemaInventario.Core/           Entidades, DTOs, contratos de servicios
│   │   ├── SistemaInventario.Infrastructure/ EF Core, migraciones, seed y servicios
│   │   └── SistemaInventario.Reporting/      Generación de PDF (QuestPDF) y Excel (ClosedXML)
│   └── SistemaInventario.sln
├── frontend/
│   └── src/
│       ├── api/        Cliente HTTP y módulos por recurso
│       ├── components/ Componentes UI reutilizables
│       ├── layout/     Sidebar, topbar y armazón de la aplicación
│       ├── pages/      Vistas (dashboard, productos, movimientos, reportes, …)
│       ├── router/     Rutas y guardas por rol
│       └── stores/     Pinia (sesión, notificaciones, almacenes, toasts)
└── README.md
```

---

## Funcionalidades

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

## API resumen

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

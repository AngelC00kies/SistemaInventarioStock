# Sistema de Inventario

Sistema completo para registrar, controlar y gestionar el ingreso, salida y stock de productos en uno o varios almacenes.

| Capa | Tecnología |
|------|------------|
| Frontend | Vue 3 + Vite + Pinia + Vue Router + Axios |
| Backend | .NET 8 (C#) + ASP.NET Core Web API |
| ORM | Entity Framework Core 8 (migraciones) |
| Base de datos | SQL Server |
| Autenticación | JWT + ASP.NET Core Identity |
| Validaciones | FluentValidation (backend) |
| Reportes | QuestPDF (PDF) + ClosedXML (Excel) |
| Documentación | Swagger / Swashbuckle |

## Estructura del monorepo

```
inventario/
├── backend/
│   ├── InventorySystem.sln
│   ├── src/
│   │   ├── InventorySystem.Domain/         # Entidades, enums, excepciones
│   │   ├── InventorySystem.Application/    # DTOs, interfaces, servicios, validadores
│   │   ├── InventorySystem.Infrastructure/ # DbContext, Identity, migraciones, seed
│   │   └── InventorySystem.API/            # Controladores, JWT, middleware, reportes
│   └── tests/
│       └── InventorySystem.Tests/          # Tests xUnit
├── database/
│   ├── InventarioDB_completo.sql           # Esquema + datos de ejemplo
│   └── migraciones_ef.sql                  # Migraciones de EF Core (idempotente)
└── frontend/                               # SPA Vue 3
```

## Requisitos

- .NET 8 SDK
- Node.js 18+ (probado con 22)
- SQL Server (LocalDB, instancia local o contenedor Docker)

## Puesta en marcha

### 1. Backend

**Opción A — creando la base con el script (recomendado para una instalación nueva):**

```powershell
sqlcmd -S TU_SERVIDOR -E -i database\InventarioDB_completo.sql
```

El script crea `InventoryDb` con las tablas, los índices, las claves
foráneas y los datos de ejemplo. La API al arrancar detecta que las migraciones
ya están aplicadas y solo valida que existan los datos.

**Opción B — dejando que EF Core cree la base:**

```powershell
cd backend
dotnet restore
dotnet build
dotnet run --project src/InventorySystem.API --urls http://localhost:5080
```

Al arrancar, la API crea/aplica las migraciones y siembra:

- Roles: `Admin`, `Usuario`, `Auditor`
- Usuarios de demostración: **admin / Admin123!** (Admin), **usuario / Usuario123!** (Usuario) y **auditor / Auditor123!** (Auditor)
- 2 almacenes, 3 categorías, 2 proveedores, 8 productos con stock y movimientos iniciales
- Alertas de stock bajo para productos que nacen por debajo de su mínimo

La cadena de conexión se configura en
`backend/src/InventorySystem.API/appsettings.json` (`ConnectionStrings:DefaultConnection`).
Swagger queda disponible en `http://localhost:5080/swagger`.

### 2. Frontend

```powershell
cd frontend
npm install
npm run dev
```

Aplicación en `http://localhost:5173` (proxy `/api` → `http://localhost:5080`, ver `vite.config.js`).

## Base de datos

| Tabla | Contenido |
|-------|-----------|
| `Productos` | Catálogo: `Codigo`, `Nombre`, `Descripcion`, `CategoriaId`, `ProveedorId`, `UnidadMedida`, `PrecioCompra`, `PrecioVenta`, `StockMinimo`, `Activo` |
| `Categorias` | Clasificación de productos (`Nombre`, `Descripcion`) |
| `Proveedores` | Origen de la mercancía (`Nombre`, `Contacto`, `Telefono`, `Email`, `Direccion`) |
| `Almacenes` | Depósitos físicos (`Codigo`, `Nombre`, `Direccion`) |
| `Existencias` | Stock actual por producto y almacén. PK compuesta `(ProductoId, AlmacenId)`, `Cantidad`, `FechaActualizacion` |
| `Movimientos` | Libro de entradas/salidas: `Fecha`, `Tipo`, `Motivo`, `Cantidad`, `StockResultante`, `DocumentoReferencia`, `ProductoId`, `AlmacenId`, `UsuarioId`, `NombreUsuario` |
| `AlertasStockBajo` | Alertas automáticas: `CantidadDetectada`, `StockMinimo`, `DetectadaEl`, `Resuelta`, `ResueltaEl` |
| `Usuarios` · `Roles` · `UsuariosRoles` · `UsuariosClaims` · `RolesClaims` · `UsuariosLogins` · `UsuariosTokens` | Usuarios, roles y permisos (`HashContrasena`, `NombreUsuario`, `NombreNormalizado`, `Activo`, `TokenRefresco`, …) |

Auditoría común en las tablas de catálogo: `Id`, `FechaCreacion`, `FechaActualizacion`, `Activo`.

Los scripts disponibles en `database/`:

- **`InventarioDB_completo.sql`** → crea la base desde cero con el esquema
  y los datos de ejemplo. Es el entregable para instalar el sistema en otra máquina.
- **`migraciones_ef.sql`** → script idempotente generado por EF Core, útil si prefieres
  gestionar el esquema con migraciones.

## Funcionalidades

### Productos
- CRUD completo: código único, nombre, descripción, categoría, proveedor, precios de
  compra/venta, unidad de medida y stock mínimo.
- Stock total consolidado y desglose por almacén.
- Baja lógica (nunca se borran registros con historial).

### Movimientos (corazón del sistema)
- Entradas y salidas con motivo y documento de referencia (factura, orden de compra…).
- **Actualización atómica del stock**: la salida ejecuta un `UPDATE` condicional
  (`WHERE cantidad >= salida`), por lo que el stock nunca queda negativo ante
  concurrencia.
- Cada movimiento guarda trazabilidad: fecha, tipo, cantidad, stock resultante,
  usuario responsable y documento asociado.
- Si `stock ≤ stock mínimo` se crea automáticamente una alerta; al reponer por encima
  del mínimo se resuelve sola.

### Notificaciones de stock bajo
- Listado pendientes/historial, contador en el sidebar (se refresca cada minuto)
  y acción "marcar atendida".

### Reportes
- Stock actual y stock crítico (filtros por categoría y almacén).
- Movimientos por producto, almacén, tipo, usuario y rango de fechas.
- Valorización del inventario (costo y precio de venta).
- Exportación a **Excel** y **PDF** desde el frontend.

### Usuarios y roles
| Rol | Permisos |
|-----|----------|
| `Admin` | Todo, incluido gestión de usuarios y borrados lógicos |
| `Usuario` | CRUD de productos/catálogos y registro de movimientos |
| `Auditor` | Solo lectura, consultas y reportes |

Autenticación con JWT (8 h) y *refresh token* (7 días); el cliente renueva el token
automáticamente ante un 401.

## API principal

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/auth/login` | Inicio de sesión |
| POST | `/api/auth/refresh` | Renueva el token |
| GET | `/api/dashboard` | KPIs, últimos movimientos y alertas |
| GET/POST/PUT/DELETE | `/api/products` | Productos |
| GET | `/api/products/{id}/stock` | Stock por almacén |
| POST | `/api/movements` | Registra entrada/salida y actualiza stock |
| GET/POST | `/api/categories`, `/api/suppliers`, `/api/warehouses` | Catálogos |
| GET | `/api/notifications` · `/api/notifications/pending-count` | Alertas |
| GET | `/api/reports/stock?format=json\|excel\|pdf` | Reporte de stock |
| GET | `/api/reports/movements?format=…` | Reporte de movimientos |
| GET | `/api/reports/inventory-value` | Valorización |
| GET/POST/PUT | `/api/users` | Usuarios (solo Admin) |

Las respuestas paginadas usan `{ items, totalCount, page, pageSize, totalPages }`.

## Tests

```powershell
cd backend
dotnet test
```

Cubren validadores, reglas de negocio de movimientos (sin stock negativo), creación y
resolución de alertas, y control de acceso por rol.

## Arquitectura por capas y principios SOLID

```
InventorySystem.API  ──►  Application  ──►  Domain
        │                     ▲
        └──► Infrastructure ───┘   (implementa las interfaces de Application)
```

| Principio | Dónde se aplica |
|-----------|-----------------|
| **S**ingle responsibility | Cada servicio hace una cosa (`MovementService`, `ProductService`, `ReportService`, `NotificationService`, `DashboardService`); `NotificationService`, `DashboardService` y `ReportService` viven cada uno en su propio archivo. |
| **O**pen/closed | **Frontend:** `src/config/navigation.js` es el registro único de pantallas (ruta, componente, roles, sección del menú y título); el router y el menú lateral se generan a partir de él. Añadir una pantalla o un rol no requiere tocar `router/index.js` ni `MainLayout.vue`. **Backend:** cada formato de exportación es una clase `IReportExporter` (JSON/Excel/PDF) registrada en DI: añadir un formato nuevo es crear una clase y una línea de registro. |
| **L**iskov | `ApplicationDbContext` se sustituye por una versión SQLite en memoria en los tests sin cambiar el comportamiento de los servicios. |
| **I**nterface segregation | Interfaces pequeñas y específicas: `IApplicationDbContext`, `ICurrentUserService`, `IInventoryServices`, `ICatalogServices`, `IIdentityServices`, `IReportExporter`. |
| **D**ependency inversion | Application depende de abstracciones, nunca del `DbContext` concreto. Los servicios HTTP (`ValidationActionFilter`, exportadores, `CurrentUserService`) se registran en `Extensions/ServiceCollectionExtensions.cs`. |

`Program.cs` queda reducido a la composición del contenedor; el detalle vive en
`Extensions/ServiceCollectionExtensions.cs` (servicios) y
`Extensions/ApplicationBuilderExtensions.cs` (pipeline y base de datos).

## Notas de implementación

- Enums (`MovementType`, `UnitOfMeasure`) se persisten como texto para legibilidad en SQL.
- `Stock` usa clave compuesta `(ProductId, WarehouseId)`.
- Los movimientos denormalizan `UserName` para conservar la auditoría histórica.
- Middleware global de excepciones: `AppException` → HTTP 400/401/403/404 con `{ message }`,
  `ValidationException` → 400 con `errors`.
- `ValidationActionFilter` ejecuta los validadores FluentValidation de cada petición antes
  del controlador y devuelve `400 { message, errors: { campo: [ mensajes ] } }`.
- CORS habilitado para `http://localhost:5173` (configurable en `Cors:Origins`).

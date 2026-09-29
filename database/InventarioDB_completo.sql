/* =============================================================================
   SISTEMA DE INVENTARIO - SCRIPT COMPLETO DE BASE DE DATOS
   Motor: SQL Server
   Base:  InventoryDb  (la misma que usa appsettings.json)

   Contenido:
     1. Creación de la base de datos
     2. Tablas con nombres y columnas en español
     3. Índices (únicos y de búsqueda)
     4. Claves foráneas
     5. Datos de ejemplo (roles, usuario admin, catálogos, productos,
        stock, movimientos y alertas)
     6. Historial de migraciones de EF Core (para que la API no vuelva
        a crear el esquema al arrancar)

   Ejecución: sqlcmd -S TU_SERVIDOR -E -i InventarioDB_completo.sql
              (o desde SSMS, botón "Ejecutar")

   Credenciales de demostración:
     admin   / Admin123!   (Admin: acceso total)
     usuario / Usuario123! (Usuario: CRUD sin administración)
     auditor / Auditor123! (Auditor: solo lectura)
   ============================================================================= */

SET NOCOUNT ON;
-- Los índices con filtro (p. ej. UserNameIndex) exigen QUOTED_IDENTIFIER ON.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ─────────────────────────── 1. BASE DE DATOS ─────────────────────────── */
-- ATENCIÓN: borra la base existente. Comenta este bloque si quieres conservarla.
IF DB_ID(N'InventoryDb') IS NOT NULL
BEGIN
    ALTER DATABASE [InventoryDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [InventoryDb];
END;
GO

CREATE DATABASE [InventoryDb];
GO

USE [InventoryDb];
GO

-- Historial de migraciones de EF Core: sin él, la API intentaría recrear el
-- esquema al arrancar. Se crea aquí y se rellena en el paso 7.
CREATE TABLE [__EFMigrationsHistory]
(
    [MigrationId]    NVARCHAR (150) NOT NULL,
    [ProductVersion] NVARCHAR (32)  NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
);
GO

/* ───────────────────── 2. TABLAS DE IDENTIDAD (USUARIOS) ───────────────── */

CREATE TABLE [Roles]
(
    [Id]                NVARCHAR (450) NOT NULL,
    [Nombre]            NVARCHAR (256) NULL,
    [NombreNormalizado] NVARCHAR (256) NULL,
    [MarcaConcurrencia] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Usuarios]
(
    [Id]                       NVARCHAR (450)  NOT NULL,
    [NombreCompleto]           NVARCHAR (MAX)  NOT NULL,
    [Activo]                   BIT             NOT NULL,
    [TokenRefresco]            NVARCHAR (MAX)  NULL,
    [TokenRefrescoVencimiento] DATETIME2       NULL,
    [NombreUsuario]            NVARCHAR (256)  NULL,
    [NombreUsuarioNormalizado] NVARCHAR (256)  NULL,
    [Email]                    NVARCHAR (256)  NULL,
    [EmailNormalizado]         NVARCHAR (256)  NULL,
    [EmailConfirmado]          BIT             NOT NULL,
    [HashContrasena]           NVARCHAR (MAX)  NULL,
    [MarcaSeguridad]           NVARCHAR (MAX)  NULL,
    [MarcaConcurrencia]        NVARCHAR (MAX)  NULL,
    [Telefono]                 NVARCHAR (MAX)  NULL,
    [TelefonoConfirmado]       BIT             NOT NULL,
    [DosFactoresHabilitado]    BIT             NOT NULL,
    [BloqueoHasta]             DATETIMEOFFSET  NULL,
    [BloqueoHabilitado]        BIT             NOT NULL,
    [IntentosFallidos]         INT             NOT NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [RolesClaims]
(
    [Id]               INT IDENTITY (1, 1) NOT NULL,
    [RolId]            NVARCHAR (450) NOT NULL,
    [TipoReclamacion]  NVARCHAR (MAX) NULL,
    [ValorReclamacion] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_RolesClaims] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [UsuariosClaims]
(
    [Id]               INT IDENTITY (1, 1) NOT NULL,
    [UsuarioId]        NVARCHAR (450) NOT NULL,
    [TipoReclamacion]  NVARCHAR (MAX) NULL,
    [ValorReclamacion] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_UsuariosClaims] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [UsuariosLogins]
(
    [ProveedorLogin]   NVARCHAR (450) NOT NULL,
    [ClaveProveedor]   NVARCHAR (450) NOT NULL,
    [NombreProveedor]  NVARCHAR (MAX) NULL,
    [UsuarioId]        NVARCHAR (450) NOT NULL,
    CONSTRAINT [PK_UsuariosLogins] PRIMARY KEY ([ProveedorLogin], [ClaveProveedor])
);
GO

CREATE TABLE [UsuariosRoles]
(
    [UsuarioId] NVARCHAR (450) NOT NULL,
    [RolId]     NVARCHAR (450) NOT NULL,
    CONSTRAINT [PK_UsuariosRoles] PRIMARY KEY ([UsuarioId], [RolId])
);
GO

CREATE TABLE [UsuariosTokens]
(
    [UsuarioId]      NVARCHAR (450) NOT NULL,
    [ProveedorLogin] NVARCHAR (450) NOT NULL,
    [Nombre]         NVARCHAR (450) NOT NULL,
    [Valor]          NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_UsuariosTokens] PRIMARY KEY ([UsuarioId], [ProveedorLogin], [Nombre])
);
GO

/* ───────────────────── 3. TABLAS DE NEGOCIO ────────────────────────────── */

CREATE TABLE [Categorias]
(
    [Id]                 INT IDENTITY (1, 1) NOT NULL,
    [Nombre]             NVARCHAR (450) NOT NULL,
    [Descripcion]        NVARCHAR (MAX) NULL,
    [FechaCreacion]      DATETIME2 NOT NULL,
    [FechaActualizacion] DATETIME2 NULL,
    [Activo]             BIT NOT NULL,
    CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Proveedores]
(
    [Id]                 INT IDENTITY (1, 1) NOT NULL,
    [Nombre]             NVARCHAR (450) NOT NULL,
    [Contacto]           NVARCHAR (MAX) NULL,
    [Telefono]           NVARCHAR (MAX) NULL,
    [Email]              NVARCHAR (MAX) NULL,
    [Direccion]          NVARCHAR (MAX) NULL,
    [FechaCreacion]      DATETIME2 NOT NULL,
    [FechaActualizacion] DATETIME2 NULL,
    [Activo]             BIT NOT NULL,
    CONSTRAINT [PK_Proveedores] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Almacenes]
(
    [Id]                 INT IDENTITY (1, 1) NOT NULL,
    [Codigo]             NVARCHAR (450) NOT NULL,
    [Nombre]             NVARCHAR (MAX) NOT NULL,
    [Direccion]          NVARCHAR (MAX) NULL,
    [FechaCreacion]      DATETIME2 NOT NULL,
    [FechaActualizacion] DATETIME2 NULL,
    [Activo]             BIT NOT NULL,
    CONSTRAINT [PK_Almacenes] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Productos]
(
    [Id]                 INT IDENTITY (1, 1) NOT NULL,
    [Codigo]             NVARCHAR (450) NOT NULL,
    [Nombre]             NVARCHAR (MAX) NOT NULL,
    [Descripcion]        NVARCHAR (MAX) NULL,
    [CategoriaId]        INT NOT NULL,
    [ProveedorId]        INT NULL,
    [UnidadMedida]       NVARCHAR (30) NOT NULL,
    [PrecioCompra]       DECIMAL (18, 2) NOT NULL,
    [PrecioVenta]        DECIMAL (18, 2) NOT NULL,
    [StockMinimo]        INT NOT NULL,
    [FechaCreacion]      DATETIME2 NOT NULL,
    [FechaActualizacion] DATETIME2 NULL,
    [Activo]             BIT NOT NULL,
    CONSTRAINT [PK_Productos] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AlertasStockBajo]
(
    [Id]                 INT IDENTITY (1, 1) NOT NULL,
    [ProductoId]         INT NOT NULL,
    [AlmacenId]          INT NOT NULL,
    [CantidadDetectada]  INT NOT NULL,
    [StockMinimo]        INT NOT NULL,
    [DetectadaEl]        DATETIME2 NOT NULL,
    [Resuelta]           BIT NOT NULL,
    [ResueltaEl]         DATETIME2 NULL,
    [FechaCreacion]      DATETIME2 NOT NULL,
    [FechaActualizacion] DATETIME2 NULL,
    [Activo]             BIT NOT NULL,
    CONSTRAINT [PK_AlertasStockBajo] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Movimientos]
(
    [Id]                  INT IDENTITY (1, 1) NOT NULL,
    [Fecha]               DATETIME2 NOT NULL,
    [Tipo]                NVARCHAR (20) NOT NULL,
    [Motivo]              NVARCHAR (MAX) NOT NULL,
    [Cantidad]            INT NOT NULL,
    [StockResultante]     INT NOT NULL,
    [DocumentoReferencia] NVARCHAR (MAX) NULL,
    [ProductoId]          INT NOT NULL,
    [AlmacenId]           INT NOT NULL,
    [UsuarioId]           NVARCHAR (450) NOT NULL,
    [NombreUsuario]       NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_Movimientos] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Existencias]
(
    [ProductoId]         INT NOT NULL,
    [AlmacenId]          INT NOT NULL,
    [Cantidad]           INT NOT NULL,
    [FechaActualizacion] DATETIME2 NOT NULL,
    CONSTRAINT [PK_Existencias] PRIMARY KEY ([ProductoId], [AlmacenId])
);
GO

/* ───────────────────────────── 4. ÍNDICES ──────────────────────────────── */

CREATE UNIQUE INDEX [UserNameIndex]
    ON [Usuarios] ([NombreUsuarioNormalizado])
    WHERE [NombreUsuarioNormalizado] IS NOT NULL;
GO

CREATE INDEX [EmailIndex] ON [Usuarios] ([EmailNormalizado]);
GO

CREATE UNIQUE INDEX [RoleNameIndex]
    ON [Roles] ([NombreNormalizado])
    WHERE [NombreNormalizado] IS NOT NULL;
GO

CREATE INDEX [IX_RolesClaims_RolId] ON [RolesClaims] ([RolId]);
GO

CREATE INDEX [IX_UsuariosClaims_UsuarioId] ON [UsuariosClaims] ([UsuarioId]);
GO

CREATE INDEX [IX_UsuariosLogins_UsuarioId] ON [UsuariosLogins] ([UsuarioId]);
GO

CREATE INDEX [IX_UsuariosRoles_RolId] ON [UsuariosRoles] ([RolId]);
GO

CREATE UNIQUE INDEX [IX_Categorias_Nombre] ON [Categorias] ([Nombre]);
GO

CREATE UNIQUE INDEX [IX_Proveedores_Nombre] ON [Proveedores] ([Nombre]);
GO

CREATE UNIQUE INDEX [IX_Almacenes_Codigo] ON [Almacenes] ([Codigo]);
GO

CREATE INDEX [IX_Productos_CategoriaId] ON [Productos] ([CategoriaId]);
GO

CREATE UNIQUE INDEX [IX_Productos_Codigo] ON [Productos] ([Codigo]);
GO

CREATE INDEX [IX_Productos_ProveedorId] ON [Productos] ([ProveedorId]);
GO

CREATE INDEX [IX_AlertasStockBajo_ProductoId_AlmacenId_Resuelta]
    ON [AlertasStockBajo] ([ProductoId], [AlmacenId], [Resuelta]);
GO

CREATE INDEX [IX_AlertasStockBajo_AlmacenId] ON [AlertasStockBajo] ([AlmacenId]);
GO

CREATE INDEX [IX_Movimientos_Fecha] ON [Movimientos] ([Fecha]);
GO

CREATE INDEX [IX_Movimientos_ProductoId_Fecha] ON [Movimientos] ([ProductoId], [Fecha]);
GO

CREATE INDEX [IX_Movimientos_UsuarioId] ON [Movimientos] ([UsuarioId]);
GO

CREATE INDEX [IX_Movimientos_AlmacenId] ON [Movimientos] ([AlmacenId]);
GO

CREATE INDEX [IX_Existencias_AlmacenId] ON [Existencias] ([AlmacenId]);
GO

/* ─────────────────────── 5. CLAVES FORÁNEAS ────────────────────────────── */

ALTER TABLE [RolesClaims] ADD CONSTRAINT [FK_RolesClaims_Roles_RolId]
    FOREIGN KEY ([RolId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [UsuariosClaims] ADD CONSTRAINT [FK_UsuariosClaims_Usuarios_UsuarioId]
    FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [UsuariosLogins] ADD CONSTRAINT [FK_UsuariosLogins_Usuarios_UsuarioId]
    FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [UsuariosRoles] ADD CONSTRAINT [FK_UsuariosRoles_Roles_RolId]
    FOREIGN KEY ([RolId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [UsuariosRoles] ADD CONSTRAINT [FK_UsuariosRoles_Usuarios_UsuarioId]
    FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [UsuariosTokens] ADD CONSTRAINT [FK_UsuariosTokens_Usuarios_UsuarioId]
    FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [Productos] ADD CONSTRAINT [FK_Productos_Categorias_CategoriaId]
    FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Productos] ADD CONSTRAINT [FK_Productos_Proveedores_ProveedorId]
    FOREIGN KEY ([ProveedorId]) REFERENCES [Proveedores] ([Id]) ON DELETE SET NULL;
GO

ALTER TABLE [AlertasStockBajo] ADD CONSTRAINT [FK_AlertasStockBajo_Productos_ProductoId]
    FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [AlertasStockBajo] ADD CONSTRAINT [FK_AlertasStockBajo_Almacenes_AlmacenId]
    FOREIGN KEY ([AlmacenId]) REFERENCES [Almacenes] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [Movimientos] ADD CONSTRAINT [FK_Movimientos_Productos_ProductoId]
    FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Movimientos] ADD CONSTRAINT [FK_Movimientos_Almacenes_AlmacenId]
    FOREIGN KEY ([AlmacenId]) REFERENCES [Almacenes] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Existencias] ADD CONSTRAINT [FK_Existencias_Productos_ProductoId]
    FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [Existencias] ADD CONSTRAINT [FK_Existencias_Almacenes_AlmacenId]
    FOREIGN KEY ([AlmacenId]) REFERENCES [Almacenes] ([Id]) ON DELETE CASCADE;
GO

/* ────────────────────────── 6. DATOS DE EJEMPLO ────────────────────────── */

-- ── Roles ──
INSERT INTO [Roles] ([Id], [Nombre], [NombreNormalizado], [MarcaConcurrencia]) VALUES
    ('rol-0000-0000-0000-000000000001', N'Admin',   N'ADMIN',   N'8f1c2d3e-0001-4a1b-9c2d-111111111111'),
    ('rol-0000-0000-0000-000000000002', N'Usuario', N'USUARIO', N'8f1c2d3e-0002-4a1b-9c2d-222222222222'),
    ('rol-0000-0000-0000-000000000003', N'Auditor', N'AUDITOR', N'8f1c2d3e-0003-4a1b-9c2d-333333333333');
GO

-- ── Usuarios de demostración (un rol por usuario) ──
-- admin   / Admin123!  → Admin (acceso total)
-- usuario / Usuario123! → Usuario (CRUD, sin administración)
-- auditor / Auditor123! → Auditor (solo lectura)
INSERT INTO [Usuarios]
(
    [Id], [NombreCompleto], [Activo], [TokenRefresco], [TokenRefrescoVencimiento],
    [NombreUsuario], [NombreUsuarioNormalizado], [Email], [EmailNormalizado],
    [EmailConfirmado], [HashContrasena], [MarcaSeguridad], [MarcaConcurrencia],
    [Telefono], [TelefonoConfirmado], [DosFactoresHabilitado], [BloqueoHasta],
    [BloqueoHabilitado], [IntentosFallidos]
)
VALUES
(
    'usr-0000-0000-0000-000000000001', N'Administrador', 1, NULL, NULL,
    N'admin', N'ADMIN', N'admin@inventario.local', N'ADMIN@INVENTARIO.LOCAL',
    1,
    N'AQAAAAIAAYagAAAAEB/63G0Wcqlj33PN3X30pnaU1k+rfnZWn4pZYQgXvGksuLVlpbZxdd0wl1XwJ/5iYQ==',
    N'XJ45SMLVN73R2S6GGS3ABFXHNLXZ2KOD', N'3a70d290-989c-4271-94cc-56e5597ea9e0',
    NULL, 0, 0, NULL, 0, 0
),
(
    'usr-0000-0000-0000-000000000002', N'Usuario Demo', 1, NULL, NULL,
    N'usuario', N'USUARIO', N'usuario@inventario.local', N'USUARIO@INVENTARIO.LOCAL',
    1,
    N'AQAAAAIAAYagAAAAEINzKfV0MGLeGZ4O4c79HeiqFJ060USG3Fb0ke1LG6qNvP3xvhqQ7U/x3ZZ+3+n+Bg==',
    N'6PBL7WHMEJ3MB4W7L26F53JQ2XLULEVJ', N'8d8b0f4d-8d35-451a-92c1-9ebc6ba1bb7b',
    NULL, 0, 0, NULL, 0, 0
),
(
    'usr-0000-0000-0000-000000000003', N'Auditor Demo', 1, NULL, NULL,
    N'auditor', N'AUDITOR', N'auditor@inventario.local', N'AUDITOR@INVENTARIO.LOCAL',
    1,
    N'AQAAAAIAAYagAAAAEPxh0KGIJBstFsu4zA5Swj4YH8wKqEulLna4kf9kTxZQLke//qWHVlNnCUuNq1/Pjg==',
    N'4HLOXA5Y4RZO7YTNVZFJOJAUSJMYUCIG', N'3381677f-0e01-42d0-92f0-59cbf144fd36',
    NULL, 0, 0, NULL, 0, 0
);
GO

INSERT INTO [UsuariosRoles] ([UsuarioId], [RolId]) VALUES
    ('usr-0000-0000-0000-000000000001', 'rol-0000-0000-0000-000000000001'),
    ('usr-0000-0000-0000-000000000002', 'rol-0000-0000-0000-000000000002'),
    ('usr-0000-0000-0000-000000000003', 'rol-0000-0000-0000-000000000003');
GO

-- ── Almacenes ──
SET IDENTITY_INSERT [Almacenes] ON;

INSERT INTO [Almacenes] ([Id], [Codigo], [Nombre], [Direccion], [FechaCreacion], [FechaActualizacion], [Activo]) VALUES
    (1, N'ALM-01', N'Almacén Central',        N'Av. Principal 100', DATEADD(DAY, -7, SYSUTCDATETIME()), NULL, 1),
    (2, N'ALM-02', N'Almacén Sucursal Norte', N'Calle Norte 45',    DATEADD(DAY, -7, SYSUTCDATETIME()), NULL, 1);
GO

SET IDENTITY_INSERT [Almacenes] OFF;
GO

-- ── Categorías ──
SET IDENTITY_INSERT [Categorias] ON;

INSERT INTO [Categorias] ([Id], [Nombre], [Descripcion], [FechaCreacion], [FechaActualizacion], [Activo]) VALUES
    (1, N'Electrónica', N'Dispositivos y accesorios electrónicos', DATEADD(DAY, -7, SYSUTCDATETIME()), NULL, 1),
    (2, N'Oficina',     N'Materiales de oficina',                  DATEADD(DAY, -7, SYSUTCDATETIME()), NULL, 1),
    (3, N'Limpieza',    N'Insumos de limpieza',                    DATEADD(DAY, -7, SYSUTCDATETIME()), NULL, 1);
GO

SET IDENTITY_INSERT [Categorias] OFF;
GO

-- ── Proveedores ──
SET IDENTITY_INSERT [Proveedores] ON;

INSERT INTO [Proveedores]
    ([Id], [Nombre], [Contacto], [Telefono], [Email], [Direccion], [FechaCreacion], [FechaActualizacion], [Activo])
VALUES
    (1, N'TecnoDistribuidora S.A.', N'María Gómez', N'+54 11 5555-0001', N'ventas@tecno.com',   N'Industrial 230', DATEADD(DAY, -7, SYSUTCDATETIME()), NULL, 1),
    (2, N'Ofimax',                  N'Juan Pérez',  N'+54 11 5555-0002', N'contacto@ofimax.com', N'Comercio 45',    DATEADD(DAY, -7, SYSUTCDATETIME()), NULL, 1);
GO

SET IDENTITY_INSERT [Proveedores] OFF;
GO

-- ── Productos ──
SET IDENTITY_INSERT [Productos] ON;

INSERT INTO [Productos]
    ([Id], [Codigo], [Nombre], [Descripcion], [CategoriaId], [ProveedorId], [UnidadMedida],
     [PrecioCompra], [PrecioVenta], [StockMinimo], [FechaCreacion], [FechaActualizacion], [Activo])
VALUES
    (1, N'P-0001', N'Teclado Mecánico',        N'Teclado mecánico RGB',        1, 1, N'Unidad',  25000.00, 39999.00, 10, DATEADD(DAY, -7, SYSUTCDATETIME()), NULL, 1),
    (2, N'P-0002', N'Mouse Inalámbrico',       N'Mouse óptico inalámbrico',    1, 1, N'Unidad',  12000.00, 19999.00, 15, DATEADD(DAY, -7, SYSUTCDATETIME()), NULL, 1),
    (3, N'P-0003', N'Monitor 24"',             N'Monitor LED 24 pulgadas',     1, 1, N'Unidad', 180000.00, 249999.00, 5, DATEADD(DAY, -7, SYSUTCDATETIME()), NULL, 1),
    (4, N'P-0004', N'Resma A4',                N'Resma de papel A4 75g',       2, 2, N'Unidad',   4500.00,  6999.00, 20, DATEADD(DAY, -7, SYSUTCDATETIME()), NULL, 1),
    (5, N'P-0005', N'Tóner HP 85A',            N'Tóner negro compatible',      2, 2, N'Unidad',  32000.00, 47999.00,  8, DATEADD(DAY, -7, SYSUTCDATETIME()), NULL, 1),
    (6, N'P-0006', N'Detergente 5L',           N'Detergente multiuso',         3, NULL, N'Litro',      8000.00, 12999.00, 12, DATEADD(DAY, -7, SYSUTCDATETIME()), NULL, 1),
    (7, N'P-0007', N'Guantes de Nitrilo (x100)', N'Guantes descartables',      3, NULL, N'Paquete',    9500.00, 15999.00, 10, DATEADD(DAY, -7, SYSUTCDATETIME()), NULL, 1),
    (8, N'P-0008', N'Auriculares USB',         N'Auriculares con micrófono',   1, 1, N'Unidad',  15000.00, 25999.00,  6, DATEADD(DAY, -7, SYSUTCDATETIME()), NULL, 1);
GO

SET IDENTITY_INSERT [Productos] OFF;
GO

-- ── Stock inicial (existencias por almacén) ──
INSERT INTO [Existencias] ([ProductoId], [AlmacenId], [Cantidad], [FechaActualizacion]) VALUES
    (1, 1,  25, SYSUTCDATETIME()),
    (2, 1,  40, SYSUTCDATETIME()),
    (3, 1,   8, SYSUTCDATETIME()),
    (4, 1, 120, SYSUTCDATETIME()),
    (5, 1,   6, SYSUTCDATETIME()),
    (6, 1,  30, SYSUTCDATETIME()),
    (7, 1,   5, SYSUTCDATETIME()),
    (8, 2,  10, SYSUTCDATETIME()),
    (1, 2,   5, SYSUTCDATETIME()),
    (4, 2,  45, SYSUTCDATETIME());
GO

-- ── Movimientos de carga inicial (entrada) ──
SET IDENTITY_INSERT [Movimientos] ON;

INSERT INTO [Movimientos]
    ([Id], [Fecha], [Tipo], [Motivo], [Cantidad], [StockResultante], [DocumentoReferencia],
     [ProductoId], [AlmacenId], [UsuarioId], [NombreUsuario])
VALUES
    (1,  DATEADD(DAY, -7, SYSUTCDATETIME()), N'Entrada', N'Carga inicial de inventario',  25,  25, NULL, 1, 1, 'usr-0000-0000-0000-000000000001', N'admin'),
    (2,  DATEADD(DAY, -7, SYSUTCDATETIME()), N'Entrada', N'Carga inicial de inventario',  40,  40, NULL, 2, 1, 'usr-0000-0000-0000-000000000001', N'admin'),
    (3,  DATEADD(DAY, -7, SYSUTCDATETIME()), N'Entrada', N'Carga inicial de inventario',   8,   8, NULL, 3, 1, 'usr-0000-0000-0000-000000000001', N'admin'),
    (4,  DATEADD(DAY, -7, SYSUTCDATETIME()), N'Entrada', N'Carga inicial de inventario', 120, 120, NULL, 4, 1, 'usr-0000-0000-0000-000000000001', N'admin'),
    (5,  DATEADD(DAY, -7, SYSUTCDATETIME()), N'Entrada', N'Carga inicial de inventario',   6,   6, NULL, 5, 1, 'usr-0000-0000-0000-000000000001', N'admin'),
    (6,  DATEADD(DAY, -7, SYSUTCDATETIME()), N'Entrada', N'Carga inicial de inventario',  30,  30, NULL, 6, 1, 'usr-0000-0000-0000-000000000001', N'admin'),
    (7,  DATEADD(DAY, -7, SYSUTCDATETIME()), N'Entrada', N'Carga inicial de inventario',   5,   5, NULL, 7, 1, 'usr-0000-0000-0000-000000000001', N'admin'),
    (8,  DATEADD(DAY, -7, SYSUTCDATETIME()), N'Entrada', N'Carga inicial de inventario',  10,  10, NULL, 8, 2, 'usr-0000-0000-0000-000000000001', N'admin'),
    (9,  DATEADD(DAY, -7, SYSUTCDATETIME()), N'Entrada', N'Carga inicial de inventario',   5,   5, NULL, 1, 2, 'usr-0000-0000-0000-000000000001', N'admin'),
    (10, DATEADD(DAY, -7, SYSUTCDATETIME()), N'Entrada', N'Carga inicial de inventario',  45,  45, NULL, 4, 2, 'usr-0000-0000-0000-000000000001', N'admin');
GO

SET IDENTITY_INSERT [Movimientos] OFF;
GO

-- ── Alertas de stock bajo (productos que nacen por debajo del mínimo) ──
SET IDENTITY_INSERT [AlertasStockBajo] ON;

INSERT INTO [AlertasStockBajo]
    ([Id], [ProductoId], [AlmacenId], [CantidadDetectada], [StockMinimo], [DetectadaEl],
     [Resuelta], [ResueltaEl], [FechaCreacion], [FechaActualizacion], [Activo])
VALUES
    (1, 5, 1, 6,  8, DATEADD(DAY, -2, SYSUTCDATETIME()), 0, NULL, DATEADD(DAY, -2, SYSUTCDATETIME()), NULL, 1),
    (2, 7, 1, 5, 10, DATEADD(DAY, -2, SYSUTCDATETIME()), 0, NULL, DATEADD(DAY, -2, SYSUTCDATETIME()), NULL, 1);
GO

SET IDENTITY_INSERT [AlertasStockBajo] OFF;
GO

/* ─────────────── 7. HISTORIAL DE MIGRACIONES DE EF CORE ────────────────── */
-- Sin estas filas, la API intentaría volver a crear el esquema al arrancar.

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES
    (N'20260923164849_InitialCreate',           N'8.0.26'),
    (N'20260929121717_RenombrarTablasAlEspanol', N'8.0.26');
GO

PRINT N'Base de datos InventoryDb creada correctamente con datos de ejemplo.';
GO

-- ---------------------------------------------------------------------
-- Roles
-- ---------------------------------------------------------------------
CREATE TABLE Roles (
    Id          INT IDENTITY (1, 1) NOT NULL,
    Nombre      NVARCHAR (50)       NOT NULL,
    Descripcion NVARCHAR (MAX)      NOT NULL,
    CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED (Id)
);

CREATE UNIQUE INDEX IX_Roles_Nombre ON Roles (Nombre);

-- ---------------------------------------------------------------------
-- Usuarios
-- ---------------------------------------------------------------------
CREATE TABLE Usuarios (
    Id             INT IDENTITY (1, 1) NOT NULL,
    NombreUsuario  NVARCHAR (60)       NOT NULL,
    ContrasenaHash NVARCHAR (200)      NOT NULL,
    NombreCompleto NVARCHAR (150)      NOT NULL,
    Correo         NVARCHAR (150)      NULL,
    RolId          INT                 NOT NULL,
    Activo         BIT                 NOT NULL,
    FechaCreacion  DATETIME2           NOT NULL,
    CONSTRAINT PK_Usuarios PRIMARY KEY CLUSTERED (Id)
);

CREATE UNIQUE INDEX IX_Usuarios_NombreUsuario ON Usuarios (NombreUsuario);

CREATE INDEX IX_Usuarios_RolId ON Usuarios (RolId);

-- ---------------------------------------------------------------------
-- Categorias
-- ---------------------------------------------------------------------
CREATE TABLE Categorias (
    Id          INT IDENTITY (1, 1) NOT NULL,
    Nombre      NVARCHAR (100)       NOT NULL,
    Descripcion NVARCHAR (400)       NULL,
    Activo      BIT                  NOT NULL,
    CONSTRAINT PK_Categorias PRIMARY KEY CLUSTERED (Id)
);

CREATE UNIQUE INDEX IX_Categorias_Nombre ON Categorias (Nombre);

-- ---------------------------------------------------------------------
-- Proveedores
-- ---------------------------------------------------------------------
CREATE TABLE Proveedores (
    Id            INT IDENTITY (1, 1) NOT NULL,
    Nombre        NVARCHAR (150)       NOT NULL,
    NombreContacto NVARCHAR (150)      NULL,
    Telefono      NVARCHAR (50)        NULL,
    Correo        NVARCHAR (150)       NULL,
    Direccion     NVARCHAR (300)       NULL,
    Activo        BIT                  NOT NULL,
    CONSTRAINT PK_Proveedores PRIMARY KEY CLUSTERED (Id)
);

CREATE UNIQUE INDEX IX_Proveedores_Nombre ON Proveedores (Nombre);

-- ---------------------------------------------------------------------
-- Almacenes
-- ---------------------------------------------------------------------
CREATE TABLE Almacenes (
    Id        INT IDENTITY (1, 1) NOT NULL,
    Nombre    NVARCHAR (120)       NOT NULL,
    Codigo    NVARCHAR (30)        NOT NULL,
    Ubicacion NVARCHAR (200)       NULL,
    Activo    BIT                  NOT NULL,
    CONSTRAINT PK_Almacenes PRIMARY KEY CLUSTERED (Id)
);

CREATE UNIQUE INDEX IX_Almacenes_Codigo ON Almacenes (Codigo);

-- ---------------------------------------------------------------------
-- Productos
-- ---------------------------------------------------------------------
CREATE TABLE Productos (
    Id            INT IDENTITY (1, 1) NOT NULL,
    Codigo        NVARCHAR (40)       NOT NULL,
    Nombre        NVARCHAR (200)      NOT NULL,
    Descripcion   NVARCHAR (800)      NULL,
    CategoriaId   INT                 NOT NULL,
    ProveedorId   INT                 NULL,
    PrecioCompra  DECIMAL (18, 2)     NOT NULL,
    PrecioVenta   DECIMAL (18, 2)     NOT NULL,
    Unidad        NVARCHAR (30)       NOT NULL,
    StockMinimo   INT                 NOT NULL,
    Activo        BIT                 NOT NULL,
    FechaCreacion DATETIME2           NOT NULL,
    CONSTRAINT PK_Productos PRIMARY KEY CLUSTERED (Id)
);

CREATE UNIQUE INDEX IX_Productos_Codigo ON Productos (Codigo);

CREATE INDEX IX_Productos_CategoriaId ON Productos (CategoriaId);

CREATE INDEX IX_Productos_ProveedorId ON Productos (ProveedorId);

-- ---------------------------------------------------------------------
-- NivelesStock (stock por producto y almacén)
-- ---------------------------------------------------------------------
CREATE TABLE NivelesStock (
    ProductoId INT NOT NULL,
    AlmacenId  INT NOT NULL,
    Cantidad   INT NOT NULL,
    CONSTRAINT PK_NivelesStock PRIMARY KEY CLUSTERED (ProductoId, AlmacenId)
);

CREATE INDEX IX_NivelesStock_AlmacenId ON NivelesStock (AlmacenId);

-- ---------------------------------------------------------------------
-- Movimientos (entradas y salidas de inventario)
-- ---------------------------------------------------------------------
CREATE TABLE Movimientos (
    Id                  INT IDENTITY (1, 1) NOT NULL,
    Fecha               DATETIME2           NOT NULL,
    Tipo                NVARCHAR (20)       NOT NULL,
    Motivo              NVARCHAR (300)      NOT NULL,
    Cantidad            INT                 NOT NULL,
    ProductoId          INT                 NOT NULL,
    AlmacenId           INT                 NOT NULL,
    UsuarioId           INT                 NOT NULL,
    DocumentoReferencia NVARCHAR (80)       NULL,
    StockResultante     INT                 NOT NULL,
    PrecioUnitario      DECIMAL (18, 2)     NOT NULL,
    CONSTRAINT PK_Movimientos PRIMARY KEY CLUSTERED (Id)
);

CREATE INDEX IX_Movimientos_Fecha ON Movimientos (Fecha);

CREATE INDEX IX_Movimientos_ProductoId ON Movimientos (ProductoId);

CREATE INDEX IX_Movimientos_AlmacenId ON Movimientos (AlmacenId);

CREATE INDEX IX_Movimientos_UsuarioId ON Movimientos (UsuarioId);

-- ---------------------------------------------------------------------
-- Notificaciones (avisos de stock bajo)
-- ---------------------------------------------------------------------
CREATE TABLE Notificaciones (
    Id            INT IDENTITY (1, 1) NOT NULL,
    ProductoId    INT                 NULL,
    AlmacenId     INT                 NULL,
    Mensaje       NVARCHAR (400)      NOT NULL,
    Nivel         NVARCHAR (20)       NOT NULL,
    Leida         BIT                 NOT NULL,
    FechaCreacion DATETIME2           NOT NULL,
    CONSTRAINT PK_Notificaciones PRIMARY KEY CLUSTERED (Id)
);

CREATE INDEX IX_Notificaciones_ProductoId ON Notificaciones (ProductoId);

CREATE INDEX IX_Notificaciones_AlmacenId ON Notificaciones (AlmacenId);

CREATE INDEX IX_Notificaciones_Leida ON Notificaciones (Leida);

-- =====================================================================
-- Claves foráneas
-- =====================================================================

ALTER TABLE Usuarios
    ADD CONSTRAINT FK_Usuarios_Roles_RolId
    FOREIGN KEY (RolId) REFERENCES Roles (Id)
    ON DELETE NO ACTION;

ALTER TABLE Productos
    ADD CONSTRAINT FK_Productos_Categorias_CategoriaId
    FOREIGN KEY (CategoriaId) REFERENCES Categorias (Id)
    ON DELETE NO ACTION;

ALTER TABLE Productos
    ADD CONSTRAINT FK_Productos_Proveedores_ProveedorId
    FOREIGN KEY (ProveedorId) REFERENCES Proveedores (Id)
    ON DELETE SET NULL;

ALTER TABLE NivelesStock
    ADD CONSTRAINT FK_NivelesStock_Productos_ProductoId
    FOREIGN KEY (ProductoId) REFERENCES Productos (Id)
    ON DELETE CASCADE;

ALTER TABLE NivelesStock
    ADD CONSTRAINT FK_NivelesStock_Almacenes_AlmacenId
    FOREIGN KEY (AlmacenId) REFERENCES Almacenes (Id)
    ON DELETE CASCADE;

ALTER TABLE Movimientos
    ADD CONSTRAINT FK_Movimientos_Productos_ProductoId
    FOREIGN KEY (ProductoId) REFERENCES Productos (Id)
    ON DELETE NO ACTION;

ALTER TABLE Movimientos
    ADD CONSTRAINT FK_Movimientos_Almacenes_AlmacenId
    FOREIGN KEY (AlmacenId) REFERENCES Almacenes (Id)
    ON DELETE NO ACTION;

ALTER TABLE Movimientos
    ADD CONSTRAINT FK_Movimientos_Usuarios_UsuarioId
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios (Id)
    ON DELETE NO ACTION;

ALTER TABLE Notificaciones
    ADD CONSTRAINT FK_Notificaciones_Productos_ProductoId
    FOREIGN KEY (ProductoId) REFERENCES Productos (Id)
    ON DELETE CASCADE;

ALTER TABLE Notificaciones
    ADD CONSTRAINT FK_Notificaciones_Almacenes_AlmacenId
    FOREIGN KEY (AlmacenId) REFERENCES Almacenes (Id)
    ON DELETE CASCADE;

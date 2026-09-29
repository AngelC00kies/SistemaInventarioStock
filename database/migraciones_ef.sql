IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [RefreshToken] nvarchar(max) NULL,
        [RefreshTokenExpiry] datetime2 NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(450) NOT NULL,
        [Description] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE TABLE [Suppliers] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(450) NOT NULL,
        [ContactName] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Suppliers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE TABLE [Warehouses] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Warehouses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [CategoryId] int NOT NULL,
        [SupplierId] int NULL,
        [UnitOfMeasure] nvarchar(30) NOT NULL,
        [PurchasePrice] decimal(18,2) NOT NULL,
        [SalePrice] decimal(18,2) NOT NULL,
        [MinimumStock] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Products_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE TABLE [LowStockNotifications] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [WarehouseId] int NOT NULL,
        [QuantityAtDetection] int NOT NULL,
        [MinimumStock] int NOT NULL,
        [DetectedAt] datetime2 NOT NULL,
        [IsResolved] bit NOT NULL,
        [ResolvedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_LowStockNotifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LowStockNotifications_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LowStockNotifications_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE TABLE [Movements] (
        [Id] int NOT NULL IDENTITY,
        [Date] datetime2 NOT NULL,
        [Type] nvarchar(20) NOT NULL,
        [Reason] nvarchar(max) NOT NULL,
        [Quantity] int NOT NULL,
        [StockAfter] int NOT NULL,
        [DocumentReference] nvarchar(max) NULL,
        [ProductId] int NOT NULL,
        [WarehouseId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [UserName] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Movements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Movements_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Movements_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE TABLE [Stocks] (
        [ProductId] int NOT NULL,
        [WarehouseId] int NOT NULL,
        [Quantity] int NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Stocks] PRIMARY KEY ([ProductId], [WarehouseId]),
        CONSTRAINT [FK_Stocks_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Stocks_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categories_Name] ON [Categories] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LowStockNotifications_ProductId_WarehouseId_IsResolved] ON [LowStockNotifications] ([ProductId], [WarehouseId], [IsResolved]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LowStockNotifications_WarehouseId] ON [LowStockNotifications] ([WarehouseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Movements_Date] ON [Movements] ([Date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Movements_ProductId_Date] ON [Movements] ([ProductId], [Date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Movements_UserId] ON [Movements] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Movements_WarehouseId] ON [Movements] ([WarehouseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Products_Code] ON [Products] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_SupplierId] ON [Products] ([SupplierId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Stocks_WarehouseId] ON [Stocks] ([WarehouseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Suppliers_Name] ON [Suppliers] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Warehouses_Code] ON [Warehouses] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923164849_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260923164849_InitialCreate', N'8.0.26');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AspNetRoleClaims] DROP CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AspNetUserClaims] DROP CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AspNetUserLogins] DROP CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AspNetUserRoles] DROP CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AspNetUserRoles] DROP CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AspNetUserTokens] DROP CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [LowStockNotifications] DROP CONSTRAINT [FK_LowStockNotifications_Products_ProductId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [LowStockNotifications] DROP CONSTRAINT [FK_LowStockNotifications_Warehouses_WarehouseId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Movements] DROP CONSTRAINT [FK_Movements_Products_ProductId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Movements] DROP CONSTRAINT [FK_Movements_Warehouses_WarehouseId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Products] DROP CONSTRAINT [FK_Products_Categories_CategoryId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Products] DROP CONSTRAINT [FK_Products_Suppliers_SupplierId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Stocks] DROP CONSTRAINT [FK_Stocks_Products_ProductId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Stocks] DROP CONSTRAINT [FK_Stocks_Warehouses_WarehouseId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Warehouses] DROP CONSTRAINT [PK_Warehouses];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Suppliers] DROP CONSTRAINT [PK_Suppliers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Stocks] DROP CONSTRAINT [PK_Stocks];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Products] DROP CONSTRAINT [PK_Products];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Movements] DROP CONSTRAINT [PK_Movements];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [LowStockNotifications] DROP CONSTRAINT [PK_LowStockNotifications];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Categories] DROP CONSTRAINT [PK_Categories];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AspNetUserTokens] DROP CONSTRAINT [PK_AspNetUserTokens];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AspNetUsers] DROP CONSTRAINT [PK_AspNetUsers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    DROP INDEX [UserNameIndex] ON [AspNetUsers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AspNetUserRoles] DROP CONSTRAINT [PK_AspNetUserRoles];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AspNetUserLogins] DROP CONSTRAINT [PK_AspNetUserLogins];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AspNetUserClaims] DROP CONSTRAINT [PK_AspNetUserClaims];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AspNetRoles] DROP CONSTRAINT [PK_AspNetRoles];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    DROP INDEX [RoleNameIndex] ON [AspNetRoles];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AspNetRoleClaims] DROP CONSTRAINT [PK_AspNetRoleClaims];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Warehouses]', N'Almacenes';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Suppliers]', N'Proveedores';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Stocks]', N'Existencias';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Products]', N'Productos';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Movements]', N'Movimientos';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[LowStockNotifications]', N'AlertasStockBajo';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Categories]', N'Categorias';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AspNetUserTokens]', N'UsuariosTokens';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AspNetUsers]', N'Usuarios';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AspNetUserRoles]', N'UsuariosRoles';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AspNetUserLogins]', N'UsuariosLogins';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AspNetUserClaims]', N'UsuariosClaims';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AspNetRoles]', N'Roles';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AspNetRoleClaims]', N'RolesClaims';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Almacenes].[UpdatedAt]', N'FechaActualizacion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Almacenes].[Name]', N'Nombre', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Almacenes].[IsActive]', N'Activo', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Almacenes].[CreatedAt]', N'FechaCreacion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Almacenes].[Code]', N'Codigo', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Almacenes].[Address]', N'Direccion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Almacenes].[IX_Warehouses_Code]', N'IX_Almacenes_Codigo', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Proveedores].[UpdatedAt]', N'FechaActualizacion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Proveedores].[Phone]', N'Telefono', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Proveedores].[Name]', N'Nombre', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Proveedores].[IsActive]', N'Activo', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Proveedores].[CreatedAt]', N'FechaCreacion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Proveedores].[ContactName]', N'Contacto', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Proveedores].[Address]', N'Direccion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Proveedores].[IX_Suppliers_Name]', N'IX_Proveedores_Nombre', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Existencias].[UpdatedAt]', N'FechaActualizacion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Existencias].[Quantity]', N'Cantidad', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Existencias].[WarehouseId]', N'AlmacenId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Existencias].[ProductId]', N'ProductoId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Existencias].[IX_Stocks_WarehouseId]', N'IX_Existencias_AlmacenId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Productos].[UpdatedAt]', N'FechaActualizacion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Productos].[UnitOfMeasure]', N'UnidadMedida', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Productos].[SupplierId]', N'ProveedorId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Productos].[SalePrice]', N'PrecioVenta', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Productos].[PurchasePrice]', N'PrecioCompra', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Productos].[Name]', N'Nombre', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Productos].[MinimumStock]', N'StockMinimo', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Productos].[IsActive]', N'Activo', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Productos].[Description]', N'Descripcion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Productos].[CreatedAt]', N'FechaCreacion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Productos].[Code]', N'Codigo', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Productos].[CategoryId]', N'CategoriaId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Productos].[IX_Products_SupplierId]', N'IX_Productos_ProveedorId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Productos].[IX_Products_Code]', N'IX_Productos_Codigo', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Productos].[IX_Products_CategoryId]', N'IX_Productos_CategoriaId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Movimientos].[WarehouseId]', N'AlmacenId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Movimientos].[UserName]', N'NombreUsuario', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Movimientos].[UserId]', N'UsuarioId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Movimientos].[Type]', N'Tipo', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Movimientos].[StockAfter]', N'StockResultante', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Movimientos].[Reason]', N'Motivo', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Movimientos].[Quantity]', N'Cantidad', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Movimientos].[ProductId]', N'ProductoId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Movimientos].[DocumentReference]', N'DocumentoReferencia', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Movimientos].[Date]', N'Fecha', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Movimientos].[IX_Movements_WarehouseId]', N'IX_Movimientos_AlmacenId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Movimientos].[IX_Movements_UserId]', N'IX_Movimientos_UsuarioId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Movimientos].[IX_Movements_ProductId_Date]', N'IX_Movimientos_ProductoId_Fecha', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Movimientos].[IX_Movements_Date]', N'IX_Movimientos_Fecha', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AlertasStockBajo].[WarehouseId]', N'AlmacenId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AlertasStockBajo].[UpdatedAt]', N'FechaActualizacion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AlertasStockBajo].[ResolvedAt]', N'ResueltaEl', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AlertasStockBajo].[QuantityAtDetection]', N'CantidadDetectada', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AlertasStockBajo].[ProductId]', N'ProductoId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AlertasStockBajo].[MinimumStock]', N'StockMinimo', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AlertasStockBajo].[IsResolved]', N'Resuelta', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AlertasStockBajo].[IsActive]', N'Activo', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AlertasStockBajo].[DetectedAt]', N'DetectadaEl', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AlertasStockBajo].[CreatedAt]', N'FechaCreacion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AlertasStockBajo].[IX_LowStockNotifications_WarehouseId]', N'IX_AlertasStockBajo_AlmacenId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[AlertasStockBajo].[IX_LowStockNotifications_ProductId_WarehouseId_IsResolved]', N'IX_AlertasStockBajo_ProductoId_AlmacenId_Resuelta', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Categorias].[UpdatedAt]', N'FechaActualizacion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Categorias].[Name]', N'Nombre', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Categorias].[IsActive]', N'Activo', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Categorias].[Description]', N'Descripcion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Categorias].[CreatedAt]', N'FechaCreacion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Categorias].[IX_Categories_Name]', N'IX_Categorias_Nombre', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosTokens].[Value]', N'Valor', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosTokens].[Name]', N'Nombre', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosTokens].[LoginProvider]', N'ProveedorLogin', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosTokens].[UserId]', N'UsuarioId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[UserName]', N'NombreUsuario', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[TwoFactorEnabled]', N'DosFactoresHabilitado', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[SecurityStamp]', N'MarcaSeguridad', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[RefreshTokenExpiry]', N'TokenRefrescoVencimiento', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[RefreshToken]', N'TokenRefresco', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[PhoneNumberConfirmed]', N'TelefonoConfirmado', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[PhoneNumber]', N'Telefono', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[PasswordHash]', N'HashContrasena', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[NormalizedUserName]', N'NombreUsuarioNormalizado', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[NormalizedEmail]', N'EmailNormalizado', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[LockoutEnd]', N'BloqueoHasta', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[LockoutEnabled]', N'BloqueoHabilitado', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[IsActive]', N'Activo', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[FullName]', N'NombreCompleto', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[EmailConfirmed]', N'EmailConfirmado', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[ConcurrencyStamp]', N'MarcaConcurrencia', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Usuarios].[AccessFailedCount]', N'IntentosFallidos', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosRoles].[RoleId]', N'RolId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosRoles].[UserId]', N'UsuarioId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosRoles].[IX_AspNetUserRoles_RoleId]', N'IX_UsuariosRoles_RolId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosLogins].[UserId]', N'UsuarioId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosLogins].[ProviderDisplayName]', N'NombreProveedor', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosLogins].[ProviderKey]', N'ClaveProveedor', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosLogins].[LoginProvider]', N'ProveedorLogin', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosLogins].[IX_AspNetUserLogins_UserId]', N'IX_UsuariosLogins_UsuarioId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosClaims].[UserId]', N'UsuarioId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosClaims].[ClaimValue]', N'ValorReclamacion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosClaims].[ClaimType]', N'TipoReclamacion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[UsuariosClaims].[IX_AspNetUserClaims_UserId]', N'IX_UsuariosClaims_UsuarioId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Roles].[NormalizedName]', N'NombreNormalizado', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Roles].[Name]', N'Nombre', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[Roles].[ConcurrencyStamp]', N'MarcaConcurrencia', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[RolesClaims].[RoleId]', N'RolId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[RolesClaims].[ClaimValue]', N'ValorReclamacion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[RolesClaims].[ClaimType]', N'TipoReclamacion', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC sp_rename N'[RolesClaims].[IX_AspNetRoleClaims_RoleId]', N'IX_RolesClaims_RolId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Almacenes] ADD CONSTRAINT [PK_Almacenes] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Proveedores] ADD CONSTRAINT [PK_Proveedores] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Existencias] ADD CONSTRAINT [PK_Existencias] PRIMARY KEY ([ProductoId], [AlmacenId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Productos] ADD CONSTRAINT [PK_Productos] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Movimientos] ADD CONSTRAINT [PK_Movimientos] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AlertasStockBajo] ADD CONSTRAINT [PK_AlertasStockBajo] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Categorias] ADD CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [UsuariosTokens] ADD CONSTRAINT [PK_UsuariosTokens] PRIMARY KEY ([UsuarioId], [ProveedorLogin], [Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Usuarios] ADD CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [UsuariosRoles] ADD CONSTRAINT [PK_UsuariosRoles] PRIMARY KEY ([UsuarioId], [RolId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [UsuariosLogins] ADD CONSTRAINT [PK_UsuariosLogins] PRIMARY KEY ([ProveedorLogin], [ClaveProveedor]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [UsuariosClaims] ADD CONSTRAINT [PK_UsuariosClaims] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Roles] ADD CONSTRAINT [PK_Roles] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [RolesClaims] ADD CONSTRAINT [PK_RolesClaims] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [Usuarios] ([NombreUsuarioNormalizado]) WHERE [NombreUsuarioNormalizado] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [Roles] ([NombreNormalizado]) WHERE [NombreNormalizado] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AlertasStockBajo] ADD CONSTRAINT [FK_AlertasStockBajo_Almacenes_AlmacenId] FOREIGN KEY ([AlmacenId]) REFERENCES [Almacenes] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [AlertasStockBajo] ADD CONSTRAINT [FK_AlertasStockBajo_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Existencias] ADD CONSTRAINT [FK_Existencias_Almacenes_AlmacenId] FOREIGN KEY ([AlmacenId]) REFERENCES [Almacenes] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Existencias] ADD CONSTRAINT [FK_Existencias_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Movimientos] ADD CONSTRAINT [FK_Movimientos_Almacenes_AlmacenId] FOREIGN KEY ([AlmacenId]) REFERENCES [Almacenes] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Movimientos] ADD CONSTRAINT [FK_Movimientos_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Productos] ADD CONSTRAINT [FK_Productos_Categorias_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [Productos] ADD CONSTRAINT [FK_Productos_Proveedores_ProveedorId] FOREIGN KEY ([ProveedorId]) REFERENCES [Proveedores] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [RolesClaims] ADD CONSTRAINT [FK_RolesClaims_Roles_RolId] FOREIGN KEY ([RolId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [UsuariosClaims] ADD CONSTRAINT [FK_UsuariosClaims_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [UsuariosLogins] ADD CONSTRAINT [FK_UsuariosLogins_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [UsuariosRoles] ADD CONSTRAINT [FK_UsuariosRoles_Roles_RolId] FOREIGN KEY ([RolId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [UsuariosRoles] ADD CONSTRAINT [FK_UsuariosRoles_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    ALTER TABLE [UsuariosTokens] ADD CONSTRAINT [FK_UsuariosTokens_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929121717_RenombrarTablasAlEspanol'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260929121717_RenombrarTablasAlEspanol', N'8.0.26');
END;
GO

COMMIT;
GO


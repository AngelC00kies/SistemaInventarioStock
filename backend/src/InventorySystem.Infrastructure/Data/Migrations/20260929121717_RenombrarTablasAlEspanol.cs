using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarTablasAlEspanol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_LowStockNotifications_Products_ProductId",
                table: "LowStockNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_LowStockNotifications_Warehouses_WarehouseId",
                table: "LowStockNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Movements_Products_ProductId",
                table: "Movements");

            migrationBuilder.DropForeignKey(
                name: "FK_Movements_Warehouses_WarehouseId",
                table: "Movements");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Products_ProductId",
                table: "Stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Warehouses_WarehouseId",
                table: "Stocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Warehouses",
                table: "Warehouses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Suppliers",
                table: "Suppliers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Stocks",
                table: "Stocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Movements",
                table: "Movements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LowStockNotifications",
                table: "LowStockNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "Warehouses",
                newName: "Almacenes");

            migrationBuilder.RenameTable(
                name: "Suppliers",
                newName: "Proveedores");

            migrationBuilder.RenameTable(
                name: "Stocks",
                newName: "Existencias");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Productos");

            migrationBuilder.RenameTable(
                name: "Movements",
                newName: "Movimientos");

            migrationBuilder.RenameTable(
                name: "LowStockNotifications",
                newName: "AlertasStockBajo");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Categorias");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "UsuariosTokens");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "Usuarios");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "UsuariosRoles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "UsuariosLogins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "UsuariosClaims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "RolesClaims");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Almacenes",
                newName: "FechaActualizacion");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Almacenes",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Almacenes",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Almacenes",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Almacenes",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Almacenes",
                newName: "Direccion");

            migrationBuilder.RenameIndex(
                name: "IX_Warehouses_Code",
                table: "Almacenes",
                newName: "IX_Almacenes_Codigo");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Proveedores",
                newName: "FechaActualizacion");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Proveedores",
                newName: "Telefono");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Proveedores",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Proveedores",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Proveedores",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "ContactName",
                table: "Proveedores",
                newName: "Contacto");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Proveedores",
                newName: "Direccion");

            migrationBuilder.RenameIndex(
                name: "IX_Suppliers_Name",
                table: "Proveedores",
                newName: "IX_Proveedores_Nombre");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Existencias",
                newName: "FechaActualizacion");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "Existencias",
                newName: "Cantidad");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "Existencias",
                newName: "AlmacenId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Existencias",
                newName: "ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_Stocks_WarehouseId",
                table: "Existencias",
                newName: "IX_Existencias_AlmacenId");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Productos",
                newName: "FechaActualizacion");

            migrationBuilder.RenameColumn(
                name: "UnitOfMeasure",
                table: "Productos",
                newName: "UnidadMedida");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "Productos",
                newName: "ProveedorId");

            migrationBuilder.RenameColumn(
                name: "SalePrice",
                table: "Productos",
                newName: "PrecioVenta");

            migrationBuilder.RenameColumn(
                name: "PurchasePrice",
                table: "Productos",
                newName: "PrecioCompra");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Productos",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "MinimumStock",
                table: "Productos",
                newName: "StockMinimo");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Productos",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Productos",
                newName: "Descripcion");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Productos",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Productos",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Productos",
                newName: "CategoriaId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_SupplierId",
                table: "Productos",
                newName: "IX_Productos_ProveedorId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_Code",
                table: "Productos",
                newName: "IX_Productos_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CategoryId",
                table: "Productos",
                newName: "IX_Productos_CategoriaId");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "Movimientos",
                newName: "AlmacenId");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Movimientos",
                newName: "NombreUsuario");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Movimientos",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Movimientos",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "StockAfter",
                table: "Movimientos",
                newName: "StockResultante");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "Movimientos",
                newName: "Motivo");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "Movimientos",
                newName: "Cantidad");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Movimientos",
                newName: "ProductoId");

            migrationBuilder.RenameColumn(
                name: "DocumentReference",
                table: "Movimientos",
                newName: "DocumentoReferencia");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Movimientos",
                newName: "Fecha");

            migrationBuilder.RenameIndex(
                name: "IX_Movements_WarehouseId",
                table: "Movimientos",
                newName: "IX_Movimientos_AlmacenId");

            migrationBuilder.RenameIndex(
                name: "IX_Movements_UserId",
                table: "Movimientos",
                newName: "IX_Movimientos_UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Movements_ProductId_Date",
                table: "Movimientos",
                newName: "IX_Movimientos_ProductoId_Fecha");

            migrationBuilder.RenameIndex(
                name: "IX_Movements_Date",
                table: "Movimientos",
                newName: "IX_Movimientos_Fecha");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "AlertasStockBajo",
                newName: "AlmacenId");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "AlertasStockBajo",
                newName: "FechaActualizacion");

            migrationBuilder.RenameColumn(
                name: "ResolvedAt",
                table: "AlertasStockBajo",
                newName: "ResueltaEl");

            migrationBuilder.RenameColumn(
                name: "QuantityAtDetection",
                table: "AlertasStockBajo",
                newName: "CantidadDetectada");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "AlertasStockBajo",
                newName: "ProductoId");

            migrationBuilder.RenameColumn(
                name: "MinimumStock",
                table: "AlertasStockBajo",
                newName: "StockMinimo");

            migrationBuilder.RenameColumn(
                name: "IsResolved",
                table: "AlertasStockBajo",
                newName: "Resuelta");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "AlertasStockBajo",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "DetectedAt",
                table: "AlertasStockBajo",
                newName: "DetectadaEl");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "AlertasStockBajo",
                newName: "FechaCreacion");

            migrationBuilder.RenameIndex(
                name: "IX_LowStockNotifications_WarehouseId",
                table: "AlertasStockBajo",
                newName: "IX_AlertasStockBajo_AlmacenId");

            migrationBuilder.RenameIndex(
                name: "IX_LowStockNotifications_ProductId_WarehouseId_IsResolved",
                table: "AlertasStockBajo",
                newName: "IX_AlertasStockBajo_ProductoId_AlmacenId_Resuelta");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Categorias",
                newName: "FechaActualizacion");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Categorias",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Categorias",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Categorias",
                newName: "Descripcion");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Categorias",
                newName: "FechaCreacion");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_Name",
                table: "Categorias",
                newName: "IX_Categorias_Nombre");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "UsuariosTokens",
                newName: "Valor");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "UsuariosTokens",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "LoginProvider",
                table: "UsuariosTokens",
                newName: "ProveedorLogin");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UsuariosTokens",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Usuarios",
                newName: "NombreUsuario");

            migrationBuilder.RenameColumn(
                name: "TwoFactorEnabled",
                table: "Usuarios",
                newName: "DosFactoresHabilitado");

            migrationBuilder.RenameColumn(
                name: "SecurityStamp",
                table: "Usuarios",
                newName: "MarcaSeguridad");

            migrationBuilder.RenameColumn(
                name: "RefreshTokenExpiry",
                table: "Usuarios",
                newName: "TokenRefrescoVencimiento");

            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "Usuarios",
                newName: "TokenRefresco");

            migrationBuilder.RenameColumn(
                name: "PhoneNumberConfirmed",
                table: "Usuarios",
                newName: "TelefonoConfirmado");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Usuarios",
                newName: "Telefono");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Usuarios",
                newName: "HashContrasena");

            migrationBuilder.RenameColumn(
                name: "NormalizedUserName",
                table: "Usuarios",
                newName: "NombreUsuarioNormalizado");

            migrationBuilder.RenameColumn(
                name: "NormalizedEmail",
                table: "Usuarios",
                newName: "EmailNormalizado");

            migrationBuilder.RenameColumn(
                name: "LockoutEnd",
                table: "Usuarios",
                newName: "BloqueoHasta");

            migrationBuilder.RenameColumn(
                name: "LockoutEnabled",
                table: "Usuarios",
                newName: "BloqueoHabilitado");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Usuarios",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Usuarios",
                newName: "NombreCompleto");

            migrationBuilder.RenameColumn(
                name: "EmailConfirmed",
                table: "Usuarios",
                newName: "EmailConfirmado");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "Usuarios",
                newName: "MarcaConcurrencia");

            migrationBuilder.RenameColumn(
                name: "AccessFailedCount",
                table: "Usuarios",
                newName: "IntentosFallidos");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "UsuariosRoles",
                newName: "RolId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UsuariosRoles",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "UsuariosRoles",
                newName: "IX_UsuariosRoles_RolId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UsuariosLogins",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "ProviderDisplayName",
                table: "UsuariosLogins",
                newName: "NombreProveedor");

            migrationBuilder.RenameColumn(
                name: "ProviderKey",
                table: "UsuariosLogins",
                newName: "ClaveProveedor");

            migrationBuilder.RenameColumn(
                name: "LoginProvider",
                table: "UsuariosLogins",
                newName: "ProveedorLogin");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "UsuariosLogins",
                newName: "IX_UsuariosLogins_UsuarioId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UsuariosClaims",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "ClaimValue",
                table: "UsuariosClaims",
                newName: "ValorReclamacion");

            migrationBuilder.RenameColumn(
                name: "ClaimType",
                table: "UsuariosClaims",
                newName: "TipoReclamacion");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "UsuariosClaims",
                newName: "IX_UsuariosClaims_UsuarioId");

            migrationBuilder.RenameColumn(
                name: "NormalizedName",
                table: "Roles",
                newName: "NombreNormalizado");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Roles",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "Roles",
                newName: "MarcaConcurrencia");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "RolesClaims",
                newName: "RolId");

            migrationBuilder.RenameColumn(
                name: "ClaimValue",
                table: "RolesClaims",
                newName: "ValorReclamacion");

            migrationBuilder.RenameColumn(
                name: "ClaimType",
                table: "RolesClaims",
                newName: "TipoReclamacion");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "RolesClaims",
                newName: "IX_RolesClaims_RolId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Almacenes",
                table: "Almacenes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Proveedores",
                table: "Proveedores",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Existencias",
                table: "Existencias",
                columns: new[] { "ProductoId", "AlmacenId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Productos",
                table: "Productos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Movimientos",
                table: "Movimientos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AlertasStockBajo",
                table: "AlertasStockBajo",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categorias",
                table: "Categorias",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsuariosTokens",
                table: "UsuariosTokens",
                columns: new[] { "UsuarioId", "ProveedorLogin", "Nombre" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsuariosRoles",
                table: "UsuariosRoles",
                columns: new[] { "UsuarioId", "RolId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsuariosLogins",
                table: "UsuariosLogins",
                columns: new[] { "ProveedorLogin", "ClaveProveedor" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsuariosClaims",
                table: "UsuariosClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RolesClaims",
                table: "RolesClaims",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Usuarios",
                column: "NombreUsuarioNormalizado",
                unique: true,
                filter: "[NombreUsuarioNormalizado] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NombreNormalizado",
                unique: true,
                filter: "[NombreNormalizado] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasStockBajo_Almacenes_AlmacenId",
                table: "AlertasStockBajo",
                column: "AlmacenId",
                principalTable: "Almacenes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasStockBajo_Productos_ProductoId",
                table: "AlertasStockBajo",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Existencias_Almacenes_AlmacenId",
                table: "Existencias",
                column: "AlmacenId",
                principalTable: "Almacenes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Existencias_Productos_ProductoId",
                table: "Existencias",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Movimientos_Almacenes_AlmacenId",
                table: "Movimientos",
                column: "AlmacenId",
                principalTable: "Almacenes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Movimientos_Productos_ProductoId",
                table: "Movimientos",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Categorias_CategoriaId",
                table: "Productos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Proveedores_ProveedorId",
                table: "Productos",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RolesClaims_Roles_RolId",
                table: "RolesClaims",
                column: "RolId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuariosClaims_Usuarios_UsuarioId",
                table: "UsuariosClaims",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuariosLogins_Usuarios_UsuarioId",
                table: "UsuariosLogins",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuariosRoles_Roles_RolId",
                table: "UsuariosRoles",
                column: "RolId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuariosRoles_Usuarios_UsuarioId",
                table: "UsuariosRoles",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuariosTokens_Usuarios_UsuarioId",
                table: "UsuariosTokens",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlertasStockBajo_Almacenes_AlmacenId",
                table: "AlertasStockBajo");

            migrationBuilder.DropForeignKey(
                name: "FK_AlertasStockBajo_Productos_ProductoId",
                table: "AlertasStockBajo");

            migrationBuilder.DropForeignKey(
                name: "FK_Existencias_Almacenes_AlmacenId",
                table: "Existencias");

            migrationBuilder.DropForeignKey(
                name: "FK_Existencias_Productos_ProductoId",
                table: "Existencias");

            migrationBuilder.DropForeignKey(
                name: "FK_Movimientos_Almacenes_AlmacenId",
                table: "Movimientos");

            migrationBuilder.DropForeignKey(
                name: "FK_Movimientos_Productos_ProductoId",
                table: "Movimientos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Categorias_CategoriaId",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Proveedores_ProveedorId",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_RolesClaims_Roles_RolId",
                table: "RolesClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuariosClaims_Usuarios_UsuarioId",
                table: "UsuariosClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuariosLogins_Usuarios_UsuarioId",
                table: "UsuariosLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuariosRoles_Roles_RolId",
                table: "UsuariosRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuariosRoles_Usuarios_UsuarioId",
                table: "UsuariosRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuariosTokens_Usuarios_UsuarioId",
                table: "UsuariosTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsuariosTokens",
                table: "UsuariosTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsuariosRoles",
                table: "UsuariosRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsuariosLogins",
                table: "UsuariosLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsuariosClaims",
                table: "UsuariosClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RolesClaims",
                table: "RolesClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Proveedores",
                table: "Proveedores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Productos",
                table: "Productos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Movimientos",
                table: "Movimientos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Existencias",
                table: "Existencias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categorias",
                table: "Categorias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Almacenes",
                table: "Almacenes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AlertasStockBajo",
                table: "AlertasStockBajo");

            migrationBuilder.RenameTable(
                name: "UsuariosTokens",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "UsuariosRoles",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "UsuariosLogins",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "UsuariosClaims",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "RolesClaims",
                newName: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "Proveedores",
                newName: "Suppliers");

            migrationBuilder.RenameTable(
                name: "Productos",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "Movimientos",
                newName: "Movements");

            migrationBuilder.RenameTable(
                name: "Existencias",
                newName: "Stocks");

            migrationBuilder.RenameTable(
                name: "Categorias",
                newName: "Categories");

            migrationBuilder.RenameTable(
                name: "Almacenes",
                newName: "Warehouses");

            migrationBuilder.RenameTable(
                name: "AlertasStockBajo",
                newName: "LowStockNotifications");

            migrationBuilder.RenameColumn(
                name: "Valor",
                table: "AspNetUserTokens",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "AspNetUserTokens",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ProveedorLogin",
                table: "AspNetUserTokens",
                newName: "LoginProvider");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "AspNetUserTokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "RolId",
                table: "AspNetUserRoles",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "AspNetUserRoles",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UsuariosRoles_RolId",
                table: "AspNetUserRoles",
                newName: "IX_AspNetUserRoles_RoleId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "AspNetUserLogins",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "NombreProveedor",
                table: "AspNetUserLogins",
                newName: "ProviderDisplayName");

            migrationBuilder.RenameColumn(
                name: "ClaveProveedor",
                table: "AspNetUserLogins",
                newName: "ProviderKey");

            migrationBuilder.RenameColumn(
                name: "ProveedorLogin",
                table: "AspNetUserLogins",
                newName: "LoginProvider");

            migrationBuilder.RenameIndex(
                name: "IX_UsuariosLogins_UsuarioId",
                table: "AspNetUserLogins",
                newName: "IX_AspNetUserLogins_UserId");

            migrationBuilder.RenameColumn(
                name: "ValorReclamacion",
                table: "AspNetUserClaims",
                newName: "ClaimValue");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "AspNetUserClaims",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "TipoReclamacion",
                table: "AspNetUserClaims",
                newName: "ClaimType");

            migrationBuilder.RenameIndex(
                name: "IX_UsuariosClaims_UsuarioId",
                table: "AspNetUserClaims",
                newName: "IX_AspNetUserClaims_UserId");

            migrationBuilder.RenameColumn(
                name: "TokenRefrescoVencimiento",
                table: "AspNetUsers",
                newName: "RefreshTokenExpiry");

            migrationBuilder.RenameColumn(
                name: "TokenRefresco",
                table: "AspNetUsers",
                newName: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "TelefonoConfirmado",
                table: "AspNetUsers",
                newName: "PhoneNumberConfirmed");

            migrationBuilder.RenameColumn(
                name: "Telefono",
                table: "AspNetUsers",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "NombreUsuarioNormalizado",
                table: "AspNetUsers",
                newName: "NormalizedUserName");

            migrationBuilder.RenameColumn(
                name: "NombreUsuario",
                table: "AspNetUsers",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "NombreCompleto",
                table: "AspNetUsers",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "MarcaSeguridad",
                table: "AspNetUsers",
                newName: "SecurityStamp");

            migrationBuilder.RenameColumn(
                name: "MarcaConcurrencia",
                table: "AspNetUsers",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameColumn(
                name: "IntentosFallidos",
                table: "AspNetUsers",
                newName: "AccessFailedCount");

            migrationBuilder.RenameColumn(
                name: "HashContrasena",
                table: "AspNetUsers",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "EmailNormalizado",
                table: "AspNetUsers",
                newName: "NormalizedEmail");

            migrationBuilder.RenameColumn(
                name: "EmailConfirmado",
                table: "AspNetUsers",
                newName: "EmailConfirmed");

            migrationBuilder.RenameColumn(
                name: "DosFactoresHabilitado",
                table: "AspNetUsers",
                newName: "TwoFactorEnabled");

            migrationBuilder.RenameColumn(
                name: "BloqueoHasta",
                table: "AspNetUsers",
                newName: "LockoutEnd");

            migrationBuilder.RenameColumn(
                name: "BloqueoHabilitado",
                table: "AspNetUsers",
                newName: "LockoutEnabled");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "AspNetUsers",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "ValorReclamacion",
                table: "AspNetRoleClaims",
                newName: "ClaimValue");

            migrationBuilder.RenameColumn(
                name: "TipoReclamacion",
                table: "AspNetRoleClaims",
                newName: "ClaimType");

            migrationBuilder.RenameColumn(
                name: "RolId",
                table: "AspNetRoleClaims",
                newName: "RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_RolesClaims_RolId",
                table: "AspNetRoleClaims",
                newName: "IX_AspNetRoleClaims_RoleId");

            migrationBuilder.RenameColumn(
                name: "NombreNormalizado",
                table: "AspNetRoles",
                newName: "NormalizedName");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "AspNetRoles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "MarcaConcurrencia",
                table: "AspNetRoles",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameColumn(
                name: "Telefono",
                table: "Suppliers",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Suppliers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "Suppliers",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "FechaActualizacion",
                table: "Suppliers",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "Direccion",
                table: "Suppliers",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "Contacto",
                table: "Suppliers",
                newName: "ContactName");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "Suppliers",
                newName: "IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Proveedores_Nombre",
                table: "Suppliers",
                newName: "IX_Suppliers_Name");

            migrationBuilder.RenameColumn(
                name: "UnidadMedida",
                table: "Products",
                newName: "UnitOfMeasure");

            migrationBuilder.RenameColumn(
                name: "StockMinimo",
                table: "Products",
                newName: "MinimumStock");

            migrationBuilder.RenameColumn(
                name: "ProveedorId",
                table: "Products",
                newName: "SupplierId");

            migrationBuilder.RenameColumn(
                name: "PrecioVenta",
                table: "Products",
                newName: "SalePrice");

            migrationBuilder.RenameColumn(
                name: "PrecioCompra",
                table: "Products",
                newName: "PurchasePrice");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Products",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "Products",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "FechaActualizacion",
                table: "Products",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "Descripcion",
                table: "Products",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "Products",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "CategoriaId",
                table: "Products",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "Products",
                newName: "IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Productos_ProveedorId",
                table: "Products",
                newName: "IX_Products_SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_Productos_Codigo",
                table: "Products",
                newName: "IX_Products_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Productos_CategoriaId",
                table: "Products",
                newName: "IX_Products_CategoryId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Movements",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "Movements",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "StockResultante",
                table: "Movements",
                newName: "StockAfter");

            migrationBuilder.RenameColumn(
                name: "ProductoId",
                table: "Movements",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "NombreUsuario",
                table: "Movements",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "Motivo",
                table: "Movements",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "Fecha",
                table: "Movements",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "DocumentoReferencia",
                table: "Movements",
                newName: "DocumentReference");

            migrationBuilder.RenameColumn(
                name: "Cantidad",
                table: "Movements",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "AlmacenId",
                table: "Movements",
                newName: "WarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_Movimientos_UsuarioId",
                table: "Movements",
                newName: "IX_Movements_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Movimientos_ProductoId_Fecha",
                table: "Movements",
                newName: "IX_Movements_ProductId_Date");

            migrationBuilder.RenameIndex(
                name: "IX_Movimientos_Fecha",
                table: "Movements",
                newName: "IX_Movements_Date");

            migrationBuilder.RenameIndex(
                name: "IX_Movimientos_AlmacenId",
                table: "Movements",
                newName: "IX_Movements_WarehouseId");

            migrationBuilder.RenameColumn(
                name: "FechaActualizacion",
                table: "Stocks",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "Cantidad",
                table: "Stocks",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "AlmacenId",
                table: "Stocks",
                newName: "WarehouseId");

            migrationBuilder.RenameColumn(
                name: "ProductoId",
                table: "Stocks",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Existencias_AlmacenId",
                table: "Stocks",
                newName: "IX_Stocks_WarehouseId");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Categories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "Categories",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "FechaActualizacion",
                table: "Categories",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "Descripcion",
                table: "Categories",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "Categories",
                newName: "IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Categorias_Nombre",
                table: "Categories",
                newName: "IX_Categories_Name");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Warehouses",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "Warehouses",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "FechaActualizacion",
                table: "Warehouses",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "Direccion",
                table: "Warehouses",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "Warehouses",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "Warehouses",
                newName: "IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Almacenes_Codigo",
                table: "Warehouses",
                newName: "IX_Warehouses_Code");

            migrationBuilder.RenameColumn(
                name: "StockMinimo",
                table: "LowStockNotifications",
                newName: "MinimumStock");

            migrationBuilder.RenameColumn(
                name: "ResueltaEl",
                table: "LowStockNotifications",
                newName: "ResolvedAt");

            migrationBuilder.RenameColumn(
                name: "Resuelta",
                table: "LowStockNotifications",
                newName: "IsResolved");

            migrationBuilder.RenameColumn(
                name: "ProductoId",
                table: "LowStockNotifications",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "LowStockNotifications",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "FechaActualizacion",
                table: "LowStockNotifications",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "DetectadaEl",
                table: "LowStockNotifications",
                newName: "DetectedAt");

            migrationBuilder.RenameColumn(
                name: "CantidadDetectada",
                table: "LowStockNotifications",
                newName: "QuantityAtDetection");

            migrationBuilder.RenameColumn(
                name: "AlmacenId",
                table: "LowStockNotifications",
                newName: "WarehouseId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "LowStockNotifications",
                newName: "IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_AlertasStockBajo_ProductoId_AlmacenId_Resuelta",
                table: "LowStockNotifications",
                newName: "IX_LowStockNotifications_ProductId_WarehouseId_IsResolved");

            migrationBuilder.RenameIndex(
                name: "IX_AlertasStockBajo_AlmacenId",
                table: "LowStockNotifications",
                newName: "IX_LowStockNotifications_WarehouseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Suppliers",
                table: "Suppliers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Movements",
                table: "Movements",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stocks",
                table: "Stocks",
                columns: new[] { "ProductId", "WarehouseId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Warehouses",
                table: "Warehouses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LowStockNotifications",
                table: "LowStockNotifications",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LowStockNotifications_Products_ProductId",
                table: "LowStockNotifications",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LowStockNotifications_Warehouses_WarehouseId",
                table: "LowStockNotifications",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Movements_Products_ProductId",
                table: "Movements",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Movements_Warehouses_WarehouseId",
                table: "Movements",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Products_ProductId",
                table: "Stocks",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Warehouses_WarehouseId",
                table: "Stocks",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

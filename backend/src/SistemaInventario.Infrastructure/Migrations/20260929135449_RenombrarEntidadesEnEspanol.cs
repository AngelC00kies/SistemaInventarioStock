using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaInventario.Infrastructure.Migrations
{
    /// <summary>
    /// Renombra tablas, columnas, índices y restricciones del esquema en inglés al español.
    /// Operaciones 100% de renombrado: no se elimina ni se recrea ningún objeto,
    /// por lo que los datos existentes se conservan intactos.
    /// </summary>
    public partial class RenombrarEntidadesEnEspanol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ------------------------------------------------------------------
            // 1. Tablas
            // ------------------------------------------------------------------
            migrationBuilder.RenameTable(name: "Users", newName: "Usuarios");
            migrationBuilder.RenameTable(name: "Categories", newName: "Categorias");
            migrationBuilder.RenameTable(name: "Suppliers", newName: "Proveedores");
            migrationBuilder.RenameTable(name: "Warehouses", newName: "Almacenes");
            migrationBuilder.RenameTable(name: "Products", newName: "Productos");
            migrationBuilder.RenameTable(name: "StockLevels", newName: "NivelesStock");
            migrationBuilder.RenameTable(name: "Movements", newName: "Movimientos");
            migrationBuilder.RenameTable(name: "Notifications", newName: "Notificaciones");

            // ------------------------------------------------------------------
            // 2. Columnas — Roles (la tabla conserva su nombre)
            // ------------------------------------------------------------------
            migrationBuilder.RenameColumn(name: "Name", table: "Roles", newName: "Nombre");
            migrationBuilder.RenameColumn(name: "Description", table: "Roles", newName: "Descripcion");

            // ------------------------------------------------------------------
            // 3. Columnas — Usuarios
            // ------------------------------------------------------------------
            migrationBuilder.RenameColumn(name: "Username", table: "Usuarios", newName: "NombreUsuario");
            migrationBuilder.RenameColumn(name: "PasswordHash", table: "Usuarios", newName: "ContrasenaHash");
            migrationBuilder.RenameColumn(name: "FullName", table: "Usuarios", newName: "NombreCompleto");
            migrationBuilder.RenameColumn(name: "Email", table: "Usuarios", newName: "Correo");
            migrationBuilder.RenameColumn(name: "RoleId", table: "Usuarios", newName: "RolId");
            migrationBuilder.RenameColumn(name: "IsActive", table: "Usuarios", newName: "Activo");
            migrationBuilder.RenameColumn(name: "CreatedAt", table: "Usuarios", newName: "FechaCreacion");

            // ------------------------------------------------------------------
            // 4. Columnas — Categorias
            // ------------------------------------------------------------------
            migrationBuilder.RenameColumn(name: "Name", table: "Categorias", newName: "Nombre");
            migrationBuilder.RenameColumn(name: "Description", table: "Categorias", newName: "Descripcion");
            migrationBuilder.RenameColumn(name: "IsActive", table: "Categorias", newName: "Activo");

            // ------------------------------------------------------------------
            // 5. Columnas — Proveedores
            // ------------------------------------------------------------------
            migrationBuilder.RenameColumn(name: "Name", table: "Proveedores", newName: "Nombre");
            migrationBuilder.RenameColumn(name: "ContactName", table: "Proveedores", newName: "NombreContacto");
            migrationBuilder.RenameColumn(name: "Phone", table: "Proveedores", newName: "Telefono");
            migrationBuilder.RenameColumn(name: "Email", table: "Proveedores", newName: "Correo");
            migrationBuilder.RenameColumn(name: "Address", table: "Proveedores", newName: "Direccion");
            migrationBuilder.RenameColumn(name: "IsActive", table: "Proveedores", newName: "Activo");

            // ------------------------------------------------------------------
            // 6. Columnas — Almacenes
            // ------------------------------------------------------------------
            migrationBuilder.RenameColumn(name: "Name", table: "Almacenes", newName: "Nombre");
            migrationBuilder.RenameColumn(name: "Code", table: "Almacenes", newName: "Codigo");
            migrationBuilder.RenameColumn(name: "Location", table: "Almacenes", newName: "Ubicacion");
            migrationBuilder.RenameColumn(name: "IsActive", table: "Almacenes", newName: "Activo");

            // ------------------------------------------------------------------
            // 7. Columnas — Productos
            // ------------------------------------------------------------------
            migrationBuilder.RenameColumn(name: "Code", table: "Productos", newName: "Codigo");
            migrationBuilder.RenameColumn(name: "Name", table: "Productos", newName: "Nombre");
            migrationBuilder.RenameColumn(name: "Description", table: "Productos", newName: "Descripcion");
            migrationBuilder.RenameColumn(name: "CategoryId", table: "Productos", newName: "CategoriaId");
            migrationBuilder.RenameColumn(name: "SupplierId", table: "Productos", newName: "ProveedorId");
            migrationBuilder.RenameColumn(name: "PurchasePrice", table: "Productos", newName: "PrecioCompra");
            migrationBuilder.RenameColumn(name: "SalePrice", table: "Productos", newName: "PrecioVenta");
            migrationBuilder.RenameColumn(name: "Unit", table: "Productos", newName: "Unidad");
            migrationBuilder.RenameColumn(name: "MinStock", table: "Productos", newName: "StockMinimo");
            migrationBuilder.RenameColumn(name: "IsActive", table: "Productos", newName: "Activo");
            migrationBuilder.RenameColumn(name: "CreatedAt", table: "Productos", newName: "FechaCreacion");

            // ------------------------------------------------------------------
            // 8. Columnas — NivelesStock
            // ------------------------------------------------------------------
            migrationBuilder.RenameColumn(name: "ProductId", table: "NivelesStock", newName: "ProductoId");
            migrationBuilder.RenameColumn(name: "WarehouseId", table: "NivelesStock", newName: "AlmacenId");
            migrationBuilder.RenameColumn(name: "Quantity", table: "NivelesStock", newName: "Cantidad");

            // ------------------------------------------------------------------
            // 9. Columnas — Movimientos
            // ------------------------------------------------------------------
            migrationBuilder.RenameColumn(name: "Date", table: "Movimientos", newName: "Fecha");
            migrationBuilder.RenameColumn(name: "Type", table: "Movimientos", newName: "Tipo");
            migrationBuilder.RenameColumn(name: "Reason", table: "Movimientos", newName: "Motivo");
            migrationBuilder.RenameColumn(name: "Quantity", table: "Movimientos", newName: "Cantidad");
            migrationBuilder.RenameColumn(name: "ProductId", table: "Movimientos", newName: "ProductoId");
            migrationBuilder.RenameColumn(name: "WarehouseId", table: "Movimientos", newName: "AlmacenId");
            migrationBuilder.RenameColumn(name: "UserId", table: "Movimientos", newName: "UsuarioId");
            migrationBuilder.RenameColumn(name: "DocumentReference", table: "Movimientos", newName: "DocumentoReferencia");
            migrationBuilder.RenameColumn(name: "StockAfter", table: "Movimientos", newName: "StockResultante");
            migrationBuilder.RenameColumn(name: "UnitPrice", table: "Movimientos", newName: "PrecioUnitario");

            // ------------------------------------------------------------------
            // 10. Columnas — Notificaciones
            // ------------------------------------------------------------------
            migrationBuilder.RenameColumn(name: "ProductId", table: "Notificaciones", newName: "ProductoId");
            migrationBuilder.RenameColumn(name: "WarehouseId", table: "Notificaciones", newName: "AlmacenId");
            migrationBuilder.RenameColumn(name: "Message", table: "Notificaciones", newName: "Mensaje");
            migrationBuilder.RenameColumn(name: "Level", table: "Notificaciones", newName: "Nivel");
            migrationBuilder.RenameColumn(name: "IsRead", table: "Notificaciones", newName: "Leida");
            migrationBuilder.RenameColumn(name: "CreatedAt", table: "Notificaciones", newName: "FechaCreacion");

            // ------------------------------------------------------------------
            // 11. Índices primarios y secundarios
            // ------------------------------------------------------------------
            migrationBuilder.RenameIndex(name: "IX_Roles_Name", table: "Roles", newName: "IX_Roles_Nombre");

            migrationBuilder.RenameIndex(name: "PK_Users", table: "Usuarios", newName: "PK_Usuarios");
            migrationBuilder.RenameIndex(name: "IX_Users_Username", table: "Usuarios", newName: "IX_Usuarios_NombreUsuario");
            migrationBuilder.RenameIndex(name: "IX_Users_RoleId", table: "Usuarios", newName: "IX_Usuarios_RolId");

            migrationBuilder.RenameIndex(name: "PK_Categories", table: "Categorias", newName: "PK_Categorias");
            migrationBuilder.RenameIndex(name: "IX_Categories_Name", table: "Categorias", newName: "IX_Categorias_Nombre");

            migrationBuilder.RenameIndex(name: "PK_Suppliers", table: "Proveedores", newName: "PK_Proveedores");
            migrationBuilder.RenameIndex(name: "IX_Suppliers_Name", table: "Proveedores", newName: "IX_Proveedores_Nombre");

            migrationBuilder.RenameIndex(name: "PK_Warehouses", table: "Almacenes", newName: "PK_Almacenes");
            migrationBuilder.RenameIndex(name: "IX_Warehouses_Code", table: "Almacenes", newName: "IX_Almacenes_Codigo");

            migrationBuilder.RenameIndex(name: "PK_Products", table: "Productos", newName: "PK_Productos");
            migrationBuilder.RenameIndex(name: "IX_Products_Code", table: "Productos", newName: "IX_Productos_Codigo");
            migrationBuilder.RenameIndex(name: "IX_Products_CategoryId", table: "Productos", newName: "IX_Productos_CategoriaId");
            migrationBuilder.RenameIndex(name: "IX_Products_SupplierId", table: "Productos", newName: "IX_Productos_ProveedorId");

            migrationBuilder.RenameIndex(name: "PK_StockLevels", table: "NivelesStock", newName: "PK_NivelesStock");
            migrationBuilder.RenameIndex(name: "IX_StockLevels_WarehouseId", table: "NivelesStock", newName: "IX_NivelesStock_AlmacenId");

            migrationBuilder.RenameIndex(name: "PK_Movements", table: "Movimientos", newName: "PK_Movimientos");
            migrationBuilder.RenameIndex(name: "IX_Movements_Date", table: "Movimientos", newName: "IX_Movimientos_Fecha");
            migrationBuilder.RenameIndex(name: "IX_Movements_ProductId", table: "Movimientos", newName: "IX_Movimientos_ProductoId");
            migrationBuilder.RenameIndex(name: "IX_Movements_WarehouseId", table: "Movimientos", newName: "IX_Movimientos_AlmacenId");
            migrationBuilder.RenameIndex(name: "IX_Movements_UserId", table: "Movimientos", newName: "IX_Movimientos_UsuarioId");

            migrationBuilder.RenameIndex(name: "PK_Notifications", table: "Notificaciones", newName: "PK_Notificaciones");
            migrationBuilder.RenameIndex(name: "IX_Notifications_ProductId", table: "Notificaciones", newName: "IX_Notificaciones_ProductoId");
            migrationBuilder.RenameIndex(name: "IX_Notifications_WarehouseId", table: "Notificaciones", newName: "IX_Notificaciones_AlmacenId");
            migrationBuilder.RenameIndex(name: "IX_Notifications_IsRead", table: "Notificaciones", newName: "IX_Notificaciones_Leida");

            // ------------------------------------------------------------------
            // 12. Restricciones de clave foránea (sp_rename sobre objetos)
            // ------------------------------------------------------------------
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Users_Roles_RoleId', N'FK_Usuarios_Roles_RolId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Products_Categories_CategoryId', N'FK_Productos_Categorias_CategoriaId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Products_Suppliers_SupplierId', N'FK_Productos_Proveedores_ProveedorId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_StockLevels_Products_ProductId', N'FK_NivelesStock_Productos_ProductoId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_StockLevels_Warehouses_WarehouseId', N'FK_NivelesStock_Almacenes_AlmacenId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Movements_Products_ProductId', N'FK_Movimientos_Productos_ProductoId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Movements_Warehouses_WarehouseId', N'FK_Movimientos_Almacenes_AlmacenId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Movements_Users_UserId', N'FK_Movimientos_Usuarios_UsuarioId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Notifications_Products_ProductId', N'FK_Notificaciones_Productos_ProductoId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Notifications_Warehouses_WarehouseId', N'FK_Notificaciones_Almacenes_AlmacenId', N'OBJECT';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restricciones de clave foránea (vuelta al inglés)
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Usuarios_Roles_RolId', N'FK_Users_Roles_RoleId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Productos_Categorias_CategoriaId', N'FK_Products_Categories_CategoryId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Productos_Proveedores_ProveedorId', N'FK_Products_Suppliers_SupplierId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_NivelesStock_Productos_ProductoId', N'FK_StockLevels_Products_ProductId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_NivelesStock_Almacenes_AlmacenId', N'FK_StockLevels_Warehouses_WarehouseId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Movimientos_Productos_ProductoId', N'FK_Movements_Products_ProductId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Movimientos_Almacenes_AlmacenId', N'FK_Movements_Warehouses_WarehouseId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Movimientos_Usuarios_UsuarioId', N'FK_Movements_Users_UserId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Notificaciones_Productos_ProductoId', N'FK_Notifications_Products_ProductId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.FK_Notificaciones_Almacenes_AlmacenId', N'FK_Notifications_Warehouses_WarehouseId', N'OBJECT';");

            // Índices
            migrationBuilder.RenameIndex(name: "PK_Usuarios", table: "Usuarios", newName: "PK_Users");
            migrationBuilder.RenameIndex(name: "IX_Usuarios_NombreUsuario", table: "Usuarios", newName: "IX_Users_Username");
            migrationBuilder.RenameIndex(name: "IX_Usuarios_RolId", table: "Usuarios", newName: "IX_Users_RoleId");

            migrationBuilder.RenameIndex(name: "PK_Categorias", table: "Categorias", newName: "PK_Categories");
            migrationBuilder.RenameIndex(name: "IX_Categorias_Nombre", table: "Categorias", newName: "IX_Categories_Name");

            migrationBuilder.RenameIndex(name: "PK_Proveedores", table: "Proveedores", newName: "PK_Suppliers");
            migrationBuilder.RenameIndex(name: "IX_Proveedores_Nombre", table: "Proveedores", newName: "IX_Suppliers_Name");

            migrationBuilder.RenameIndex(name: "PK_Almacenes", table: "Almacenes", newName: "PK_Warehouses");
            migrationBuilder.RenameIndex(name: "IX_Almacenes_Codigo", table: "Almacenes", newName: "IX_Warehouses_Code");

            migrationBuilder.RenameIndex(name: "PK_Productos", table: "Productos", newName: "PK_Products");
            migrationBuilder.RenameIndex(name: "IX_Productos_Codigo", table: "Productos", newName: "IX_Products_Code");
            migrationBuilder.RenameIndex(name: "IX_Productos_CategoriaId", table: "Productos", newName: "IX_Products_CategoryId");
            migrationBuilder.RenameIndex(name: "IX_Productos_ProveedorId", table: "Productos", newName: "IX_Products_SupplierId");

            migrationBuilder.RenameIndex(name: "PK_NivelesStock", table: "NivelesStock", newName: "PK_StockLevels");
            migrationBuilder.RenameIndex(name: "IX_NivelesStock_AlmacenId", table: "NivelesStock", newName: "IX_StockLevels_WarehouseId");

            migrationBuilder.RenameIndex(name: "PK_Movimientos", table: "Movimientos", newName: "PK_Movements");
            migrationBuilder.RenameIndex(name: "IX_Movimientos_Fecha", table: "Movimientos", newName: "IX_Movements_Date");
            migrationBuilder.RenameIndex(name: "IX_Movimientos_ProductoId", table: "Movimientos", newName: "IX_Movements_ProductId");
            migrationBuilder.RenameIndex(name: "IX_Movimientos_AlmacenId", table: "Movimientos", newName: "IX_Movements_WarehouseId");
            migrationBuilder.RenameIndex(name: "IX_Movimientos_UsuarioId", table: "Movimientos", newName: "IX_Movements_UserId");

            migrationBuilder.RenameIndex(name: "PK_Notificaciones", table: "Notificaciones", newName: "PK_Notifications");
            migrationBuilder.RenameIndex(name: "IX_Notificaciones_ProductoId", table: "Notificaciones", newName: "IX_Notifications_ProductId");
            migrationBuilder.RenameIndex(name: "IX_Notificaciones_AlmacenId", table: "Notificaciones", newName: "IX_Notifications_WarehouseId");
            migrationBuilder.RenameIndex(name: "IX_Notificaciones_Leida", table: "Notificaciones", newName: "IX_Notifications_IsRead");

            migrationBuilder.RenameIndex(name: "IX_Roles_Nombre", table: "Roles", newName: "IX_Roles_Name");

            // Columnas
            migrationBuilder.RenameColumn(name: "Nombre", table: "Roles", newName: "Name");
            migrationBuilder.RenameColumn(name: "Descripcion", table: "Roles", newName: "Description");

            migrationBuilder.RenameColumn(name: "NombreUsuario", table: "Usuarios", newName: "Username");
            migrationBuilder.RenameColumn(name: "ContrasenaHash", table: "Usuarios", newName: "PasswordHash");
            migrationBuilder.RenameColumn(name: "NombreCompleto", table: "Usuarios", newName: "FullName");
            migrationBuilder.RenameColumn(name: "Correo", table: "Usuarios", newName: "Email");
            migrationBuilder.RenameColumn(name: "RolId", table: "Usuarios", newName: "RoleId");
            migrationBuilder.RenameColumn(name: "Activo", table: "Usuarios", newName: "IsActive");
            migrationBuilder.RenameColumn(name: "FechaCreacion", table: "Usuarios", newName: "CreatedAt");

            migrationBuilder.RenameColumn(name: "Nombre", table: "Categorias", newName: "Name");
            migrationBuilder.RenameColumn(name: "Descripcion", table: "Categorias", newName: "Description");
            migrationBuilder.RenameColumn(name: "Activo", table: "Categorias", newName: "IsActive");

            migrationBuilder.RenameColumn(name: "Nombre", table: "Proveedores", newName: "Name");
            migrationBuilder.RenameColumn(name: "NombreContacto", table: "Proveedores", newName: "ContactName");
            migrationBuilder.RenameColumn(name: "Telefono", table: "Proveedores", newName: "Phone");
            migrationBuilder.RenameColumn(name: "Correo", table: "Proveedores", newName: "Email");
            migrationBuilder.RenameColumn(name: "Direccion", table: "Proveedores", newName: "Address");
            migrationBuilder.RenameColumn(name: "Activo", table: "Proveedores", newName: "IsActive");

            migrationBuilder.RenameColumn(name: "Nombre", table: "Almacenes", newName: "Name");
            migrationBuilder.RenameColumn(name: "Codigo", table: "Almacenes", newName: "Code");
            migrationBuilder.RenameColumn(name: "Ubicacion", table: "Almacenes", newName: "Location");
            migrationBuilder.RenameColumn(name: "Activo", table: "Almacenes", newName: "IsActive");

            migrationBuilder.RenameColumn(name: "Codigo", table: "Productos", newName: "Code");
            migrationBuilder.RenameColumn(name: "Nombre", table: "Productos", newName: "Name");
            migrationBuilder.RenameColumn(name: "Descripcion", table: "Productos", newName: "Description");
            migrationBuilder.RenameColumn(name: "CategoriaId", table: "Productos", newName: "CategoryId");
            migrationBuilder.RenameColumn(name: "ProveedorId", table: "Productos", newName: "SupplierId");
            migrationBuilder.RenameColumn(name: "PrecioCompra", table: "Productos", newName: "PurchasePrice");
            migrationBuilder.RenameColumn(name: "PrecioVenta", table: "Productos", newName: "SalePrice");
            migrationBuilder.RenameColumn(name: "Unidad", table: "Productos", newName: "Unit");
            migrationBuilder.RenameColumn(name: "StockMinimo", table: "Productos", newName: "MinStock");
            migrationBuilder.RenameColumn(name: "Activo", table: "Productos", newName: "IsActive");
            migrationBuilder.RenameColumn(name: "FechaCreacion", table: "Productos", newName: "CreatedAt");

            migrationBuilder.RenameColumn(name: "ProductoId", table: "NivelesStock", newName: "ProductId");
            migrationBuilder.RenameColumn(name: "AlmacenId", table: "NivelesStock", newName: "WarehouseId");
            migrationBuilder.RenameColumn(name: "Cantidad", table: "NivelesStock", newName: "Quantity");

            migrationBuilder.RenameColumn(name: "Fecha", table: "Movimientos", newName: "Date");
            migrationBuilder.RenameColumn(name: "Tipo", table: "Movimientos", newName: "Type");
            migrationBuilder.RenameColumn(name: "Motivo", table: "Movimientos", newName: "Reason");
            migrationBuilder.RenameColumn(name: "Cantidad", table: "Movimientos", newName: "Quantity");
            migrationBuilder.RenameColumn(name: "ProductoId", table: "Movimientos", newName: "ProductId");
            migrationBuilder.RenameColumn(name: "AlmacenId", table: "Movimientos", newName: "WarehouseId");
            migrationBuilder.RenameColumn(name: "UsuarioId", table: "Movimientos", newName: "UserId");
            migrationBuilder.RenameColumn(name: "DocumentoReferencia", table: "Movimientos", newName: "DocumentReference");
            migrationBuilder.RenameColumn(name: "StockResultante", table: "Movimientos", newName: "StockAfter");
            migrationBuilder.RenameColumn(name: "PrecioUnitario", table: "Movimientos", newName: "UnitPrice");

            migrationBuilder.RenameColumn(name: "ProductoId", table: "Notificaciones", newName: "ProductId");
            migrationBuilder.RenameColumn(name: "AlmacenId", table: "Notificaciones", newName: "WarehouseId");
            migrationBuilder.RenameColumn(name: "Mensaje", table: "Notificaciones", newName: "Message");
            migrationBuilder.RenameColumn(name: "Nivel", table: "Notificaciones", newName: "Level");
            migrationBuilder.RenameColumn(name: "Leida", table: "Notificaciones", newName: "IsRead");
            migrationBuilder.RenameColumn(name: "FechaCreacion", table: "Notificaciones", newName: "CreatedAt");

            // Tablas
            migrationBuilder.RenameTable(name: "Usuarios", newName: "Users");
            migrationBuilder.RenameTable(name: "Categorias", newName: "Categories");
            migrationBuilder.RenameTable(name: "Proveedores", newName: "Suppliers");
            migrationBuilder.RenameTable(name: "Almacenes", newName: "Warehouses");
            migrationBuilder.RenameTable(name: "Productos", newName: "Products");
            migrationBuilder.RenameTable(name: "NivelesStock", newName: "StockLevels");
            migrationBuilder.RenameTable(name: "Movimientos", newName: "Movements");
            migrationBuilder.RenameTable(name: "Notificaciones", newName: "Notifications");
        }
    }
}

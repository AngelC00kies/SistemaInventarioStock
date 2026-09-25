using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Entities;

namespace SistemaInventario.Infrastructure.Data;

public static class Mapping
{
    public static string ProductStatus(int total, int minStock)
    {
        if (total <= 0) return "empty";
        if (total <= minStock) return total <= Math.Max(1, minStock / 2) ? "critical" : "low";
        return "ok";
    }

    public static ProductDto ToDto(Producto p, int? warehouseId = null)
    {
        var stockLevels = p.NivelesStock ?? new List<NivelStock>();
        var total = stockLevels.Sum(s => s.Cantidad);

        return new ProductDto
        {
            Id = p.Id,
            Code = p.Codigo,
            Name = p.Nombre,
            Description = p.Descripcion,
            CategoryId = p.CategoriaId,
            CategoryName = p.Categoria?.Nombre ?? string.Empty,
            SupplierId = p.ProveedorId,
            SupplierName = p.Proveedor?.Nombre,
            PurchasePrice = p.PrecioCompra,
            SalePrice = p.PrecioVenta,
            Unit = p.Unidad,
            MinStock = p.StockMinimo,
            IsActive = p.Activo,
            TotalStock = total,
            Status = ProductStatus(total, p.StockMinimo),
            CreatedAt = p.FechaCreacion,
            StockByWarehouse = stockLevels
                .Where(s => !warehouseId.HasValue || s.AlmacenId == warehouseId.Value)
                .Select(s => new WarehouseStockDto
                {
                    WarehouseId = s.AlmacenId,
                    WarehouseName = s.Almacen?.Nombre ?? string.Empty,
                    Quantity = s.Cantidad
                })
                .OrderBy(s => s.WarehouseName)
                .ToList()
        };
    }

    public static CategoryDto ToDto(Categoria c) => new()
    {
        Id = c.Id,
        Name = c.Nombre,
        Description = c.Descripcion,
        IsActive = c.Activo,
        ProductCount = c.Productos?.Count(p => p.Activo) ?? 0
    };

    public static SupplierDto ToDto(Proveedor s) => new()
    {
        Id = s.Id,
        Name = s.Nombre,
        ContactName = s.NombreContacto,
        Phone = s.Telefono,
        Email = s.Correo,
        Address = s.Direccion,
        IsActive = s.Activo,
        ProductCount = s.Productos?.Count(p => p.Activo) ?? 0
    };

    public static WarehouseDto ToDto(Almacen w) => new()
    {
        Id = w.Id,
        Name = w.Nombre,
        Code = w.Codigo,
        Location = w.Ubicacion,
        IsActive = w.Activo,
        ProductCount = w.NivelesStock?.Count(s => s.Cantidad > 0) ?? 0,
        TotalUnits = w.NivelesStock?.Sum(s => s.Cantidad) ?? 0
    };

    public static MovementDto ToDto(Movimiento m) => new()
    {
        Id = m.Id,
        Date = m.Fecha,
        Type = m.Tipo,
        Reason = m.Motivo,
        Quantity = m.Cantidad,
        ProductId = m.ProductoId,
        ProductCode = m.Producto?.Codigo ?? string.Empty,
        ProductName = m.Producto?.Nombre ?? string.Empty,
        WarehouseId = m.AlmacenId,
        WarehouseName = m.Almacen?.Nombre ?? string.Empty,
        UserId = m.UsuarioId,
        UserName = m.Usuario?.NombreCompleto ?? string.Empty,
        DocumentReference = m.DocumentoReferencia,
        StockAfter = m.StockResultante,
        UnitPrice = m.PrecioUnitario
    };

    public static UserDto ToDto(Usuario u) => new()
    {
        Id = u.Id,
        Username = u.NombreUsuario,
        FullName = u.NombreCompleto,
        Email = u.Correo,
        RoleId = u.RolId,
        Role = u.Rol?.Nombre ?? string.Empty,
        IsActive = u.Activo,
        CreatedAt = u.FechaCreacion
    };

    public static NotificationDto ToDto(Notificacion n) => new()
    {
        Id = n.Id,
        ProductId = n.ProductoId,
        ProductName = n.Producto?.Nombre,
        WarehouseName = n.Almacen?.Nombre,
        Message = n.Mensaje,
        Level = n.Nivel,
        IsRead = n.Leida,
        CreatedAt = n.FechaCreacion
    };
}

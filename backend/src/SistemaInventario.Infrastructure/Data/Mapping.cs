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

    public static ProductDto ToDto(Product p, int? warehouseId = null)
    {
        var stockLevels = p.StockLevels ?? new List<StockLevel>();
        var total = stockLevels.Sum(s => s.Quantity);

        return new ProductDto
        {
            Id = p.Id,
            Code = p.Code,
            Name = p.Name,
            Description = p.Description,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? string.Empty,
            SupplierId = p.SupplierId,
            SupplierName = p.Supplier?.Name,
            PurchasePrice = p.PurchasePrice,
            SalePrice = p.SalePrice,
            Unit = p.Unit,
            MinStock = p.MinStock,
            IsActive = p.IsActive,
            TotalStock = total,
            Status = ProductStatus(total, p.MinStock),
            CreatedAt = p.CreatedAt,
            StockByWarehouse = stockLevels
                .Where(s => !warehouseId.HasValue || s.WarehouseId == warehouseId.Value)
                .Select(s => new WarehouseStockDto
                {
                    WarehouseId = s.WarehouseId,
                    WarehouseName = s.Warehouse?.Name ?? string.Empty,
                    Quantity = s.Quantity
                })
                .OrderBy(s => s.WarehouseName)
                .ToList()
        };
    }

    public static CategoryDto ToDto(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        IsActive = c.IsActive,
        ProductCount = c.Products?.Count(p => p.IsActive) ?? 0
    };

    public static SupplierDto ToDto(Supplier s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        ContactName = s.ContactName,
        Phone = s.Phone,
        Email = s.Email,
        Address = s.Address,
        IsActive = s.IsActive,
        ProductCount = s.Products?.Count(p => p.IsActive) ?? 0
    };

    public static WarehouseDto ToDto(Warehouse w) => new()
    {
        Id = w.Id,
        Name = w.Name,
        Code = w.Code,
        Location = w.Location,
        IsActive = w.IsActive,
        ProductCount = w.StockLevels?.Count(s => s.Quantity > 0) ?? 0,
        TotalUnits = w.StockLevels?.Sum(s => s.Quantity) ?? 0
    };

    public static MovementDto ToDto(Movement m) => new()
    {
        Id = m.Id,
        Date = m.Date,
        Type = m.Type,
        Reason = m.Reason,
        Quantity = m.Quantity,
        ProductId = m.ProductId,
        ProductCode = m.Product?.Code ?? string.Empty,
        ProductName = m.Product?.Name ?? string.Empty,
        WarehouseId = m.WarehouseId,
        WarehouseName = m.Warehouse?.Name ?? string.Empty,
        UserId = m.UserId,
        UserName = m.User?.FullName ?? string.Empty,
        DocumentReference = m.DocumentReference,
        StockAfter = m.StockAfter,
        UnitPrice = m.UnitPrice
    };

    public static UserDto ToDto(User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        FullName = u.FullName,
        Email = u.Email,
        RoleId = u.RoleId,
        Role = u.Role?.Name ?? string.Empty,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt
    };

    public static NotificationDto ToDto(AppNotification n) => new()
    {
        Id = n.Id,
        ProductId = n.ProductId,
        ProductName = n.Product?.Name,
        WarehouseName = n.Warehouse?.Name,
        Message = n.Message,
        Level = n.Level,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt
    };
}

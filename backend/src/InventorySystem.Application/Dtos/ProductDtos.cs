using InventorySystem.Application.Common;
using InventorySystem.Domain.Enums;

namespace InventorySystem.Application.Dtos;

public class ProductDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int MinimumStock { get; set; }
    public int TotalStock { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsActive { get; set; }
}

public class ProductRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; } = UnitOfMeasure.Unidad;
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int MinimumStock { get; set; }
}

public class ProductQuery : PagedQuery
{
    public int? CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public int? WarehouseId { get; set; }
    public bool LowStockOnly { get; set; }
    public bool OutOfStockOnly { get; set; }
}

public class StockEntryDto
{
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime UpdatedAt { get; set; }
}

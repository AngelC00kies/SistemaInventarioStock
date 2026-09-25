namespace SistemaInventario.Core.Dtos;

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
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int MinStock { get; set; }
    public bool IsActive { get; set; }
    public int TotalStock { get; set; }
    public string Status { get; set; } = "ok";
    public DateTime CreatedAt { get; set; }
    public List<WarehouseStockDto> StockByWarehouse { get; set; } = new();
}

public class WarehouseStockDto
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class ProductRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public string Unit { get; set; } = "Unidad";
    public int MinStock { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProductFilter
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public int? WarehouseId { get; set; }
    public bool? OnlyLowStock { get; set; }
    public bool? IncludeInactive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
}

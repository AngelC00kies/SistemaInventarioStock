namespace SistemaInventario.Core.Dtos;

/// <summary>Ficha de producto para el front: datos de la carta junto al stock agregado y al estado calculado.</summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    /// <summary>Null cuando el producto no tiene proveedor asignado.</summary>
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int MinStock { get; set; }
    public bool IsActive { get; set; }
    /// <summary>Suma de existencias en todos los almacenes (incluye almacenes desactivados).</summary>
    public int TotalStock { get; set; }
    /// <summary>Estado derivado de TotalStock frente a MinStock: "ok", "low", "critical" o "empty"; el front lo pinta con color.</summary>
    public string Status { get; set; } = "ok";
    public DateTime CreatedAt { get; set; }
    /// <summary>Desglose por almacén; si la petición pidió un almacén concreto, sólo aparece ese.</summary>
    public List<WarehouseStockDto> StockByWarehouse { get; set; } = new();
}

/// <summary>Existencias de un producto en un solo almacén, para la tabla de desglose.</summary>
public class WarehouseStockDto
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

/// <summary>Alta o edición de producto (misma forma en ambos casos); validado en el servidor antes de guardarlo.</summary>
public class ProductRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    /// <summary>Null para productos sin proveedor; si viene, debe apuntar a un proveedor activo.</summary>
    public int? SupplierId { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public string Unit { get; set; } = "Unidad";
    public int MinStock { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>Filtros y paginación del listado de productos; los nulos significan "sin filtrar por eso".</summary>
public class ProductFilter
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public int? SupplierId { get; set; }
    /// <summary>Sólo productos que tengan existencias en ese almacén.</summary>
    public int? WarehouseId { get; set; }
    /// <summary>true = sólo productos cuyo stock total no supera su MinStock.</summary>
    public bool? OnlyLowStock { get; set; }
    /// <summary>null o false = se omiten los productos inactivos.</summary>
    public bool? IncludeInactive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
}

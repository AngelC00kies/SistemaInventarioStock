namespace InventorySystem.Domain.Entities;

/// <summary>Cantidad de un producto en un almacén. PK compuesta (ProductId, WarehouseId).</summary>
public class Stock
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public int Quantity { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

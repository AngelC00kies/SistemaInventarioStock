using InventorySystem.Domain.Common;

namespace InventorySystem.Domain.Entities;

/// <summary>Alerta generada cuando el stock de un producto baja hasta el mínimo.</summary>
public class LowStockNotification : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public int QuantityAtDetection { get; set; }
    public int MinimumStock { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

using System.ComponentModel.DataAnnotations.Schema;
using InventorySystem.Domain.Common;

namespace InventorySystem.Domain.Entities;

/// <summary>Alerta generada cuando el stock de un producto baja hasta el mínimo.</summary>
[Table("AlertasStockBajo")]
public class LowStockNotification : BaseEntity
{
    [Column("ProductoId")]
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Column("AlmacenId")]
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    [Column("CantidadDetectada")]
    public int QuantityAtDetection { get; set; }

    [Column("StockMinimo")]
    public int MinimumStock { get; set; }

    [Column("DetectadaEl")]
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    [Column("Resuelta")]
    public bool IsResolved { get; set; }

    [Column("ResueltaEl")]
    public DateTime? ResolvedAt { get; set; }
}

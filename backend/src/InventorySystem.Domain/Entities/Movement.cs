using InventorySystem.Domain.Enums;

namespace InventorySystem.Domain.Entities;

/// <summary>Movimiento de inventario (entrada o salida) con trazabilidad completa.</summary>
public class Movement
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public MovementType Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }

    /// <summary>Stock resultante después del movimiento (snapshot para auditoría).</summary>
    public int StockAfter { get; set; }

    public string? DocumentReference { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>Id del usuario responsable.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Nombre del usuario al momento del movimiento (denormalizado para auditoría).</summary>
    public string UserName { get; set; } = string.Empty;
}

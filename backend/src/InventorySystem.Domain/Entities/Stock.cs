using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySystem.Domain.Entities;

/// <summary>
/// Cantidad de un producto en un almacén. PK compuesta (ProductId, WarehouseId)
/// → columnas <c>ProductoId</c> + <c>AlmacenId</c>.
/// </summary>
[Table("Existencias")]
public class Stock
{
    [Column("ProductoId")]
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Column("AlmacenId")]
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    [Column("Cantidad")]
    public int Quantity { get; set; }

    [Column("FechaActualizacion")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

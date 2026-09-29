using System.ComponentModel.DataAnnotations.Schema;
using InventorySystem.Domain.Common;

namespace InventorySystem.Domain.Entities;

/// <summary>Almacén o depósito físico.</summary>
[Table("Almacenes")]
public class Warehouse : BaseEntity
{
    [Column("Codigo")]
    public string Code { get; set; } = string.Empty;

    [Column("Nombre")]
    public string Name { get; set; } = string.Empty;

    [Column("Direccion")]
    public string? Address { get; set; }

    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
}

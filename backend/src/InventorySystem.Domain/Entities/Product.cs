using System.ComponentModel.DataAnnotations.Schema;
using InventorySystem.Domain.Common;
using InventorySystem.Domain.Enums;

namespace InventorySystem.Domain.Entities;

/// <summary>Producto del catálogo de inventario.</summary>
[Table("Productos")]
public class Product : BaseEntity
{
    [Column("Codigo")]
    public string Code { get; set; } = string.Empty;

    [Column("Nombre")]
    public string Name { get; set; } = string.Empty;

    [Column("Descripcion")]
    public string? Description { get; set; }

    [Column("CategoriaId")]
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    [Column("ProveedorId")]
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    [Column("UnidadMedida")]
    public UnitOfMeasure UnitOfMeasure { get; set; } = UnitOfMeasure.Unidad;

    [Column("PrecioCompra")]
    public decimal PurchasePrice { get; set; }

    [Column("PrecioVenta")]
    public decimal SalePrice { get; set; }

    [Column("StockMinimo")]
    public int MinimumStock { get; set; }

    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    public ICollection<Movement> Movements { get; set; } = new List<Movement>();
}

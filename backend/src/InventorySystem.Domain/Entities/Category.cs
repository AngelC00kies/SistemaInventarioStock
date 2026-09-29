using System.ComponentModel.DataAnnotations.Schema;
using InventorySystem.Domain.Common;

namespace InventorySystem.Domain.Entities;

/// <summary>Categoría de productos.</summary>
[Table("Categorias")]
public class Category : BaseEntity
{
    [Column("Nombre")]
    public string Name { get; set; } = string.Empty;

    [Column("Descripcion")]
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

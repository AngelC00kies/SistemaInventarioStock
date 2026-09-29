using System.ComponentModel.DataAnnotations.Schema;
using InventorySystem.Domain.Common;

namespace InventorySystem.Domain.Entities;

/// <summary>Proveedor de productos.</summary>
[Table("Proveedores")]
public class Supplier : BaseEntity
{
    [Column("Nombre")]
    public string Name { get; set; } = string.Empty;

    [Column("Contacto")]
    public string? ContactName { get; set; }

    [Column("Telefono")]
    public string? Phone { get; set; }

    [Column("Email")]
    public string? Email { get; set; }

    [Column("Direccion")]
    public string? Address { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

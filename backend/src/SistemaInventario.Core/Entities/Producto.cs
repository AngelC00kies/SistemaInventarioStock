namespace SistemaInventario.Core.Entities;

/// <summary>Artículo del inventario: lo que se compra, se almacena y se vende, repartido por almacenes en NivelStock.</summary>
public class Producto
{
    public int Id { get; set; }
    /// <summary>Referencia única y normalizada a mayúsculas; es la clave de búsqueda en listados e informes.</summary>
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CategoriaId { get; set; }
    /// <summary>Null cuando el producto no tiene proveedor asignado.</summary>
    public int? ProveedorId { get; set; }
    /// <summary>Precio de coste: es el precio unitario por defecto de las entradas.</summary>
    public decimal PrecioCompra { get; set; }
    /// <summary>Precio al público: precio unitario por defecto de las salidas y base del valor de stock.</summary>
    public decimal PrecioVenta { get; set; }
    /// <summary>Unidad de medida literal ("Unidad", "Caja", "Bidón"...) con la que se expresan las cantidades.</summary>
    public string Unidad { get; set; } = "Unidad";
    /// <summary>Umbral global (suma de todos los almacenes) a partir del cual el producto se considera con stock bajo.</summary>
    public int StockMinimo { get; set; }
    // Un producto inactivo no admite movimientos ni aparece en los listados por defecto;
    // si tiene histórico el "borrado" se resuelve desactivándolo en lugar de eliminarlo.
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Categoria Categoria { get; set; } = null!;
    public Proveedor? Proveedor { get; set; }
    public ICollection<NivelStock> NivelesStock { get; set; } = new List<NivelStock>();
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}

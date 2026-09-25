namespace SistemaInventario.Core.Entities;

public class Producto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CategoriaId { get; set; }
    public int? ProveedorId { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public string Unidad { get; set; } = "Unidad";
    public int StockMinimo { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Categoria Categoria { get; set; } = null!;
    public Proveedor? Proveedor { get; set; }
    public ICollection<NivelStock> NivelesStock { get; set; } = new List<NivelStock>();
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}

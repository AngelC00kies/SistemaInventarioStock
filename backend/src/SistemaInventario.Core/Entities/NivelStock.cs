namespace SistemaInventario.Core.Entities;

public class NivelStock
{
    public int ProductoId { get; set; }
    public int AlmacenId { get; set; }
    public int Cantidad { get; set; }

    public Producto Producto { get; set; } = null!;
    public Almacen Almacen { get; set; } = null!;
}

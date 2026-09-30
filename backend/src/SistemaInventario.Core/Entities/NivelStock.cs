namespace SistemaInventario.Core.Entities;

/// <summary>Existencias de un producto en un almacén concreto: es la fila de stock, la base de todos los totales.</summary>
public class NivelStock
{
    /// <summary>Sin Id propio: la clave primaria es la pareja (ProductoId, AlmacenId), una sola fila por combinación.</summary>
    public int ProductoId { get; set; }
    public int AlmacenId { get; set; }
    /// <summary>Unidades disponibles; nunca negativo porque las salidas se validan contra lo que hay.</summary>
    public int Cantidad { get; set; }

    public Producto Producto { get; set; } = null!;
    public Almacen Almacen { get; set; } = null!;
}

using SistemaInventario.Core.Enums;

namespace SistemaInventario.Core.Entities;

/// <summary>Movimiento de almacén: hecho consumado que registra una entrada o salida de stock y alimenta el histórico.</summary>
public class Movimiento
{
    public int Id { get; set; }
    /// <summary>Instante en UTC en que ocurrió; puede ser distinto del momento de registro (campo Date opcional del alta).</summary>
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public MovementType Tipo { get; set; }
    /// <summary>Concepto del movimiento; es obligatorio y además es uno de los campos de búsqueda del histórico.</summary>
    public string Motivo { get; set; } = string.Empty;
    /// <summary>Unidades movidas, siempre positivas: el signo lo aporta Tipo.</summary>
    public int Cantidad { get; set; }
    public int ProductoId { get; set; }
    public int AlmacenId { get; set; }
    /// <summary>Usuario que lo registró; no se puede dar de baja sin perder el rastro del movimiento.</summary>
    public int UsuarioId { get; set; }
    /// <summary>Documento que lo respalda (orden de compra, guía de despacho...); null si no se indicó ninguno.</summary>
    public string? DocumentoReferencia { get; set; }
    /// <summary>Stock que quedó en ese almacén tras aplicar el movimiento: fotografía que evita recalcular el histórico.</summary>
    public int StockResultante { get; set; }
    /// <summary>Coste (entrada) o precio de venta (salida) por unidad; Cantidad × PrecioUnitario es el valor del movimiento.</summary>
    public decimal PrecioUnitario { get; set; }

    public Producto Producto { get; set; } = null!;
    public Almacen Almacen { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}

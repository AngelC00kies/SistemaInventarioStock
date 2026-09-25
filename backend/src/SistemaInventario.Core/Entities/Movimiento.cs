using SistemaInventario.Core.Enums;

namespace SistemaInventario.Core.Entities;

public class Movimiento
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public MovementType Tipo { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public int ProductoId { get; set; }
    public int AlmacenId { get; set; }
    public int UsuarioId { get; set; }
    public string? DocumentoReferencia { get; set; }
    public int StockResultante { get; set; }
    public decimal PrecioUnitario { get; set; }

    public Producto Producto { get; set; } = null!;
    public Almacen Almacen { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}

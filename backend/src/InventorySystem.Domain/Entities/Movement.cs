using System.ComponentModel.DataAnnotations.Schema;
using InventorySystem.Domain.Enums;

namespace InventorySystem.Domain.Entities;

/// <summary>Movimiento de inventario (entrada o salida) con trazabilidad completa.</summary>
[Table("Movimientos")]
public class Movement
{
    public int Id { get; set; }

    [Column("Fecha")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Column("Tipo")]
    public MovementType Type { get; set; }

    [Column("Motivo")]
    public string Reason { get; set; } = string.Empty;

    [Column("Cantidad")]
    public int Quantity { get; set; }

    /// <summary>Stock resultante después del movimiento (snapshot para auditoría).</summary>
    [Column("StockResultante")]
    public int StockAfter { get; set; }

    [Column("DocumentoReferencia")]
    public string? DocumentReference { get; set; }

    [Column("ProductoId")]
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Column("AlmacenId")]
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>Id del usuario responsable.</summary>
    [Column("UsuarioId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>Nombre del usuario al momento del movimiento (denormalizado para auditoría).</summary>
    [Column("NombreUsuario")]
    public string UserName { get; set; } = string.Empty;
}

using SistemaInventario.Core.Enums;

namespace SistemaInventario.Core.Entities;

/// <summary>Aviso de stock bajo generado automáticamente cuando las existencias de un producto llegan a su mínimo.</summary>
public class Notificacion
{
    public int Id { get; set; }
    /// <summary>Null cuando el aviso no está ligado a un producto concreto (ámbito general).</summary>
    public int? ProductoId { get; set; }
    /// <summary>Almacén en el que se detectó el desabastecimiento; null si el aviso no aplica a uno determinado.</summary>
    public int? AlmacenId { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public NotificationLevel Nivel { get; set; } = NotificationLevel.Warning;
    // Se marca como leída tanto por el usuario como sola, sin intervención, cuando el stock vuelve a superar el mínimo.
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Producto? Producto { get; set; }
    public Almacen? Almacen { get; set; }
}

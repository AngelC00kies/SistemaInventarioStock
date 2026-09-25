using SistemaInventario.Core.Enums;

namespace SistemaInventario.Core.Entities;

public class Notificacion
{
    public int Id { get; set; }
    public int? ProductoId { get; set; }
    public int? AlmacenId { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public NotificationLevel Nivel { get; set; } = NotificationLevel.Warning;
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Producto? Producto { get; set; }
    public Almacen? Almacen { get; set; }
}

using SistemaInventario.Core.Enums;

namespace SistemaInventario.Core.Dtos;

/// <summary>Aviso para la campana del front: texto, gravedad y si ya fue leído.</summary>
public class NotificationDto
{
    public int Id { get; set; }
    /// <summary>Null cuando el aviso no está ligado a un producto concreto.</summary>
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    /// <summary>Almacén en el que se detectó el problema; null si no aplica a uno concreto.</summary>
    public string? WarehouseName { get; set; }
    public string Message { get; set; } = string.Empty;
    /// <summary>Gravedad: Info, Warning (stock en el mínimo) o Critical (a la mitad o por debajo).</summary>
    public NotificationLevel Level { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

using SistemaInventario.Core.Enums;

namespace SistemaInventario.Core.Entities;

public class AppNotification
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public int? WarehouseId { get; set; }
    public string Message { get; set; } = string.Empty;
    public NotificationLevel Level { get; set; } = NotificationLevel.Warning;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
}

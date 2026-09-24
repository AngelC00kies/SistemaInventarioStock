using SistemaInventario.Core.Enums;

namespace SistemaInventario.Core.Dtos;

public class NotificationDto
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? WarehouseName { get; set; }
    public string Message { get; set; } = string.Empty;
    public NotificationLevel Level { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

using InventorySystem.Application.Common;
using InventorySystem.Domain.Enums;

namespace InventorySystem.Application.Dtos;

public class MovementRequest
{
    public MovementType Type { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? DocumentReference { get; set; }
}

public class MovementDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public MovementType Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int StockAfter { get; set; }
    public string? DocumentReference { get; set; }
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}

public class MovementQuery : PagedQuery
{
    public int? ProductId { get; set; }
    public int? WarehouseId { get; set; }
    public MovementType? Type { get; set; }
    public string? UserId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

// ───────────────────────── Notificaciones ─────────────────────────

public class NotificationDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int QuantityAtDetection { get; set; }
    public int MinimumStock { get; set; }
    public DateTime DetectedAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

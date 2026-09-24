using SistemaInventario.Core.Enums;

namespace SistemaInventario.Core.Dtos;

public class MovementDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public MovementType Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? DocumentReference { get; set; }
    public int StockAfter { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total => Quantity * UnitPrice;
}

public class CreateMovementRequest
{
    public MovementType Type { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? DocumentReference { get; set; }
    public decimal? UnitPrice { get; set; }
    public DateTime? Date { get; set; }
}

public class MovementFilter
{
    public int? ProductId { get; set; }
    public int? WarehouseId { get; set; }
    public int? UserId { get; set; }
    public MovementType? Type { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
}

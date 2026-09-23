using InventorySystem.Domain.Enums;

namespace InventorySystem.Application.Dtos;

// ───────────────────────── Reportes ─────────────────────────

public class StockReportRow
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Supplier { get; set; }
    public string Warehouse { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinimumStock { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal StockValue { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class StockReportQuery
{
    public int? CategoryId { get; set; }
    public int? WarehouseId { get; set; }
    public bool CriticalOnly { get; set; }
}

public class MovementReportRow
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int StockAfter { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? DocumentReference { get; set; }
    public string UserName { get; set; } = string.Empty;
}

public class MovementReportQuery
{
    public int? ProductId { get; set; }
    public int? WarehouseId { get; set; }
    public MovementType? Type { get; set; }
    public string? UserId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public class InventoryValueDto
{
    public int TotalProducts { get; set; }
    public int TotalUnits { get; set; }
    public decimal TotalPurchaseValue { get; set; }
    public decimal TotalSaleValue { get; set; }
    public int CriticalProducts { get; set; }
}

// ───────────────────────── Dashboard ─────────────────────────

public class DashboardDto
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int TotalUnits { get; set; }
    public int Warehouses { get; set; }
    public int Suppliers { get; set; }
    public int Categories { get; set; }
    public decimal InventoryValue { get; set; }
    public int LowStockCount { get; set; }
    public int MovementsToday { get; set; }
    public int EntriesThisMonth { get; set; }
    public int ExitsThisMonth { get; set; }
    public List<MovementDto> RecentMovements { get; set; } = new();
    public List<NotificationDto> LowStockAlerts { get; set; } = new();
}

using SistemaInventario.Core.Enums;

namespace SistemaInventario.Core.Dtos;

public class StockReportRow
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinStock { get; set; }
    public decimal SalePrice { get; set; }
    public decimal StockValue { get; set; }
    public string Status { get; set; } = "ok";
}

public class StockReportFilter
{
    public int? WarehouseId { get; set; }
    public int? CategoryId { get; set; }
    public bool CriticalOnly { get; set; }
    public string? Search { get; set; }
}

public class MovementReportRow
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public int StockAfter { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? DocumentReference { get; set; }
}

public class ReportFile
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class ReportRequest
{
    public int? WarehouseId { get; set; }
    public int? CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public int? ProductId { get; set; }
    public int? UserId { get; set; }
    public MovementType? Type { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Search { get; set; }
    public bool CriticalOnly { get; set; }
    public bool IncludeInactive { get; set; }
}

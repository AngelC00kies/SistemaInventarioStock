namespace SistemaInventario.Core.Dtos;

public class DashboardDto
{
    public int TotalProducts { get; set; }
    public int TotalUnits { get; set; }
    public int LowStockProducts { get; set; }
    public int MovementsToday { get; set; }
    public decimal MonthlyInValue { get; set; }
    public decimal MonthlyOutValue { get; set; }
    public int ActiveWarehouses { get; set; }
    public List<DailyFlowDto> DailyFlow { get; set; } = new();
    public List<CategoryShareDto> CategoryShare { get; set; } = new();
    public List<LowStockDto> CriticalProducts { get; set; } = new();
    public List<MovementDto> RecentMovements { get; set; } = new();
}

public class DailyFlowDto
{
    public string Date { get; set; } = string.Empty;
    public int Entries { get; set; }
    public int Exits { get; set; }
}

public class CategoryShareDto
{
    public string Name { get; set; } = string.Empty;
    public int Products { get; set; }
    public int Units { get; set; }
}

public class LowStockDto
{
    public int ProductId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinStock { get; set; }
    public string Status { get; set; } = "low";
}

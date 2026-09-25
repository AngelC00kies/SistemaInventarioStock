using SistemaInventario.Core.Enums;

namespace SistemaInventario.Core.Entities;

public class Movement
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public MovementType Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int UserId { get; set; }
    public string? DocumentReference { get; set; }
    public int StockAfter { get; set; }
    public decimal UnitPrice { get; set; }

    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public User User { get; set; } = null!;
}

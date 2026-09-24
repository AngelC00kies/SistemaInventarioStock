namespace SistemaInventario.Core.Entities;

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public string Unit { get; set; } = "Unidad";
    public int MinStock { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Category Category { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
    public ICollection<Movement> Movements { get; set; } = new List<Movement>();
}

using InventorySystem.Domain.Common;

namespace InventorySystem.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }

    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
}

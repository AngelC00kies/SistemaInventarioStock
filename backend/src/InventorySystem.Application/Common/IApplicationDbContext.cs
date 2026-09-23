using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Application.Common;

/// <summary>Abstracción del contexto de datos para la capa de aplicación.</summary>
public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Category> Categories { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<Stock> Stocks { get; }
    DbSet<Movement> Movements { get; }
    DbSet<LowStockNotification> LowStockNotifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

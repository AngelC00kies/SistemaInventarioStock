using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Enums;
using SistemaInventario.Core.Interfaces;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardDto> GetAsync(CancellationToken ct = default)
    {
        var products = _db.Products.AsNoTracking().Where(p => p.IsActive);
        var stockLevels = _db.StockLevels.AsNoTracking();
        var movements = _db.Movements.AsNoTracking();

        var now = DateTime.UtcNow;
        var today = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var flowStart = today.AddDays(-29);

        var totalProducts = await products.CountAsync(ct);
        var totalUnits = await stockLevels.SumAsync(s => (long?)s.Quantity, ct) ?? 0;
        var activeWarehouses = await _db.Warehouses.CountAsync(w => w.IsActive, ct);

        var lowStock = await products
            .Where(p => p.StockLevels.Sum(s => s.Quantity) <= p.MinStock)
            .CountAsync(ct);

        var movementsToday = await movements.CountAsync(m => m.Date >= today, ct);

        var monthlyIn = await movements
            .Where(m => m.Type == MovementType.Entrada && m.Date >= monthStart)
            .SumAsync(m => (decimal?)(m.Quantity * m.UnitPrice), ct) ?? 0m;

        var monthlyOut = await movements
            .Where(m => m.Type == MovementType.Salida && m.Date >= monthStart)
            .SumAsync(m => (decimal?)(m.Quantity * m.UnitPrice), ct) ?? 0m;

        var flowData = await movements
            .Where(m => m.Date >= flowStart)
            .GroupBy(m => new { m.Date.Year, m.Date.Month, m.Date.Day })
            .Select(g => new
            {
                Key = g.Key,
                Entries = g.Where(x => x.Type == MovementType.Entrada).Sum(x => x.Quantity),
                Exits = g.Where(x => x.Type == MovementType.Salida).Sum(x => x.Quantity)
            })
            .ToListAsync(ct);

        var dailyFlow = new List<DailyFlowDto>();
        for (var i = 29; i >= 0; i--)
        {
            var day = today.AddDays(-i);
            var match = flowData.FirstOrDefault(f =>
                f.Key.Year == day.Year && f.Key.Month == day.Month && f.Key.Day == day.Day);

            dailyFlow.Add(new DailyFlowDto
            {
                Date = day.ToString("yyyy-MM-dd"),
                Entries = match?.Entries ?? 0,
                Exits = match?.Exits ?? 0
            });
        }

        var categoryProducts = await products
            .GroupBy(p => p.Category.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var categoryUnits = await _db.StockLevels.AsNoTracking()
            .Where(s => s.Product.IsActive)
            .GroupBy(s => s.Product.Category.Name)
            .Select(g => new { Name = g.Key, Units = g.Sum(s => s.Quantity) })
            .ToListAsync(ct);

        var categoryShare = categoryProducts
            .Select(c => new CategoryShareDto
            {
                Name = c.Name,
                Products = c.Count,
                Units = categoryUnits.FirstOrDefault(u => u.Name == c.Name)?.Units ?? 0
            })
            .OrderByDescending(c => c.Units)
            .ThenByDescending(c => c.Products)
            .ToList();

        var critical = await products
            .SelectMany(p => p.StockLevels.Select(s => new
            {
                p.Id,
                p.Code,
                p.Name,
                p.Unit,
                p.MinStock,
                s.Quantity,
                Warehouse = s.Warehouse.Name
            }))
            .Where(x => x.Quantity <= x.MinStock)
            .OrderBy(x => x.Quantity - x.MinStock)
            .Take(8)
            .ToListAsync(ct);

        var criticalProducts = critical
            .Select(x => new LowStockDto
            {
                ProductId = x.Id,
                Code = x.Code,
                Name = x.Name,
                WarehouseName = x.Warehouse,
                Quantity = x.Quantity,
                MinStock = x.MinStock,
                Status = Mapping.ProductStatus(x.Quantity, x.MinStock)
            })
            .ToList();

        var recentMovements = await movements
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .Include(m => m.User)
            .OrderByDescending(m => m.Date)
            .Take(8)
            .Select(m => Mapping.ToDto(m))
            .ToListAsync(ct);

        return new DashboardDto
        {
            TotalProducts = totalProducts,
            TotalUnits = (int)Math.Min(int.MaxValue, totalUnits),
            LowStockProducts = lowStock,
            MovementsToday = movementsToday,
            MonthlyInValue = monthlyIn,
            MonthlyOutValue = monthlyOut,
            ActiveWarehouses = activeWarehouses,
            DailyFlow = dailyFlow,
            CategoryShare = categoryShare,
            CriticalProducts = criticalProducts,
            RecentMovements = recentMovements
        };
    }
}

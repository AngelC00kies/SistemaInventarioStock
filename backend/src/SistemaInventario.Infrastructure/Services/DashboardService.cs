using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Enums;
using SistemaInventario.Core.Interfaces;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Infrastructure.Services;

/// <summary>Agrega los indicadores del panel: contadores, valorización mensual, flujo diario, categorías y productos críticos.</summary>
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardDto> GetAsync(CancellationToken ct = default)
    {
        var products = _db.Productos.AsNoTracking().Where(p => p.Activo);
        var stockLevels = _db.NivelesStock.AsNoTracking();
        var movements = _db.Movimientos.AsNoTracking();

        var now = DateTime.UtcNow;
        var today = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        // Ventanas en UTC: el mes en curso y los últimos 30 días (hoy incluido, por eso -29).
        var flowStart = today.AddDays(-29);

        var totalProducts = await products.CountAsync(ct);
        var totalUnits = await stockLevels.SumAsync(s => (long?)s.Cantidad, ct) ?? 0;
        var activeWarehouses = await _db.Almacenes.CountAsync(w => w.Activo, ct);

        var lowStock = await products
            .Where(p => p.NivelesStock.Sum(s => s.Cantidad) <= p.StockMinimo)
            .CountAsync(ct);

        var movementsToday = await movements.CountAsync(m => m.Fecha >= today, ct);

        // Valorización mensual: Σ(cantidad × precio unitario) con el precio congelado en cada movimiento (compra en entradas, venta en salidas).
        var monthlyIn = await movements
            .Where(m => m.Tipo == MovementType.Entrada && m.Fecha >= monthStart)
            .SumAsync(m => (decimal?)(m.Cantidad * m.PrecioUnitario), ct) ?? 0m;

        var monthlyOut = await movements
            .Where(m => m.Tipo == MovementType.Salida && m.Fecha >= monthStart)
            .SumAsync(m => (decimal?)(m.Cantidad * m.PrecioUnitario), ct) ?? 0m;

        var flowData = await movements
            .Where(m => m.Fecha >= flowStart)
            .GroupBy(m => new { m.Fecha.Year, m.Fecha.Month, m.Fecha.Day })
            .Select(g => new
            {
                Key = g.Key,
                Entries = g.Where(x => x.Tipo == MovementType.Entrada).Sum(x => x.Cantidad),
                Exits = g.Where(x => x.Tipo == MovementType.Salida).Sum(x => x.Cantidad)
            })
            .ToListAsync(ct);

        var dailyFlow = new List<DailyFlowDto>();
        // Se generan los 30 días aunque no haya movimientos, para que la gráfica no tenga huecos.
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
            .GroupBy(p => p.Categoria.Nombre)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var categoryUnits = await _db.NivelesStock.AsNoTracking()
            .Where(s => s.Producto.Activo)
            .GroupBy(s => s.Producto.Categoria.Nombre)
            .Select(g => new { Name = g.Key, Units = g.Sum(s => s.Cantidad) })
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

        // Los 8 déficits más grandes (cantidad − stock mínimo) para la tabla de productos críticos del panel.
        var critical = await products
            .SelectMany(p => p.NivelesStock.Select(s => new
            {
                p.Id,
                p.Codigo,
                p.Nombre,
                p.Unidad,
                p.StockMinimo,
                s.Cantidad,
                Warehouse = s.Almacen.Nombre
            }))
            .Where(x => x.Cantidad <= x.StockMinimo)
            .OrderBy(x => x.Cantidad - x.StockMinimo)
            .Take(8)
            .ToListAsync(ct);

        var criticalProducts = critical
            .Select(x => new LowStockDto
            {
                ProductId = x.Id,
                Code = x.Codigo,
                Name = x.Nombre,
                WarehouseName = x.Warehouse,
                Quantity = x.Cantidad,
                MinStock = x.StockMinimo,
                Status = Mapping.ProductStatus(x.Cantidad, x.StockMinimo)
            })
            .ToList();

        var recentMovements = await movements
            .Include(m => m.Producto)
            .Include(m => m.Almacen)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.Fecha)
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

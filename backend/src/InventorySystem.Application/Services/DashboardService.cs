using InventorySystem.Application.Common;
using InventorySystem.Application.Dtos;
using InventorySystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Application.Services;

/// <summary>KPIs del panel principal: totales, alertas y últimos movimientos.</summary>
public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _db;

    public DashboardService(IApplicationDbContext db) => _db = db;

    public async Task<DashboardDto> GetAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfDay = now.Date;

        var products = _db.Products.AsNoTracking();
        var stocks = _db.Stocks.AsNoTracking();
        var movements = _db.Movements.AsNoTracking();

        var dto = new DashboardDto
        {
            TotalProducts = await products.CountAsync(ct),
            ActiveProducts = await products.CountAsync(p => p.IsActive, ct),
            Warehouses = await _db.Warehouses.CountAsync(w => w.IsActive, ct),
            Suppliers = await _db.Suppliers.CountAsync(s => s.IsActive, ct),
            Categories = await _db.Categories.CountAsync(c => c.IsActive, ct),
            TotalUnits = await stocks.SumAsync(s => (int?)s.Quantity, ct) ?? 0,
            MovementsToday = await movements.CountAsync(m => m.Date >= startOfDay, ct),
            EntriesThisMonth = await movements.CountAsync(m => m.Date >= startOfMonth && m.Type == Domain.Enums.MovementType.Entrada, ct),
            ExitsThisMonth = await movements.CountAsync(m => m.Date >= startOfMonth && m.Type == Domain.Enums.MovementType.Salida, ct)
        };

        dto.InventoryValue = await products
            .Where(p => p.IsActive)
            .Select(p => new
            {
                p.PurchasePrice,
                Qty = p.Stocks.Sum(s => s.Quantity)
            })
            .ToListAsync(ct)
            .ContinueWith(t => t.Result.Sum(x => x.PurchasePrice * x.Qty), ct);

        // Productos con stock total <= mínimo (y mínimo configurado)
        var critical = await products
            .Where(p => p.IsActive && p.MinimumStock > 0)
            .Select(p => new { p.Id, p.MinimumStock, Qty = p.Stocks.Sum(s => s.Quantity) })
            .ToListAsync(ct);

        dto.LowStockCount = critical.Count(x => x.Qty <= x.MinimumStock);

        dto.RecentMovements = await movements
            .OrderByDescending(m => m.Date)
            .Take(8)
            .Select(m => new MovementDto
            {
                Id = m.Id,
                Date = m.Date,
                Type = m.Type,
                Reason = m.Reason,
                Quantity = m.Quantity,
                StockAfter = m.StockAfter,
                DocumentReference = m.DocumentReference,
                ProductId = m.ProductId,
                ProductCode = m.Product.Code,
                ProductName = m.Product.Name,
                WarehouseId = m.WarehouseId,
                WarehouseName = m.Warehouse.Name,
                UserId = m.UserId,
                UserName = m.UserName
            })
            .ToListAsync(ct);

        dto.LowStockAlerts = await _db.LowStockNotifications.AsNoTracking()
            .Where(n => !n.IsResolved)
            .OrderByDescending(n => n.DetectedAt)
            .Take(8)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                ProductId = n.ProductId,
                ProductCode = n.Product.Code,
                ProductName = n.Product.Name,
                WarehouseId = n.WarehouseId,
                WarehouseName = n.Warehouse.Name,
                QuantityAtDetection = n.QuantityAtDetection,
                MinimumStock = n.MinimumStock,
                DetectedAt = n.DetectedAt,
                IsResolved = n.IsResolved,
                ResolvedAt = n.ResolvedAt
            })
            .ToListAsync(ct);

        return dto;
    }
}

using InventorySystem.Application.Common;
using InventorySystem.Application.Dtos;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _db;

    public NotificationService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<NotificationDto>> GetAsync(bool pendingOnly, CancellationToken ct = default)
    {
        var q = _db.LowStockNotifications.AsNoTracking();

        if (pendingOnly)
            q = q.Where(n => !n.IsResolved);

        return await q
            .OrderBy(n => n.IsResolved)
            .ThenByDescending(n => n.DetectedAt)
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
    }

    public async Task AcknowledgeAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.LowStockNotifications.FirstOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new NotFoundException($"NotificaciÃ³n {id} no encontrada.");

        entity.IsResolved = true;
        entity.ResolvedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public Task<int> GetPendingCountAsync(CancellationToken ct = default) =>
        _db.LowStockNotifications.CountAsync(n => !n.IsResolved, ct);
}

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

        // Productos con stock total <= mÃ­nimo (y mÃ­nimo configurado)
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

public class ReportService : IReportService
{
    private readonly IApplicationDbContext _db;

    public ReportService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<StockReportRow>> GetStockReportAsync(StockReportQuery query, CancellationToken ct = default)
    {
        var stocks = _db.Stocks.AsNoTracking()
            .Include(s => s.Product).ThenInclude(p => p.Category)
            .Include(s => s.Product).ThenInclude(p => p.Supplier)
            .Include(s => s.Warehouse)
            .AsQueryable();

        if (query.CategoryId.HasValue)
            stocks = stocks.Where(s => s.Product.CategoryId == query.CategoryId);

        if (query.WarehouseId.HasValue)
            stocks = stocks.Where(s => s.WarehouseId == query.WarehouseId);

        stocks = query.CriticalOnly
            ? stocks.Where(s => s.Product.MinimumStock > 0 && s.Quantity <= s.Product.MinimumStock)
            : stocks.Where(s => s.Quantity > 0 || s.Product.MinimumStock > 0);

        var rows = await stocks
            .OrderBy(s => s.Product.Name)
            .ThenBy(s => s.Warehouse.Name)
            .Select(s => new
            {
                s.Quantity,
                s.Product.Code,
                s.Product.Name,
                s.Product.MinimumStock,
                s.Product.UnitOfMeasure,
                s.Product.PurchasePrice,
                s.Product.SalePrice,
                Category = s.Product.Category.Name,
                Supplier = s.Product.Supplier != null ? s.Product.Supplier.Name : null,
                Warehouse = s.Warehouse.Name
            })
            .ToListAsync(ct);

        return rows.Select(r => new StockReportRow
        {
            ProductCode = r.Code,
            ProductName = r.Name,
            Category = r.Category,
            Supplier = r.Supplier,
            Warehouse = r.Warehouse,
            Quantity = r.Quantity,
            MinimumStock = r.MinimumStock,
            UnitOfMeasure = r.UnitOfMeasure,
            PurchasePrice = r.PurchasePrice,
            SalePrice = r.SalePrice,
            StockValue = r.PurchasePrice * r.Quantity,
            Status = GetStatus(r.Quantity, r.MinimumStock)
        }).ToList();
    }

    public async Task<IReadOnlyList<MovementReportRow>> GetMovementsReportAsync(MovementReportQuery query, CancellationToken ct = default)
    {
        var q = _db.Movements.AsNoTracking();

        if (query.ProductId.HasValue)
            q = q.Where(m => m.ProductId == query.ProductId);

        if (query.WarehouseId.HasValue)
            q = q.Where(m => m.WarehouseId == query.WarehouseId);

        if (query.Type.HasValue)
            q = q.Where(m => m.Type == query.Type);

        if (!string.IsNullOrWhiteSpace(query.UserId))
            q = q.Where(m => m.UserId == query.UserId);

        if (query.From.HasValue)
            q = q.Where(m => m.Date >= query.From.Value.ToUniversalTime());

        if (query.To.HasValue)
        {
            var to = query.To.Value.ToUniversalTime().Date.AddDays(1);
            q = q.Where(m => m.Date < to);
        }

        return await q
            .OrderByDescending(m => m.Date)
            .Select(m => new MovementReportRow
            {
                Date = m.Date,
                Type = m.Type.ToString(),
                ProductCode = m.Product.Code,
                ProductName = m.Product.Name,
                Warehouse = m.Warehouse.Name,
                Quantity = m.Quantity,
                StockAfter = m.StockAfter,
                Reason = m.Reason,
                DocumentReference = m.DocumentReference,
                UserName = m.UserName
            })
            .ToListAsync(ct);
    }

    public async Task<InventoryValueDto> GetInventoryValueAsync(CancellationToken ct = default)
    {
        var products = await _db.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new
            {
                p.PurchasePrice,
                p.SalePrice,
                p.MinimumStock,
                Qty = p.Stocks.Sum(s => s.Quantity)
            })
            .ToListAsync(ct);

        return new InventoryValueDto
        {
            TotalProducts = products.Count,
            TotalUnits = products.Sum(p => p.Qty),
            TotalPurchaseValue = products.Sum(p => p.PurchasePrice * p.Qty),
            TotalSaleValue = products.Sum(p => p.SalePrice * p.Qty),
            CriticalProducts = products.Count(p => p.MinimumStock > 0 && p.Qty <= p.MinimumStock)
        };
    }

    private static string GetStatus(int quantity, int minimumStock)
    {
        if (minimumStock <= 0)
            return quantity > 0 ? "Disponible" : "Sin stock";

        if (quantity <= 0)
            return "CrÃ­tico";
        if (quantity <= minimumStock)
            return "Stock bajo";
        if (quantity <= minimumStock * 1.5)
            return "Ajustado";

        return "Disponible";
    }
}

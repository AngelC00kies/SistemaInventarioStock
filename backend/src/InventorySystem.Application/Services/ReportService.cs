using InventorySystem.Application.Common;
using InventorySystem.Application.Dtos;
using InventorySystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Application.Services;

/// <summary>Consultas de reportes: stock, movimientos y valorización.</summary>
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
            return "Crítico";
        if (quantity <= minimumStock)
            return "Stock bajo";
        if (quantity <= minimumStock * 1.5)
            return "Ajustado";

        return "Disponible";
    }
}

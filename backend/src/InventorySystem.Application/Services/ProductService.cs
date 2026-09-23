using InventorySystem.Application.Common;
using InventorySystem.Application.Dtos;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Application.Services;

public class ProductService : IProductService
{
    private readonly IApplicationDbContext _db;

    public ProductService(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<ProductDto>> GetAsync(ProductQuery query, CancellationToken ct = default)
    {
        var q = _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!query.IncludeInactive)
            q = q.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(p => p.Name.Contains(term) || p.Code.Contains(term) ||
                             (p.Description != null && p.Description.Contains(term)));
        }

        if (query.CategoryId.HasValue)
            q = q.Where(p => p.CategoryId == query.CategoryId);

        if (query.SupplierId.HasValue)
            q = q.Where(p => p.SupplierId == query.SupplierId);

        if (query.WarehouseId.HasValue)
            q = q.Where(p => p.Stocks.Any(s => s.WarehouseId == query.WarehouseId));

        var filtered = q.Select(p => new
        {
            Product = p,
            TotalStock = p.Stocks.Sum(s => s.Quantity)
        });

        if (query.OutOfStockOnly)
            filtered = filtered.Where(x => x.TotalStock == 0);

        if (query.LowStockOnly)
            filtered = filtered.Where(x => x.TotalStock <= x.Product.MinimumStock);

        var total = await filtered.CountAsync(ct);

        var rows = await filtered
            .OrderBy(x => x.Product.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new
            {
                x.Product,
                x.TotalStock,
                CategoryName = x.Product.Category.Name,
                SupplierName = x.Product.Supplier != null ? x.Product.Supplier.Name : null
            })
            .ToListAsync(ct);

        var items = rows.Select(x => Map(x.Product, x.TotalStock, x.CategoryName, x.SupplierName)).ToList();

        return new PagedResult<ProductDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<ProductDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var product = await _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException($"Producto {id} no encontrado.");

        var total = await _db.Stocks.AsNoTracking()
            .Where(s => s.ProductId == id)
            .SumAsync(s => (int?)s.Quantity, ct) ?? 0;

        return Map(product, total, product.Category.Name, product.Supplier?.Name);
    }

    public async Task<ProductDto> CreateAsync(ProductRequest request, CancellationToken ct = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();

        if (await _db.Products.AnyAsync(p => p.Code == code, ct))
            throw new AppException($"Ya existe un producto con el código '{code}'.");

        await EnsureCategoryAsync(request.CategoryId, ct);
        await EnsureSupplierAsync(request.SupplierId, ct);

        var entity = new Product
        {
            Code = code,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CategoryId = request.CategoryId,
            SupplierId = request.SupplierId,
            UnitOfMeasure = request.UnitOfMeasure,
            PurchasePrice = request.PurchasePrice,
            SalePrice = request.SalePrice,
            MinimumStock = request.MinimumStock
        };

        _db.Products.Add(entity);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<ProductDto> UpdateAsync(int id, ProductRequest request, CancellationToken ct = default)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException($"Producto {id} no encontrado.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (await _db.Products.AnyAsync(p => p.Code == code && p.Id != id, ct))
            throw new AppException($"Ya existe un producto con el código '{code}'.");

        await EnsureCategoryAsync(request.CategoryId, ct);
        await EnsureSupplierAsync(request.SupplierId, ct);

        entity.Code = code;
        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.CategoryId = request.CategoryId;
        entity.SupplierId = request.SupplierId;
        entity.UnitOfMeasure = request.UnitOfMeasure;
        entity.PurchasePrice = request.PurchasePrice;
        entity.SalePrice = request.SalePrice;
        entity.MinimumStock = request.MinimumStock;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException($"Producto {id} no encontrado.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<StockEntryDto>> GetStockAsync(int id, CancellationToken ct = default)
    {
        _ = await _db.Products.AnyAsync(p => p.Id == id, ct)
            ? true
            : throw new NotFoundException($"Producto {id} no encontrado.");

        return await _db.Stocks.AsNoTracking()
            .Where(s => s.ProductId == id)
            .OrderByDescending(s => s.Quantity)
            .Select(s => new StockEntryDto
            {
                WarehouseId = s.WarehouseId,
                WarehouseCode = s.Warehouse.Code,
                WarehouseName = s.Warehouse.Name,
                Quantity = s.Quantity,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync(ct);
    }

    private async Task EnsureCategoryAsync(int categoryId, CancellationToken ct)
    {
        if (!await _db.Categories.AnyAsync(c => c.Id == categoryId && c.IsActive, ct))
            throw new AppException("La categoría seleccionada no existe o está inactiva.");
    }

    private async Task EnsureSupplierAsync(int? supplierId, CancellationToken ct)
    {
        if (supplierId.HasValue &&
            !await _db.Suppliers.AnyAsync(s => s.Id == supplierId && s.IsActive, ct))
            throw new AppException("El proveedor seleccionado no existe o está inactivo.");
    }

    private static ProductDto Map(Product p, int totalStock, string categoryName, string? supplierName) => new()
    {
        Id = p.Id,
        Code = p.Code,
        Name = p.Name,
        Description = p.Description,
        CategoryId = p.CategoryId,
        CategoryName = categoryName,
        SupplierId = p.SupplierId,
        SupplierName = supplierName,
        UnitOfMeasure = p.UnitOfMeasure,
        PurchasePrice = p.PurchasePrice,
        SalePrice = p.SalePrice,
        MinimumStock = p.MinimumStock,
        TotalStock = totalStock,
        IsLowStock = totalStock <= p.MinimumStock,
        IsActive = p.IsActive
    };
}

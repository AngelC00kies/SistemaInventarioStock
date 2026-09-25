using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Common;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Core.Interfaces;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db) => _db = db;

    private IQueryable<Core.Entities.Product> Query() =>
        _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.StockLevels).ThenInclude(s => s.Warehouse);

    public async Task<PagedResult<ProductDto>> GetAsync(ProductFilter filter, CancellationToken ct = default)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);

        var query = Query();

        if (!filter.IncludeInactive.GetValueOrDefault())
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(p =>
                p.Code.ToLower().Contains(term) ||
                p.Name.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (filter.SupplierId.HasValue)
            query = query.Where(p => p.SupplierId == filter.SupplierId.Value);

        if (filter.WarehouseId.HasValue)
            query = query.Where(p => p.StockLevels.Any(s => s.WarehouseId == filter.WarehouseId.Value));

        if (filter.OnlyLowStock.GetValueOrDefault())
            query = query.Where(p => p.StockLevels.Sum(s => s.Quantity) <= p.MinStock);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = items.Select(p => Mapping.ToDto(p, filter.WarehouseId)).ToList();

        return new PagedResult<ProductDto>
        {
            Items = dtos,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductDto> GetAsync(int id, CancellationToken ct = default)
    {
        var product = await Query().FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Producto no encontrado.");
        return Mapping.ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(ProductRequest request, CancellationToken ct = default)
    {
        await ValidateAsync(request, null, ct);

        var product = new Core.Entities.Product
        {
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CategoryId = request.CategoryId,
            SupplierId = request.SupplierId,
            PurchasePrice = request.PurchasePrice,
            SalePrice = request.SalePrice,
            Unit = string.IsNullOrWhiteSpace(request.Unit) ? "Unidad" : request.Unit.Trim(),
            MinStock = request.MinStock,
            IsActive = request.IsActive
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return await GetAsync(product.Id, ct);
    }

    public async Task<ProductDto> UpdateAsync(int id, ProductRequest request, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Producto no encontrado.");

        await ValidateAsync(request, id, ct);

        product.Code = request.Code.Trim().ToUpperInvariant();
        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.CategoryId = request.CategoryId;
        product.SupplierId = request.SupplierId;
        product.PurchasePrice = request.PurchasePrice;
        product.SalePrice = request.SalePrice;
        product.Unit = string.IsNullOrWhiteSpace(request.Unit) ? "Unidad" : request.Unit.Trim();
        product.MinStock = request.MinStock;
        product.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Producto no encontrado.");

        if (await _db.Movements.AnyAsync(m => m.ProductId == id, ct))
        {
            product.IsActive = false;
        }
        else
        {
            _db.Products.Remove(product);
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task ValidateAsync(ProductRequest request, int? currentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new AppException("El código del producto es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new AppException("El nombre del producto es obligatorio.");
        if (request.PurchasePrice < 0 || request.SalePrice < 0)
            throw new AppException("Los precios no pueden ser negativos.");
        if (request.SalePrice < request.PurchasePrice)
            throw new AppException("El precio de venta no puede ser menor al precio de compra.");
        if (request.MinStock < 0)
            throw new AppException("El stock mínimo no puede ser negativo.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (await _db.Products.AnyAsync(p => p.Code == code && p.Id != currentId, ct))
            throw new AppException($"Ya existe un producto con el código \"{code}\".");

        if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId && c.IsActive, ct))
            throw new AppException("La categoría seleccionada no es válida.");

        if (request.SupplierId.HasValue &&
            !await _db.Suppliers.AnyAsync(s => s.Id == request.SupplierId.Value && s.IsActive, ct))
            throw new AppException("El proveedor seleccionado no es válido.");
    }
}

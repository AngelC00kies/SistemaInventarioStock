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

    private IQueryable<Core.Entities.Producto> Query() =>
        _db.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
            .Include(p => p.NivelesStock).ThenInclude(s => s.Almacen);

    public async Task<PagedResult<ProductDto>> GetAsync(ProductFilter filter, CancellationToken ct = default)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);

        var query = Query();

        if (!filter.IncludeInactive.GetValueOrDefault())
            query = query.Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(p =>
                p.Codigo.ToLower().Contains(term) ||
                p.Nombre.ToLower().Contains(term) ||
                (p.Descripcion != null && p.Descripcion.ToLower().Contains(term)));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoriaId == filter.CategoryId.Value);

        if (filter.SupplierId.HasValue)
            query = query.Where(p => p.ProveedorId == filter.SupplierId.Value);

        if (filter.WarehouseId.HasValue)
            query = query.Where(p => p.NivelesStock.Any(s => s.AlmacenId == filter.WarehouseId.Value));

        if (filter.OnlyLowStock.GetValueOrDefault())
            query = query.Where(p => p.NivelesStock.Sum(s => s.Cantidad) <= p.StockMinimo);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Codigo)
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

        var product = new Core.Entities.Producto
        {
            Codigo = request.Code.Trim().ToUpperInvariant(),
            Nombre = request.Name.Trim(),
            Descripcion = request.Description?.Trim(),
            CategoriaId = request.CategoryId,
            ProveedorId = request.SupplierId,
            PrecioCompra = request.PurchasePrice,
            PrecioVenta = request.SalePrice,
            Unidad = string.IsNullOrWhiteSpace(request.Unit) ? "Unidad" : request.Unit.Trim(),
            StockMinimo = request.MinStock,
            Activo = request.IsActive
        };

        _db.Productos.Add(product);
        await _db.SaveChangesAsync(ct);
        return await GetAsync(product.Id, ct);
    }

    public async Task<ProductDto> UpdateAsync(int id, ProductRequest request, CancellationToken ct = default)
    {
        var product = await _db.Productos.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Producto no encontrado.");

        await ValidateAsync(request, id, ct);

        product.Codigo = request.Code.Trim().ToUpperInvariant();
        product.Nombre = request.Name.Trim();
        product.Descripcion = request.Description?.Trim();
        product.CategoriaId = request.CategoryId;
        product.ProveedorId = request.SupplierId;
        product.PrecioCompra = request.PurchasePrice;
        product.PrecioVenta = request.SalePrice;
        product.Unidad = string.IsNullOrWhiteSpace(request.Unit) ? "Unidad" : request.Unit.Trim();
        product.StockMinimo = request.MinStock;
        product.Activo = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await _db.Productos.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Producto no encontrado.");

        if (await _db.Movimientos.AnyAsync(m => m.ProductoId == id, ct))
        {
            product.Activo = false;
        }
        else
        {
            _db.Productos.Remove(product);
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
        if (await _db.Productos.AnyAsync(p => p.Codigo == code && p.Id != currentId, ct))
            throw new AppException($"Ya existe un producto con el código \"{code}\".");

        if (!await _db.Categorias.AnyAsync(c => c.Id == request.CategoryId && c.Activo, ct))
            throw new AppException("La categoría seleccionada no es válida.");

        if (request.SupplierId.HasValue &&
            !await _db.Proveedores.AnyAsync(s => s.Id == request.SupplierId.Value && s.Activo, ct))
            throw new AppException("El proveedor seleccionado no es válido.");
    }
}

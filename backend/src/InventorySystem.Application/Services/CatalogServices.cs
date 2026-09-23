using InventorySystem.Application.Common;
using InventorySystem.Application.Dtos;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IApplicationDbContext _db;

    public CategoryService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<CategoryDto>> GetAsync(bool includeInactive, CancellationToken ct = default)
    {
        var query = _db.Categories.AsNoTracking();

        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .ToListAsync(ct);
    }

    public async Task<CategoryDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var dto = await _db.Categories.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .FirstOrDefaultAsync(ct);

        return dto ?? throw new NotFoundException($"Categoría {id} no encontrada.");
    }

    public async Task<CategoryDto> CreateAsync(CategoryRequest request, CancellationToken ct = default)
    {
        await EnsureUniqueNameAsync(request.Name, null, ct);

        var entity = new Category { Name = request.Name.Trim(), Description = request.Description?.Trim() };
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<CategoryDto> UpdateAsync(int id, CategoryRequest request, CancellationToken ct = default)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException($"Categoría {id} no encontrada.");

        await EnsureUniqueNameAsync(request.Name, id, ct);

        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Categories.Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException($"Categoría {id} no encontrada.");

        if (entity.Products.Any(p => p.IsActive))
            throw new AppException("No se puede eliminar: la categoría tiene productos activos asociados.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureUniqueNameAsync(string name, int? currentId, CancellationToken ct)
    {
        var normalized = name.Trim();
        var exists = await _db.Categories.AnyAsync(
            c => c.Name == normalized && (!currentId.HasValue || c.Id != currentId), ct);

        if (exists)
            throw new AppException($"Ya existe una categoría con el nombre '{normalized}'.");
    }
}

public class SupplierService : ISupplierService
{
    private readonly IApplicationDbContext _db;

    public SupplierService(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<SupplierDto>> GetAsync(PagedQuery query, CancellationToken ct = default)
    {
        var q = _db.Suppliers.AsNoTracking();

        if (!query.IncludeInactive)
            q = q.Where(s => s.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(s => s.Name.Contains(term) ||
                             (s.ContactName != null && s.ContactName.Contains(term)) ||
                             (s.Email != null && s.Email.Contains(term)));
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderBy(s => s.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                ContactName = s.ContactName,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                IsActive = s.IsActive,
                ProductCount = s.Products.Count(p => p.IsActive)
            })
            .ToListAsync(ct);

        return new PagedResult<SupplierDto> { Items = items, TotalCount = total, Page = query.Page, PageSize = query.PageSize };
    }

    public async Task<SupplierDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var dto = await _db.Suppliers.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                ContactName = s.ContactName,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                IsActive = s.IsActive,
                ProductCount = s.Products.Count(p => p.IsActive)
            })
            .FirstOrDefaultAsync(ct);

        return dto ?? throw new NotFoundException($"Proveedor {id} no encontrado.");
    }

    public async Task<SupplierDto> CreateAsync(SupplierRequest request, CancellationToken ct = default)
    {
        await EnsureUniqueNameAsync(request.Name, null, ct);

        var entity = new Supplier
        {
            Name = request.Name.Trim(),
            ContactName = request.ContactName?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            Address = request.Address?.Trim()
        };

        _db.Suppliers.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<SupplierDto> UpdateAsync(int id, SupplierRequest request, CancellationToken ct = default)
    {
        var entity = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException($"Proveedor {id} no encontrado.");

        await EnsureUniqueNameAsync(request.Name, id, ct);

        entity.Name = request.Name.Trim();
        entity.ContactName = request.ContactName?.Trim();
        entity.Phone = request.Phone?.Trim();
        entity.Email = request.Email?.Trim();
        entity.Address = request.Address?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Suppliers.Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException($"Proveedor {id} no encontrado.");

        if (entity.Products.Any(p => p.IsActive))
            throw new AppException("No se puede eliminar: el proveedor tiene productos activos asociados.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureUniqueNameAsync(string name, int? currentId, CancellationToken ct)
    {
        var normalized = name.Trim();
        var exists = await _db.Suppliers.AnyAsync(
            s => s.Name == normalized && (!currentId.HasValue || s.Id != currentId), ct);

        if (exists)
            throw new AppException($"Ya existe un proveedor con el nombre '{normalized}'.");
    }
}

public class WarehouseService : IWarehouseService
{
    private readonly IApplicationDbContext _db;

    public WarehouseService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<WarehouseDto>> GetAsync(bool includeInactive, CancellationToken ct = default)
    {
        var query = _db.Warehouses.AsNoTracking();

        if (!includeInactive)
            query = query.Where(w => w.IsActive);

        return await query
            .OrderBy(w => w.Name)
            .Select(w => new WarehouseDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name,
                Address = w.Address,
                IsActive = w.IsActive,
                ProductCount = w.Stocks.Count(s => s.Quantity > 0)
            })
            .ToListAsync(ct);
    }

    public async Task<WarehouseDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var dto = await _db.Warehouses.AsNoTracking()
            .Where(w => w.Id == id)
            .Select(w => new WarehouseDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name,
                Address = w.Address,
                IsActive = w.IsActive,
                ProductCount = w.Stocks.Count(s => s.Quantity > 0)
            })
            .FirstOrDefaultAsync(ct);

        return dto ?? throw new NotFoundException($"Almacén {id} no encontrado.");
    }

    public async Task<WarehouseDto> CreateAsync(WarehouseRequest request, CancellationToken ct = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await _db.Warehouses.AnyAsync(w => w.Code == code, ct))
            throw new AppException($"Ya existe un almacén con el código '{code}'.");

        var entity = new Warehouse { Code = code, Name = request.Name.Trim(), Address = request.Address?.Trim() };
        _db.Warehouses.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<WarehouseDto> UpdateAsync(int id, WarehouseRequest request, CancellationToken ct = default)
    {
        var entity = await _db.Warehouses.FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new NotFoundException($"Almacén {id} no encontrado.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (await _db.Warehouses.AnyAsync(w => w.Code == code && w.Id != id, ct))
            throw new AppException($"Ya existe un almacén con el código '{code}'.");

        entity.Code = code;
        entity.Name = request.Name.Trim();
        entity.Address = request.Address?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Warehouses.Include(w => w.Stocks)
            .FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new NotFoundException($"Almacén {id} no encontrado.");

        if (entity.Stocks.Any(s => s.Quantity > 0))
            throw new AppException("No se puede eliminar: el almacén contiene stock.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}

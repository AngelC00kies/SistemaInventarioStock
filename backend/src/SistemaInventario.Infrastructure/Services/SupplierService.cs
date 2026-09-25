using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Core.Interfaces;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly AppDbContext _db;

    public SupplierService(AppDbContext db) => _db = db;

    public async Task<List<SupplierDto>> GetAsync(string? search, bool includeInactive, CancellationToken ct = default)
    {
        var query = _db.Suppliers.AsNoTracking().Include(s => s.Products).AsQueryable();

        if (!includeInactive) query = query.Where(s => s.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(term) ||
                                     (s.ContactName != null && s.ContactName.ToLower().Contains(term)));
        }

        var items = await query.OrderBy(s => s.Name).ToListAsync(ct);
        return items.Select(Mapping.ToDto).ToList();
    }

    public async Task<SupplierDto> GetAsync(int id, CancellationToken ct = default)
    {
        var supplier = await _db.Suppliers.AsNoTracking().Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("Proveedor no encontrado.");
        return Mapping.ToDto(supplier);
    }

    public async Task<SupplierDto> CreateAsync(SupplierRequest request, CancellationToken ct = default)
    {
        await ValidateAsync(request, null, ct);

        var supplier = new Core.Entities.Supplier
        {
            Name = request.Name.Trim(),
            ContactName = request.ContactName?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            Address = request.Address?.Trim(),
            IsActive = request.IsActive
        };

        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync(ct);
        return await GetAsync(supplier.Id, ct);
    }

    public async Task<SupplierDto> UpdateAsync(int id, SupplierRequest request, CancellationToken ct = default)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("Proveedor no encontrado.");

        await ValidateAsync(request, id, ct);

        supplier.Name = request.Name.Trim();
        supplier.ContactName = request.ContactName?.Trim();
        supplier.Phone = request.Phone?.Trim();
        supplier.Email = request.Email?.Trim();
        supplier.Address = request.Address?.Trim();
        supplier.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("Proveedor no encontrado.");

        if (await _db.Products.AnyAsync(p => p.SupplierId == id, ct))
        {
            supplier.IsActive = false;
        }
        else
        {
            _db.Suppliers.Remove(supplier);
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task ValidateAsync(SupplierRequest request, int? currentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new AppException("El nombre del proveedor es obligatorio.");

        var name = request.Name.Trim().ToLower();
        if (await _db.Suppliers.AnyAsync(s => s.Name.ToLower() == name && s.Id != currentId, ct))
            throw new AppException($"Ya existe un proveedor llamado \"{request.Name.Trim()}\".");

        if (!string.IsNullOrWhiteSpace(request.Email) && !request.Email.Contains('@'))
            throw new AppException("El correo electrónico no es válido.");
    }
}

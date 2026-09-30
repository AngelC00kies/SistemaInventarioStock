using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Core.Interfaces;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Infrastructure.Services;

/// <summary>CRUD de proveedores con unicidad de nombre y borrado protegido frente a productos asociados.</summary>
public class SupplierService : ISupplierService
{
    private readonly AppDbContext _db;

    public SupplierService(AppDbContext db) => _db = db;

    public async Task<List<SupplierDto>> GetAsync(string? search, bool includeInactive, CancellationToken ct = default)
    {
        var query = _db.Proveedores.AsNoTracking().Include(s => s.Productos).AsQueryable();

        if (!includeInactive) query = query.Where(s => s.Activo);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s => s.Nombre.ToLower().Contains(term) ||
                                     (s.NombreContacto != null && s.NombreContacto.ToLower().Contains(term)));
        }

        var items = await query.OrderBy(s => s.Nombre).ToListAsync(ct);
        return items.Select(Mapping.ToDto).ToList();
    }

    public async Task<SupplierDto> GetAsync(int id, CancellationToken ct = default)
    {
        var supplier = await _db.Proveedores.AsNoTracking().Include(s => s.Productos)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("Proveedor no encontrado.");
        return Mapping.ToDto(supplier);
    }

    public async Task<SupplierDto> CreateAsync(SupplierRequest request, CancellationToken ct = default)
    {
        await ValidateAsync(request, null, ct);

        var supplier = new Core.Entities.Proveedor
        {
            Nombre = request.Name.Trim(),
            NombreContacto = request.ContactName?.Trim(),
            Telefono = request.Phone?.Trim(),
            Correo = request.Email?.Trim(),
            Direccion = request.Address?.Trim(),
            Activo = request.IsActive
        };

        _db.Proveedores.Add(supplier);
        await _db.SaveChangesAsync(ct);
        return await GetAsync(supplier.Id, ct);
    }

    public async Task<SupplierDto> UpdateAsync(int id, SupplierRequest request, CancellationToken ct = default)
    {
        var supplier = await _db.Proveedores.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("Proveedor no encontrado.");

        await ValidateAsync(request, id, ct);

        supplier.Nombre = request.Name.Trim();
        supplier.NombreContacto = request.ContactName?.Trim();
        supplier.Telefono = request.Phone?.Trim();
        supplier.Correo = request.Email?.Trim();
        supplier.Direccion = request.Address?.Trim();
        supplier.Activo = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var supplier = await _db.Proveedores.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("Proveedor no encontrado.");

        // Con productos asociados sólo se desactiva, para no perder el vínculo con lo que ya se compró a ese proveedor.
        if (await _db.Productos.AnyAsync(p => p.ProveedorId == id, ct))
        {
            supplier.Activo = false;
        }
        else
        {
            _db.Proveedores.Remove(supplier);
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task ValidateAsync(SupplierRequest request, int? currentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new AppException("El nombre del proveedor es obligatorio.");

        // Unicidad del nombre sin distinguir mayúsculas: ambas partes se comparan en minúsculas.
        var name = request.Name.Trim().ToLower();
        if (await _db.Proveedores.AnyAsync(s => s.Nombre.ToLower() == name && s.Id != currentId, ct))
            throw new AppException($"Ya existe un proveedor llamado \"{request.Name.Trim()}\".");

        if (!string.IsNullOrWhiteSpace(request.Email) && !request.Email.Contains('@'))
            throw new AppException("El correo electrónico no es válido.");
    }
}

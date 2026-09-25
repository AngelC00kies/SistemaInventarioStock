using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Core.Interfaces;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Infrastructure.Services;

public class WarehouseService : IWarehouseService
{
    private readonly AppDbContext _db;

    public WarehouseService(AppDbContext db) => _db = db;

    public async Task<List<WarehouseDto>> GetAsync(string? search, bool includeInactive, CancellationToken ct = default)
    {
        var query = _db.Almacenes.AsNoTracking().Include(w => w.NivelesStock).AsQueryable();

        if (!includeInactive) query = query.Where(w => w.Activo);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(w => w.Nombre.ToLower().Contains(term) || w.Codigo.ToLower().Contains(term));
        }

        var items = await query.OrderBy(w => w.Codigo).ToListAsync(ct);
        return items.Select(Mapping.ToDto).ToList();
    }

    public async Task<WarehouseDto> GetAsync(int id, CancellationToken ct = default)
    {
        var warehouse = await _db.Almacenes.AsNoTracking().Include(w => w.NivelesStock)
            .FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new NotFoundException("Almacén no encontrado.");
        return Mapping.ToDto(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(WarehouseRequest request, CancellationToken ct = default)
    {
        await ValidateAsync(request, null, ct);

        var warehouse = new Core.Entities.Almacen
        {
            Nombre = request.Name.Trim(),
            Codigo = request.Code.Trim().ToUpperInvariant(),
            Ubicacion = request.Location?.Trim(),
            Activo = request.IsActive
        };

        _db.Almacenes.Add(warehouse);
        await _db.SaveChangesAsync(ct);
        return await GetAsync(warehouse.Id, ct);
    }

    public async Task<WarehouseDto> UpdateAsync(int id, WarehouseRequest request, CancellationToken ct = default)
    {
        var warehouse = await _db.Almacenes.FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new NotFoundException("Almacén no encontrado.");

        await ValidateAsync(request, id, ct);

        warehouse.Nombre = request.Name.Trim();
        warehouse.Codigo = request.Code.Trim().ToUpperInvariant();
        warehouse.Ubicacion = request.Location?.Trim();
        warehouse.Activo = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var warehouse = await _db.Almacenes.FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new NotFoundException("Almacén no encontrado.");

        if (await _db.Movimientos.AnyAsync(m => m.AlmacenId == id, ct) ||
            await _db.NivelesStock.AnyAsync(s => s.AlmacenId == id && s.Cantidad > 0, ct))
        {
            warehouse.Activo = false;
        }
        else
        {
            _db.Almacenes.Remove(warehouse);
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task ValidateAsync(WarehouseRequest request, int? currentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new AppException("El nombre del almacén es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new AppException("El código del almacén es obligatorio.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (await _db.Almacenes.AnyAsync(w => w.Codigo == code && w.Id != currentId, ct))
            throw new AppException($"Ya existe un almacén con el código \"{code}\".");
    }
}

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
        var query = _db.Warehouses.AsNoTracking().Include(w => w.StockLevels).AsQueryable();

        if (!includeInactive) query = query.Where(w => w.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(w => w.Name.ToLower().Contains(term) || w.Code.ToLower().Contains(term));
        }

        var items = await query.OrderBy(w => w.Code).ToListAsync(ct);
        return items.Select(Mapping.ToDto).ToList();
    }

    public async Task<WarehouseDto> GetAsync(int id, CancellationToken ct = default)
    {
        var warehouse = await _db.Warehouses.AsNoTracking().Include(w => w.StockLevels)
            .FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new NotFoundException("Almacén no encontrado.");
        return Mapping.ToDto(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(WarehouseRequest request, CancellationToken ct = default)
    {
        await ValidateAsync(request, null, ct);

        var warehouse = new Core.Entities.Warehouse
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpperInvariant(),
            Location = request.Location?.Trim(),
            IsActive = request.IsActive
        };

        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync(ct);
        return await GetAsync(warehouse.Id, ct);
    }

    public async Task<WarehouseDto> UpdateAsync(int id, WarehouseRequest request, CancellationToken ct = default)
    {
        var warehouse = await _db.Warehouses.FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new NotFoundException("Almacén no encontrado.");

        await ValidateAsync(request, id, ct);

        warehouse.Name = request.Name.Trim();
        warehouse.Code = request.Code.Trim().ToUpperInvariant();
        warehouse.Location = request.Location?.Trim();
        warehouse.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var warehouse = await _db.Warehouses.FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new NotFoundException("Almacén no encontrado.");

        if (await _db.Movements.AnyAsync(m => m.WarehouseId == id, ct) ||
            await _db.StockLevels.AnyAsync(s => s.WarehouseId == id && s.Quantity > 0, ct))
        {
            warehouse.IsActive = false;
        }
        else
        {
            _db.Warehouses.Remove(warehouse);
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
        if (await _db.Warehouses.AnyAsync(w => w.Code == code && w.Id != currentId, ct))
            throw new AppException($"Ya existe un almacén con el código \"{code}\".");
    }
}

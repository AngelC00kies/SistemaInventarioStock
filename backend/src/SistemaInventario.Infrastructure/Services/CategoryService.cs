using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Core.Interfaces;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db) => _db = db;

    public async Task<List<CategoryDto>> GetAsync(string? search, bool includeInactive, CancellationToken ct = default)
    {
        var query = _db.Categorias.AsNoTracking().Include(c => c.Productos).AsQueryable();

        if (!includeInactive) query = query.Where(c => c.Activo);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c => c.Nombre.ToLower().Contains(term));
        }

        var items = await query.OrderBy(c => c.Nombre).ToListAsync(ct);
        return items.Select(Mapping.ToDto).ToList();
    }

    public async Task<CategoryDto> GetAsync(int id, CancellationToken ct = default)
    {
        var category = await _db.Categorias.AsNoTracking().Include(c => c.Productos)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Categoría no encontrada.");
        return Mapping.ToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CategoryRequest request, CancellationToken ct = default)
    {
        await ValidateAsync(request, null, ct);

        var category = new Core.Entities.Categoria
        {
            Nombre = request.Name.Trim(),
            Descripcion = request.Description?.Trim(),
            Activo = request.IsActive
        };

        _db.Categorias.Add(category);
        await _db.SaveChangesAsync(ct);
        return await GetAsync(category.Id, ct);
    }

    public async Task<CategoryDto> UpdateAsync(int id, CategoryRequest request, CancellationToken ct = default)
    {
        var category = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Categoría no encontrada.");

        await ValidateAsync(request, id, ct);

        category.Nombre = request.Name.Trim();
        category.Descripcion = request.Description?.Trim();
        category.Activo = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Categoría no encontrada.");

        if (await _db.Productos.AnyAsync(p => p.CategoriaId == id, ct))
        {
            category.Activo = false;
        }
        else
        {
            _db.Categorias.Remove(category);
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task ValidateAsync(CategoryRequest request, int? currentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new AppException("El nombre de la categoría es obligatorio.");

        var name = request.Name.Trim().ToLower();
        if (await _db.Categorias.AnyAsync(c => c.Nombre.ToLower() == name && c.Id != currentId, ct))
            throw new AppException($"Ya existe una categoría llamada \"{request.Name.Trim()}\".");
    }
}

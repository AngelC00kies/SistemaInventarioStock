using SistemaInventario.Core.Common;
using SistemaInventario.Core.Dtos;

namespace SistemaInventario.Core.Interfaces;

/// <summary>Catálogo de productos: listado paginado con filtros y stock agregado, más su alta, edición y baja.</summary>
public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAsync(ProductFilter filter, CancellationToken ct = default);
    Task<ProductDto> GetAsync(int id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(ProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(int id, ProductRequest request, CancellationToken ct = default);
    /// <summary>Si el producto tiene movimientos se desactiva en lugar de borrarlo: el histórico no se toca.</summary>
    Task DeleteAsync(int id, CancellationToken ct = default);
}

/// <summary>Categorías de productos: árbol de agrupación usado en filtros e informes.</summary>
public interface ICategoryService
{
    /// <summary>Combobox del front: con includeInactive = false sólo devuelve las categorías activas.</summary>
    Task<List<CategoryDto>> GetAsync(string? search, bool includeInactive, CancellationToken ct = default);
    Task<CategoryDto> GetAsync(int id, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CategoryRequest request, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(int id, CategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

/// <summary>Proveedores: datos de contacto de quienes abastecen el inventario.</summary>
public interface ISupplierService
{
    Task<List<SupplierDto>> GetAsync(string? search, bool includeInactive, CancellationToken ct = default);
    Task<SupplierDto> GetAsync(int id, CancellationToken ct = default);
    Task<SupplierDto> CreateAsync(SupplierRequest request, CancellationToken ct = default);
    Task<SupplierDto> UpdateAsync(int id, SupplierRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

/// <summary>Almacenes: ubicaciones que acumulan existencias y registran movimientos.</summary>
public interface IWarehouseService
{
    Task<List<WarehouseDto>> GetAsync(string? search, bool includeInactive, CancellationToken ct = default);
    Task<WarehouseDto> GetAsync(int id, CancellationToken ct = default);
    Task<WarehouseDto> CreateAsync(WarehouseRequest request, CancellationToken ct = default);
    Task<WarehouseDto> UpdateAsync(int id, WarehouseRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

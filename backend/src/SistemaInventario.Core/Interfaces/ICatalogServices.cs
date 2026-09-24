using SistemaInventario.Core.Common;
using SistemaInventario.Core.Dtos;

namespace SistemaInventario.Core.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAsync(ProductFilter filter, CancellationToken ct = default);
    Task<ProductDto> GetAsync(int id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(ProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(int id, ProductRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAsync(string? search, bool includeInactive, CancellationToken ct = default);
    Task<CategoryDto> GetAsync(int id, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CategoryRequest request, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(int id, CategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface ISupplierService
{
    Task<List<SupplierDto>> GetAsync(string? search, bool includeInactive, CancellationToken ct = default);
    Task<SupplierDto> GetAsync(int id, CancellationToken ct = default);
    Task<SupplierDto> CreateAsync(SupplierRequest request, CancellationToken ct = default);
    Task<SupplierDto> UpdateAsync(int id, SupplierRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IWarehouseService
{
    Task<List<WarehouseDto>> GetAsync(string? search, bool includeInactive, CancellationToken ct = default);
    Task<WarehouseDto> GetAsync(int id, CancellationToken ct = default);
    Task<WarehouseDto> CreateAsync(WarehouseRequest request, CancellationToken ct = default);
    Task<WarehouseDto> UpdateAsync(int id, WarehouseRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

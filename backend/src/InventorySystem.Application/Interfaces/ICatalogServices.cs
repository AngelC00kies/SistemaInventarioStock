using InventorySystem.Application.Common;
using InventorySystem.Application.Dtos;

namespace InventorySystem.Application.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAsync(bool includeInactive, CancellationToken ct = default);
    Task<CategoryDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CategoryRequest request, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(int id, CategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface ISupplierService
{
    Task<PagedResult<SupplierDto>> GetAsync(PagedQuery query, CancellationToken ct = default);
    Task<SupplierDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SupplierDto> CreateAsync(SupplierRequest request, CancellationToken ct = default);
    Task<SupplierDto> UpdateAsync(int id, SupplierRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IWarehouseService
{
    Task<IReadOnlyList<WarehouseDto>> GetAsync(bool includeInactive, CancellationToken ct = default);
    Task<WarehouseDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<WarehouseDto> CreateAsync(WarehouseRequest request, CancellationToken ct = default);
    Task<WarehouseDto> UpdateAsync(int id, WarehouseRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

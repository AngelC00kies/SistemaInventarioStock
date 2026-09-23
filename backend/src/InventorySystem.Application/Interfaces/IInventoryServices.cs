using InventorySystem.Application.Common;
using InventorySystem.Application.Dtos;

namespace InventorySystem.Application.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAsync(ProductQuery query, CancellationToken ct = default);
    Task<ProductDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(ProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(int id, ProductRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<StockEntryDto>> GetStockAsync(int id, CancellationToken ct = default);
}

public interface IMovementService
{
    Task<MovementDto> CreateAsync(MovementRequest request, CancellationToken ct = default);
    Task<PagedResult<MovementDto>> GetAsync(MovementQuery query, CancellationToken ct = default);
    Task<MovementDto> GetByIdAsync(int id, CancellationToken ct = default);
}

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetAsync(bool pendingOnly, CancellationToken ct = default);
    Task AcknowledgeAsync(int id, CancellationToken ct = default);
    Task<int> GetPendingCountAsync(CancellationToken ct = default);
}

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(CancellationToken ct = default);
}

public interface IReportService
{
    Task<IReadOnlyList<StockReportRow>> GetStockReportAsync(StockReportQuery query, CancellationToken ct = default);
    Task<IReadOnlyList<MovementReportRow>> GetMovementsReportAsync(MovementReportQuery query, CancellationToken ct = default);
    Task<InventoryValueDto> GetInventoryValueAsync(CancellationToken ct = default);
}

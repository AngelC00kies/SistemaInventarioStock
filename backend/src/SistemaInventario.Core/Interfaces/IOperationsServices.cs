using SistemaInventario.Core.Common;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Enums;

namespace SistemaInventario.Core.Interfaces;

public interface IMovementService
{
    Task<MovementDto> CreateAsync(CreateMovementRequest request, int userId, CancellationToken ct = default);
    Task<PagedResult<MovementDto>> GetAsync(MovementFilter filter, CancellationToken ct = default);
    Task<MovementDto> GetAsync(int id, CancellationToken ct = default);
}

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(CancellationToken ct = default);
}

public interface INotificationService
{
    Task<List<NotificationDto>> GetAsync(int limit, bool onlyUnread, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(CancellationToken ct = default);
    Task MarkAsReadAsync(int id, CancellationToken ct = default);
    Task MarkAllAsReadAsync(CancellationToken ct = default);
}

public interface IReportService
{
    Task<ReportFile> ExportAsync(string report, string format, ReportRequest request, CancellationToken ct = default);
}

using SistemaInventario.Core.Common;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Enums;

namespace SistemaInventario.Core.Interfaces;

/// <summary>Histórico de movimientos: el alta aplica el cambio de stock de forma transaccional y el resto sólo consulta.</summary>
public interface IMovementService
{
    Task<MovementDto> CreateAsync(CreateMovementRequest request, int userId, CancellationToken ct = default);
    Task<PagedResult<MovementDto>> GetAsync(MovementFilter filter, CancellationToken ct = default);
    Task<MovementDto> GetAsync(int id, CancellationToken ct = default);
}

/// <summary>KPIs agregados del panel de inicio: stock, flujos del mes y últimos movimientos.</summary>
public interface IDashboardService
{
    Task<DashboardDto> GetAsync(CancellationToken ct = default);
}

/// <summary>Avisos de stock bajo que alimentan la campana de notificaciones.</summary>
public interface INotificationService
{
    /// <summary>limit se recorta al rango 1..200; con onlyUnread sólo se devuelven las no leídas.</summary>
    Task<List<NotificationDto>> GetAsync(int limit, bool onlyUnread, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(CancellationToken ct = default);
    Task MarkAsReadAsync(int id, CancellationToken ct = default);
    Task MarkAllAsReadAsync(CancellationToken ct = default);
}

/// <summary>Exportación de informes a PDF o Excel.</summary>
public interface IReportService
{
    /// <summary>report son los identificadores "stock", "critical-stock", "movements", "products", "categories", "suppliers" o "users"; format "pdf" u "xlsx".</summary>
    Task<ReportFile> ExportAsync(string report, string format, ReportRequest request, CancellationToken ct = default);
}

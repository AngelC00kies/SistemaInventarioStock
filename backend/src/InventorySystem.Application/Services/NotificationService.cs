using InventorySystem.Application.Common;
using InventorySystem.Application.Dtos;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Application.Services;

/// <summary>Alertas de stock bajo: consulta, contador y acuse de recibo.</summary>
public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _db;

    public NotificationService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<NotificationDto>> GetAsync(bool pendingOnly, CancellationToken ct = default)
    {
        var q = _db.LowStockNotifications.AsNoTracking();

        if (pendingOnly)
            q = q.Where(n => !n.IsResolved);

        return await q
            .OrderBy(n => n.IsResolved)
            .ThenByDescending(n => n.DetectedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                ProductId = n.ProductId,
                ProductCode = n.Product.Code,
                ProductName = n.Product.Name,
                WarehouseId = n.WarehouseId,
                WarehouseName = n.Warehouse.Name,
                QuantityAtDetection = n.QuantityAtDetection,
                MinimumStock = n.MinimumStock,
                DetectedAt = n.DetectedAt,
                IsResolved = n.IsResolved,
                ResolvedAt = n.ResolvedAt
            })
            .ToListAsync(ct);
    }

    public async Task AcknowledgeAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.LowStockNotifications.FirstOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new NotFoundException($"Notificación {id} no encontrada.");

        entity.IsResolved = true;
        entity.ResolvedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public Task<int> GetPendingCountAsync(CancellationToken ct = default) =>
        _db.LowStockNotifications.CountAsync(n => !n.IsResolved, ct);
}

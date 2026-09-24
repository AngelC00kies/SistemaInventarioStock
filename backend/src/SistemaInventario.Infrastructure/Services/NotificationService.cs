using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Core.Interfaces;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db) => _db = db;

    public async Task<List<NotificationDto>> GetAsync(int limit, bool onlyUnread, CancellationToken ct = default)
    {
        var query = _db.Notifications.AsNoTracking()
            .Include(n => n.Product)
            .Include(n => n.Warehouse)
            .AsQueryable();

        if (onlyUnread) query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.IsRead)
            .ThenByDescending(n => n.CreatedAt)
            .Take(Math.Clamp(limit, 1, 200))
            .Select(n => Mapping.ToDto(n))
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(CancellationToken ct = default) =>
        await _db.Notifications.CountAsync(n => !n.IsRead, ct);

    public async Task MarkAsReadAsync(int id, CancellationToken ct = default)
    {
        var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new NotFoundException("Notificación no encontrada.");

        notification.IsRead = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(CancellationToken ct = default)
    {
        var unread = await _db.Notifications.Where(n => !n.IsRead).ToListAsync(ct);
        unread.ForEach(n => n.IsRead = true);
        await _db.SaveChangesAsync(ct);
    }
}

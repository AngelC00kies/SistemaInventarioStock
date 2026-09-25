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
        var query = _db.Notificaciones.AsNoTracking()
            .Include(n => n.Producto)
            .Include(n => n.Almacen)
            .AsQueryable();

        if (onlyUnread) query = query.Where(n => !n.Leida);

        return await query
            .OrderByDescending(n => n.Leida)
            .ThenByDescending(n => n.FechaCreacion)
            .Take(Math.Clamp(limit, 1, 200))
            .Select(n => Mapping.ToDto(n))
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(CancellationToken ct = default) =>
        await _db.Notificaciones.CountAsync(n => !n.Leida, ct);

    public async Task MarkAsReadAsync(int id, CancellationToken ct = default)
    {
        var notification = await _db.Notificaciones.FirstOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new NotFoundException("Notificación no encontrada.");

        notification.Leida = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(CancellationToken ct = default)
    {
        var unread = await _db.Notificaciones.Where(n => !n.Leida).ToListAsync(ct);
        unread.ForEach(n => n.Leida = true);
        await _db.SaveChangesAsync(ct);
    }
}

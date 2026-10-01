using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Enums;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Infrastructure.Services;
using SistemaInventario.Tests.Infrastructure;
using Xunit;

namespace SistemaInventario.Tests.Services;

/// <summary>Pruebas de <see cref="NotificationService"/>: listado, contador y marcado de leídas.</summary>
public class NotificationServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly NotificationService _service;

    private readonly int _productoId;
    private readonly int _almacenId;

    public NotificationServiceTests()
    {
        _service = new NotificationService(_db.Db);

        var categoria = Seed.CategoriaAsync(_db.Db).GetAwaiter().GetResult();
        var almacen = Seed.AlmacenAsync(_db.Db).GetAwaiter().GetResult();
        var producto = Seed.ProductoAsync(_db.Db, categoria.Id).GetAwaiter().GetResult();
        _productoId = producto.Id;
        _almacenId = almacen.Id;
    }

    public void Dispose() => _db.Dispose();

    private Task<Core.Entities.Notificacion> AvisoAsync(
        NotificationLevel nivel = NotificationLevel.Warning,
        bool leida = false,
        DateTime? fecha = null) =>
        Seed.NotificacionAsync(_db.Db, _productoId, _almacenId, nivel, leida, fecha);

    [Fact]
    public async Task GetAsync_PorDefecto_DevuelveLosNivelesResueltos()
    {
        await AvisoAsync(NotificationLevel.Critical);

        var avisos = await _service.GetAsync(limit: 10, onlyUnread: false);

        var unico = Assert.Single(avisos);
        Assert.Equal("Principal", unico.WarehouseName);
        Assert.Equal("Stock bajo", unico.Message);
        Assert.Equal(NotificationLevel.Critical, unico.Level);
    }

    [Fact]
    public async Task GetAsync_OnlyUnread_SoloDevuelveLasPendientes()
    {
        await AvisoAsync(leida: false);
        await AvisoAsync(leida: true);

        var pendientes = await _service.GetAsync(limit: 10, onlyUnread: true);
        var todas = await _service.GetAsync(limit: 10, onlyUnread: false);

        Assert.Single(pendientes);
        Assert.False(pendientes[0].IsRead);
        Assert.Equal(2, todas.Count);
    }

    [Fact]
    public async Task GetAsync_ElLimitSeAcotaEntre1Y200()
    {
        // 205 avisos en un único guardado: sirve para comprobar el tope superior sin tardar.
        var avisos = Enumerable.Range(0, 205)
            .Select(i => new Core.Entities.Notificacion
            {
                ProductoId = _productoId,
                AlmacenId = _almacenId,
                Mensaje = $"Aviso {i}",
                Nivel = NotificationLevel.Warning,
                Leida = false,
                FechaCreacion = DateTime.UtcNow.AddMinutes(-i)
            });
        _db.Db.Notificaciones.AddRange(avisos);
        await _db.Db.SaveChangesAsync();

        var tope = await _service.GetAsync(limit: 5000, onlyUnread: false);
        var suelo = await _service.GetAsync(limit: 0, onlyUnread: false);

        Assert.Equal(200, tope.Count);
        Assert.Single(suelo);
    }

    [Fact]
    public async Task GetAsync_OrdenaLasLeidasPrimeroYNuevasPrimero()
    {
        // Orden real del servicio: OrderByDescending(Leida) y después por fecha descendente.
        // Ojo: "descendente" sobre un bool pone las ya leídas por delante de las pendientes.
        await AvisoAsync(leida: true, fecha: new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        await AvisoAsync(leida: false, fecha: new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc));
        await AvisoAsync(leida: false, fecha: new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc));

        var avisos = await _service.GetAsync(limit: 10, onlyUnread: false);

        Assert.Equal(new[] { true, false, false }, avisos.Select(a => a.IsRead));
        Assert.Equal(new[] { "2026-01-03", "2026-01-02" },
            avisos.Where(a => !a.IsRead).Select(a => a.CreatedAt.ToString("yyyy-MM-dd")));
    }

    [Fact]
    public async Task GetUnreadCountAsync_CuentaSoloLasPendientes()
    {
        await AvisoAsync(leida: false);
        await AvisoAsync(leida: false);
        await AvisoAsync(leida: true);

        Assert.Equal(2, await _service.GetUnreadCountAsync());
    }

    [Fact]
    public async Task MarkAsReadAsync_MarcaUnaAvisoComoLeido()
    {
        var aviso = await AvisoAsync(leida: false);

        await _service.MarkAsReadAsync(aviso.Id);

        Assert.True((await _db.Db.Notificaciones.SingleAsync(n => n.Id == aviso.Id)).Leida);
        Assert.Equal(0, await _service.GetUnreadCountAsync());
    }

    [Fact]
    public async Task MarkAsReadAsync_IdInexistente_LanzaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.MarkAsReadAsync(8888));
    }

    [Fact]
    public async Task MarkAllAsReadAsync_DejaTodasComoLeidas()
    {
        await AvisoAsync(leida: false);
        await AvisoAsync(leida: false);
        await AvisoAsync(leida: true);

        await _service.MarkAllAsReadAsync();

        Assert.Equal(0, await _service.GetUnreadCountAsync());
        Assert.Equal(3, (await _service.GetAsync(limit: 10, onlyUnread: false)).Count);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_SinAvisos_NoFalla()
    {
        await _service.MarkAllAsReadAsync();

        Assert.Equal(0, await _service.GetUnreadCountAsync());
    }
}

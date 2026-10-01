using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Entities;
using SistemaInventario.Core.Enums;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Infrastructure.Services;
using SistemaInventario.Tests.Infrastructure;
using Xunit;

namespace SistemaInventario.Tests.Services;

/// <summary>
/// Pruebas de <see cref="MovementService"/>, el servicio con más reglas de negocio:
/// validación del alta, control de existencias, valorización por defecto y avisos de stock bajo.
/// </summary>
public class MovementServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly MovementService _service;

    private readonly Rol _rol;
    private readonly Usuario _usuario;
    private readonly Categoria _categoria;
    private readonly Almacen _almacen;
    private readonly Producto _producto;

    public MovementServiceTests()
    {
        _service = new MovementService(_db.Db);
        _rol = Seed.RolAsync(_db.Db).GetAwaiter().GetResult();
        _usuario = Seed.UsuarioAsync(_db.Db, _rol.Id, "operador").GetAwaiter().GetResult();
        _categoria = Seed.CategoriaAsync(_db.Db).GetAwaiter().GetResult();
        _almacen = Seed.AlmacenAsync(_db.Db).GetAwaiter().GetResult();
        _producto = Seed.ProductoAsync(_db.Db, _categoria.Id, stockMinimo: 10,
            precioCompra: 1000m, precioVenta: 1500m).GetAwaiter().GetResult();
    }

    public void Dispose() => _db.Dispose();

    private CreateMovementRequest Solicitud(
        MovementType tipo = MovementType.Entrada,
        int cantidad = 5,
        string motivo = "Reposición",
        int? productoId = null,
        int? almacenId = null) => new()
    {
        Type = tipo,
        ProductId = productoId ?? _producto.Id,
        WarehouseId = almacenId ?? _almacen.Id,
        Quantity = cantidad,
        Reason = motivo
    };

    // ---------- validación ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-4)]
    public async Task CreateAsync_CantidadNoPositiva_LanzaAppException(int cantidad)
    {
        var excepcion = await Assert.ThrowsAsync<AppException>(
            () => _service.CreateAsync(Solicitud(cantidad: cantidad), _usuario.Id));

        Assert.Equal("La cantidad debe ser mayor a cero.", excepcion.Message);
    }

    [Fact]
    public async Task CreateAsync_SinMotivo_LanzaAppException()
    {
        var excepcion = await Assert.ThrowsAsync<AppException>(
            () => _service.CreateAsync(Solicitud(motivo: "   "), _usuario.Id));

        Assert.Equal("El motivo del movimiento es obligatorio.", excepcion.Message);
    }

    [Fact]
    public async Task CreateAsync_TipoNoValido_LanzaAppException()
    {
        var solicitud = Solicitud();
        solicitud.Type = (MovementType)99;

        var excepcion = await Assert.ThrowsAsync<AppException>(
            () => _service.CreateAsync(solicitud, _usuario.Id));

        Assert.Equal("Tipo de movimiento no válido.", excepcion.Message);
    }

    [Fact]
    public async Task CreateAsync_ProductoInexistente_LanzaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CreateAsync(Solicitud(productoId: 9999), _usuario.Id));
    }

    [Fact]
    public async Task CreateAsync_AlmacenInexistente_LanzaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CreateAsync(Solicitud(almacenId: 9999), _usuario.Id));
    }

    [Fact]
    public async Task CreateAsync_ProductoInactivo_LanzaAppException()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db, "Retirados");
        var inactivo = await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "BAJO-1", activo: false);

        var excepcion = await Assert.ThrowsAsync<AppException>(
            () => _service.CreateAsync(Solicitud(productoId: inactivo.Id), _usuario.Id));

        Assert.Equal("No se pueden registrar movimientos de un producto inactivo.", excepcion.Message);
    }

    [Fact]
    public async Task CreateAsync_AlmacenInactivo_LanzaAppException()
    {
        var cerrado = await Seed.AlmacenAsync(_db.Db, codigo: "CERR", activo: false);

        var excepcion = await Assert.ThrowsAsync<AppException>(
            () => _service.CreateAsync(Solicitud(almacenId: cerrado.Id), _usuario.Id));

        Assert.Equal("No se pueden registrar movimientos en un almacén inactivo.", excepcion.Message);
    }

    [Fact]
    public async Task CreateAsync_SalidaMayorQueElStock_LanzaAppExceptionSinTocarNada()
    {
        await Seed.StockAsync(_db.Db, _producto.Id, _almacen.Id, cantidad: 5);

        var excepcion = await Assert.ThrowsAsync<AppException>(
            () => _service.CreateAsync(Solicitud(tipo: MovementType.Salida, cantidad: 6), _usuario.Id));

        Assert.Equal("Stock insuficiente. Disponible: 5 unidad en Principal.", excepcion.Message);

        // La comprobación ocurre antes de la transacción: ni movimiento ni cambio de stock.
        Assert.Equal(0, await _db.Db.Movimientos.CountAsync());
        Assert.Equal(5, (await _db.Db.NivelesStock.SingleAsync()).Cantidad);
    }

    // ---------- efecto sobre el stock ----------

    [Fact]
    public async Task CreateAsync_EntradaCreaElNivelDeStockYSumaLasUnidades()
    {
        var movimiento = await _service.CreateAsync(Solicitud(tipo: MovementType.Entrada, cantidad: 7), _usuario.Id);

        var nivel = await _db.Db.NivelesStock.SingleAsync();
        Assert.Equal(7, nivel.Cantidad);
        Assert.Equal(7, movimiento.StockAfter);
        Assert.Equal(MovementType.Entrada, movimiento.Type);
    }

    [Fact]
    public async Task CreateAsync_EntradaAcumulaSobreElStockExistente()
    {
        await Seed.StockAsync(_db.Db, _producto.Id, _almacen.Id, cantidad: 4);

        var movimiento = await _service.CreateAsync(Solicitud(tipo: MovementType.Entrada, cantidad: 6), _usuario.Id);

        Assert.Equal(10, (await _db.Db.NivelesStock.SingleAsync()).Cantidad);
        Assert.Equal(10, movimiento.StockAfter);
    }

    [Fact]
    public async Task CreateAsync_SalidaRestaLasUnidades()
    {
        await Seed.StockAsync(_db.Db, _producto.Id, _almacen.Id, cantidad: 9);

        var movimiento = await _service.CreateAsync(Solicitud(tipo: MovementType.Salida, cantidad: 4), _usuario.Id);

        Assert.Equal(5, (await _db.Db.NivelesStock.SingleAsync()).Cantidad);
        Assert.Equal(5, movimiento.StockAfter);
    }

    [Fact]
    public async Task CreateAsync_SalidaPorElStockExacto_DejaElAlmacenACero()
    {
        await Seed.StockAsync(_db.Db, _producto.Id, _almacen.Id, cantidad: 5);

        var movimiento = await _service.CreateAsync(Solicitud(tipo: MovementType.Salida, cantidad: 5), _usuario.Id);

        Assert.Equal(0, (await _db.Db.NivelesStock.SingleAsync()).Cantidad);
        Assert.Equal(0, movimiento.StockAfter);
    }

    // ---------- valorización ----------

    [Fact]
    public async Task CreateAsync_SinPrecio_UsaElCosteEnEntradasYPrecioDeVentaEnSalidas()
    {
        await Seed.StockAsync(_db.Db, _producto.Id, _almacen.Id, cantidad: 10);

        var entrada = await _service.CreateAsync(Solicitud(tipo: MovementType.Entrada, cantidad: 1), _usuario.Id);
        var salida = await _service.CreateAsync(Solicitud(tipo: MovementType.Salida, cantidad: 1), _usuario.Id);

        Assert.Equal(1000m, entrada.UnitPrice);
        Assert.Equal(1500m, salida.UnitPrice);
    }

    [Fact]
    public async Task CreateAsync_ConPrecioExplicito_SeUsaEnLugarDelDefecto()
    {
        var solicitud = Solicitud(tipo: MovementType.Entrada, cantidad: 2);
        solicitud.UnitPrice = 777.5m;

        var movimiento = await _service.CreateAsync(solicitud, _usuario.Id);

        Assert.Equal(777.5m, movimiento.UnitPrice);
        Assert.Equal(1555m, movimiento.Total);
    }

    [Fact]
    public async Task CreateAsync_GuardaElDocumentoYLaFechaIndicadas()
    {
        var fecha = new DateTime(2026, 3, 10, 8, 30, 0, DateTimeKind.Utc);
        var solicitud = Solicitud(tipo: MovementType.Entrada, cantidad: 3);
        solicitud.DocumentReference = "  OC-456  ";
        solicitud.Date = fecha;

        var movimiento = await _service.CreateAsync(solicitud, _usuario.Id);

        Assert.Equal("OC-456", movimiento.DocumentReference);
        Assert.Equal(fecha, movimiento.Date);
        Assert.Equal(_usuario.Id, movimiento.UserId);
    }

    // ---------- avisos de stock bajo ----------

    [Fact]
    public async Task CreateAsync_SinAviso_CuandoElStockQuedaPorEncimaDelMinimo()
    {
        await Seed.StockAsync(_db.Db, _producto.Id, _almacen.Id, cantidad: 0);

        await _service.CreateAsync(Solicitud(tipo: MovementType.Entrada, cantidad: 11), _usuario.Id);

        Assert.Equal(0, await _db.Db.Notificaciones.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_AvisoWarning_CuandoElStockQuedaEnElMinimo()
    {
        await Seed.StockAsync(_db.Db, _producto.Id, _almacen.Id, cantidad: 0);

        await _service.CreateAsync(Solicitud(tipo: MovementType.Entrada, cantidad: 10), _usuario.Id);

        var aviso = await _db.Db.Notificaciones.SingleAsync();
        Assert.Equal(NotificationLevel.Warning, aviso.Nivel);
        Assert.False(aviso.Leida);
        Assert.Contains(_producto.Nombre, aviso.Mensaje);
        Assert.Contains("10", aviso.Mensaje);
    }

    [Fact]
    public async Task CreateAsync_AvisoCritical_CuandoElStockLlegaALaMitadDelMinimo()
    {
        await Seed.StockAsync(_db.Db, _producto.Id, _almacen.Id, cantidad: 0);

        await _service.CreateAsync(Solicitud(tipo: MovementType.Entrada, cantidad: 5), _usuario.Id);

        Assert.Equal(NotificationLevel.Critical, (await _db.Db.Notificaciones.SingleAsync()).Nivel);
    }

    [Fact]
    public async Task CreateAsync_NoDuplicaElAviso_SiYaHayUnoPendiente()
    {
        await Seed.StockAsync(_db.Db, _producto.Id, _almacen.Id, cantidad: 0);

        await _service.CreateAsync(Solicitud(tipo: MovementType.Entrada, cantidad: 10), _usuario.Id);
        // La salida deja 9 unidades, todavía por debajo del mínimo: no debe abrir un segundo aviso.
        await _service.CreateAsync(Solicitud(tipo: MovementType.Salida, cantidad: 1), _usuario.Id);

        Assert.Equal(1, await _db.Db.Notificaciones.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_AlRecuperarsePorEncimaDelMinimo_CierraLosAvisosPendientes()
    {
        await Seed.StockAsync(_db.Db, _producto.Id, _almacen.Id, cantidad: 0);
        await _service.CreateAsync(Solicitud(tipo: MovementType.Entrada, cantidad: 10), _usuario.Id);
        Assert.False((await _db.Db.Notificaciones.SingleAsync()).Leida);

        await _service.CreateAsync(Solicitud(tipo: MovementType.Entrada, cantidad: 1), _usuario.Id);

        Assert.True((await _db.Db.Notificaciones.SingleAsync()).Leida);
    }

    // ---------- listado y filtros ----------

    [Fact]
    public async Task GetAsync_OrdenaDeMasRecienteAMasAntiguo()
    {
        await CrearMovimientosAsync(
            new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 3, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 2, 9, 0, 0, DateTimeKind.Utc));

        var resultado = await _service.GetAsync(new MovementFilter());

        Assert.Equal(3, resultado.Total);
        Assert.Equal(new[] { "2026-03-03", "2026-03-02", "2026-03-01" },
            resultado.Items.Select(m => m.Date.ToString("yyyy-MM-dd")));
    }

    [Fact]
    public async Task GetAsync_FiltroHastaAbarcaElDiaCompleto()
    {
        await CrearMovimientosAsync(
            new DateTime(2026, 3, 10, 0, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 10, 23, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 11, 0, 30, 0, DateTimeKind.Utc));

        var soloDiezDeMarzo = await _service.GetAsync(new MovementFilter
        {
            From = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
            To = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc)
        });

        // Sin esta ampliación, el "hasta" a medianoche excluiría el movimiento de las 23:30.
        Assert.Equal(2, soloDiezDeMarzo.Total);
        Assert.All(soloDiezDeMarzo.Items, m => Assert.Equal("2026-03-10", m.Date.ToString("yyyy-MM-dd")));
    }

    [Fact]
    public async Task GetAsync_FiltroDesdeExcluyeLoAnterior()
    {
        await CrearMovimientosAsync(
            new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 20, 9, 0, 0, DateTimeKind.Utc));

        var resultado = await _service.GetAsync(new MovementFilter
        {
            From = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc)
        });

        Assert.Equal(1, resultado.Total);
    }

    [Fact]
    public async Task GetAsync_FiltraPorProductoAlmacenUsuarioYTipo()
    {
        await CrearMovimientosAsync(
            new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 2, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 3, 9, 0, 0, DateTimeKind.Utc));

        var entrada = await _service.GetAsync(new MovementFilter { Type = MovementType.Entrada });
        var salida = await _service.GetAsync(new MovementFilter { Type = MovementType.Salida });
        var porProducto = await _service.GetAsync(new MovementFilter { ProductId = _producto.Id });
        var porAlmacen = await _service.GetAsync(new MovementFilter { WarehouseId = _almacen.Id });
        var porUsuario = await _service.GetAsync(new MovementFilter { UserId = _usuario.Id });
        var porOtroUsuario = await _service.GetAsync(new MovementFilter { UserId = _usuario.Id + 100 });

        Assert.Equal(3, entrada.Total);
        Assert.Equal(0, salida.Total);
        Assert.Equal(3, porProducto.Total);
        Assert.Equal(3, porAlmacen.Total);
        Assert.Equal(3, porUsuario.Total);
        Assert.Equal(0, porOtroUsuario.Total);
    }

    [Fact]
    public async Task GetAsync_BusquedaCoincideEnProductoMotivoYDocumento()
    {
        var movimiento = await _service.CreateAsync(Solicitud(tipo: MovementType.Entrada, cantidad: 1), _usuario.Id);
        var solicitud = Solicitud(tipo: MovementType.Entrada, cantidad: 2, motivo: "Traslado a tienda");
        solicitud.DocumentReference = "GUIA-99";
        await _service.CreateAsync(solicitud, _usuario.Id);

        var porMotivo = await _service.GetAsync(new MovementFilter { Search = "traslado" });
        var porDocumento = await _service.GetAsync(new MovementFilter { Search = "guia-99" });
        var porProducto = await _service.GetAsync(new MovementFilter { Search = _producto.Codigo });
        var porInexistente = await _service.GetAsync(new MovementFilter { Search = "no-figura" });

        Assert.Equal(1, porMotivo.Total);
        Assert.Equal(1, porDocumento.Total);
        Assert.Equal(2, porProducto.Total);
        Assert.Equal(0, porInexistente.Total);
        Assert.True(movimiento.Id > 0);
    }

    [Fact]
    public async Task GetAsync_PaginaLosResultados()
    {
        await CrearMovimientosAsync(
            new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 2, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 3, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 4, 9, 0, 0, DateTimeKind.Utc));

        var pagina1 = await _service.GetAsync(new MovementFilter { Page = 1, PageSize = 3 });
        var pagina2 = await _service.GetAsync(new MovementFilter { Page = 2, PageSize = 3 });

        Assert.Equal(4, pagina1.Total);
        Assert.Equal(3, pagina1.Items.Count);
        Assert.Single(pagina2.Items);
        Assert.Equal("2026-03-01", pagina2.Items[0].Date.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public async Task GetAsync_PorId_DevuelveElMovimientoResuelto()
    {
        var creado = await _service.CreateAsync(Solicitud(tipo: MovementType.Entrada, cantidad: 8), _usuario.Id);

        var consultado = await _service.GetAsync(creado.Id);

        Assert.Equal(_producto.Codigo, consultado.ProductCode);
        Assert.Equal(_almacen.Nombre, consultado.WarehouseName);
        Assert.Equal(_usuario.NombreCompleto, consultado.UserName);
    }

    [Fact]
    public async Task GetAsync_PorIdInexistente_LanzaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAsync(31416));
    }

    /// <summary>Crea un movimiento por cada fecha indicada (entrada de 1 unidad).</summary>
    private async Task CrearMovimientosAsync(params DateTime[] fechas)
    {
        await Seed.StockAsync(_db.Db, _producto.Id, _almacen.Id, cantidad: 0);

        foreach (var fecha in fechas)
        {
            var solicitud = Solicitud(tipo: MovementType.Entrada, cantidad: 1);
            solicitud.Date = fecha;
            await _service.CreateAsync(solicitud, _usuario.Id);
        }
    }
}

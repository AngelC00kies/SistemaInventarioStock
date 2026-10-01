using SistemaInventario.Core.Enums;
using SistemaInventario.Infrastructure.Services;
using SistemaInventario.Tests.Infrastructure;
using Xunit;

namespace SistemaInventario.Tests.Services;

/// <summary>Pruebas de <see cref="DashboardService"/>: KPIs, gráfico de 30 días y tablas del panel.</summary>
public class DashboardServiceTests : IDisposable
{
    // InMemory a propósito: el panel agrega importes con SUM sobre decimal, que SQLite no admite.
    private readonly TestDatabase _db = new(TestProvider.InMemory);
    private readonly DashboardService _service;

    private readonly int _usuarioId;

    public DashboardServiceTests()
    {
        _service = new DashboardService(_db.Db);

        var rol = Seed.RolAsync(_db.Db).GetAwaiter().GetResult();
        var usuario = Seed.UsuarioAsync(_db.Db, rol.Id, "operador").GetAwaiter().GetResult();
        _usuarioId = usuario.Id;
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetAsync_SinDatos_DevuelveLosContadoresACeroYSinErrores()
    {
        var panel = await _service.GetAsync();

        Assert.Equal(0, panel.TotalProducts);
        Assert.Equal(0, panel.TotalUnits);
        Assert.Equal(0, panel.LowStockProducts);
        Assert.Equal(0, panel.MovementsToday);
        Assert.Equal(0m, panel.MonthlyInValue);
        Assert.Equal(0m, panel.MonthlyOutValue);
        Assert.Empty(panel.RecentMovements);
        Assert.Empty(panel.CriticalProducts);
    }

    [Fact]
    public async Task GetAsync_SumaLasUnidadesYLosProductosActivos()
    {
        var (categoria, almacen) = await CrearBaseAsync();
        var activo = await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "A-1", stockMinimo: 10);
        var inactivo = await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "I-1", activo: false);
        await Seed.StockAsync(_db.Db, activo.Id, almacen.Id, cantidad: 12);
        await Seed.StockAsync(_db.Db, inactivo.Id, almacen.Id, cantidad: 40);

        var panel = await _service.GetAsync();

        // Los productos inactivos no cuentan, pero su stock sí está en el almacén.
        Assert.Equal(1, panel.TotalProducts);
        Assert.Equal(52, panel.TotalUnits);
        Assert.Equal(1, panel.ActiveWarehouses);
        // 12 > mínimo 10, así que no hay faltantes; el producto inactivo no entra en el recuento.
        Assert.Equal(0, panel.LowStockProducts);
    }

    [Fact]
    public async Task GetAsync_CuentaLosProductosConStockEnElMinimo()
    {
        var (categoria, almacen) = await CrearBaseAsync();
        var falto = await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "F-1", stockMinimo: 10);
        var sobra = await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "S-1", stockMinimo: 10);
        await Seed.StockAsync(_db.Db, falto.Id, almacen.Id, cantidad: 10);
        await Seed.StockAsync(_db.Db, sobra.Id, almacen.Id, cantidad: 11);

        Assert.Equal(1, (await _service.GetAsync()).LowStockProducts);
    }

    [Fact]
    public async Task GetAsync_ValorizaLasEntradasYSalidasDelMesEnCurso()
    {
        var (categoria, almacen) = await CrearBaseAsync();
        var producto = await Seed.ProductoAsync(_db.Db, categoria.Id);

        await Seed.MovimientoAsync(_db.Db, producto.Id, almacen.Id, _usuarioId,
            MovementType.Entrada, cantidad: 4, precioUnitario: 100m);
        await Seed.MovimientoAsync(_db.Db, producto.Id, almacen.Id, _usuarioId,
            MovementType.Salida, cantidad: 3, precioUnitario: 250m);
        // Un movimiento del mes pasado no debe sumar en el total del mes en curso.
        await Seed.MovimientoAsync(_db.Db, producto.Id, almacen.Id, _usuarioId,
            MovementType.Entrada, cantidad: 100, precioUnitario: 100m,
            fecha: DateTime.UtcNow.AddMonths(-1));

        var panel = await _service.GetAsync();

        Assert.Equal(400m, panel.MonthlyInValue);
        Assert.Equal(750m, panel.MonthlyOutValue);
    }

    [Fact]
    public async Task GetAsync_CuentaLosMovimientosDeHoy()
    {
        var (categoria, almacen) = await CrearBaseAsync();
        var producto = await Seed.ProductoAsync(_db.Db, categoria.Id);

        await Seed.MovimientoAsync(_db.Db, producto.Id, almacen.Id, _usuarioId,
            fecha: DateTime.UtcNow.AddHours(-2));
        await Seed.MovimientoAsync(_db.Db, producto.Id, almacen.Id, _usuarioId,
            fecha: DateTime.UtcNow.AddDays(-3));

        Assert.Equal(1, (await _service.GetAsync()).MovementsToday);
    }

    [Fact]
    public async Task GetAsync_ElGraficoDiarioTiene30DiasYTerminaEnHoy()
    {
        var (categoria, almacen) = await CrearBaseAsync();
        var producto = await Seed.ProductoAsync(_db.Db, categoria.Id);
        await Seed.MovimientoAsync(_db.Db, producto.Id, almacen.Id, _usuarioId,
            MovementType.Entrada, cantidad: 7, fecha: DateTime.UtcNow.AddDays(-1));

        var panel = await _service.GetAsync();

        // 30 casillas aunque no haya actividad: así la gráfica no queda con huecos.
        Assert.Equal(30, panel.DailyFlow.Count);
        Assert.Equal(DateTime.UtcNow.ToString("yyyy-MM-dd"), panel.DailyFlow[^1].Date);
        Assert.Equal(DateTime.UtcNow.AddDays(-29).ToString("yyyy-MM-dd"), panel.DailyFlow[0].Date);

        var ayer = panel.DailyFlow.Single(d => d.Date == DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd"));
        Assert.Equal(7, ayer.Entries);
        Assert.Equal(0, ayer.Exits);
    }

    [Fact]
    public async Task GetAsync_AgrupaPorCategoriaOrdenandoPorUnidades()
    {
        var poca = await Seed.CategoriaAsync(_db.Db, "Accesorios");
        var mucha = await Seed.CategoriaAsync(_db.Db, "Herramientas");
        var almacen = await Seed.AlmacenAsync(_db.Db);
        var chico = await Seed.ProductoAsync(_db.Db, poca.Id, codigo: "C-1");
        var grande = await Seed.ProductoAsync(_db.Db, mucha.Id, codigo: "G-1");
        await Seed.StockAsync(_db.Db, chico.Id, almacen.Id, cantidad: 2);
        await Seed.StockAsync(_db.Db, grande.Id, almacen.Id, cantidad: 50);

        var panel = await _service.GetAsync();

        Assert.Equal(new[] { "Herramientas", "Accesorios" }, panel.CategoryShare.Select(c => c.Name));
        Assert.Equal(50, panel.CategoryShare[0].Units);
        Assert.Equal(1, panel.CategoryShare[0].Products);
    }

    [Fact]
    public async Task GetAsync_LosProductosCriticosVanDelMasDesabastecidoAlMenos()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);
        var almacen = await Seed.AlmacenAsync(_db.Db);
        var leve = await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "L-1", stockMinimo: 10);
        var grave = await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "G-1", stockMinimo: 10);
        await Seed.StockAsync(_db.Db, leve.Id, almacen.Id, cantidad: 9);
        await Seed.StockAsync(_db.Db, grave.Id, almacen.Id, cantidad: 1);

        var panel = await _service.GetAsync();

        Assert.Equal(new[] { "G-1", "L-1" }, panel.CriticalProducts.Select(c => c.Code));
        Assert.Equal("critical", panel.CriticalProducts[0].Status);
        Assert.Equal(almacen.Nombre, panel.CriticalProducts[0].WarehouseName);
    }

    [Fact]
    public async Task GetAsync_ElListadoDeCriticosNoSuperaLas8Filas()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);
        var almacen = await Seed.AlmacenAsync(_db.Db);
        for (var i = 1; i <= 12; i++)
        {
            var producto = await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: $"X-{i:D2}", stockMinimo: 10);
            await Seed.StockAsync(_db.Db, producto.Id, almacen.Id, cantidad: 1);
        }

        Assert.Equal(8, (await _service.GetAsync()).CriticalProducts.Count);
    }

    [Fact]
    public async Task GetAsync_ElListadoDeMovimientosRecientesSaleLimitadoA8()
    {
        var (categoria, almacen) = await CrearBaseAsync();
        var producto = await Seed.ProductoAsync(_db.Db, categoria.Id);
        for (var i = 0; i < 12; i++)
            await Seed.MovimientoAsync(_db.Db, producto.Id, almacen.Id, _usuarioId,
                fecha: DateTime.UtcNow.AddMinutes(-i));

        var panel = await _service.GetAsync();

        Assert.Equal(8, panel.RecentMovements.Count);
        // Orden descendente por fecha: el más nuevo encima.
        var fechas = panel.RecentMovements.Select(m => m.Date).ToList();
        Assert.Equal(fechas.OrderByDescending(f => f), fechas);
    }

    /// <summary>Crea una categoría y un almacén válidos para colgar el resto de los datos.</summary>
    private async Task<(Core.Entities.Categoria Categoria, Core.Entities.Almacen Almacen)> CrearBaseAsync()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);
        var almacen = await Seed.AlmacenAsync(_db.Db);
        return (categoria, almacen);
    }
}

using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Enums;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Infrastructure.Services;
using SistemaInventario.Tests.Infrastructure;
using Xunit;

namespace SistemaInventario.Tests.Services;

/// <summary>Pruebas de <see cref="WarehouseService"/>: código único normalizado y borrado protegido.</summary>
public class WarehouseServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly WarehouseService _service;

    public WarehouseServiceTests() => _service = new WarehouseService(_db.Db);

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateAsync_NormalizaElCodigoAMayusculas()
    {
        var almacen = await _service.CreateAsync(new WarehouseRequest
        {
            Name = "  Depósito  ",
            Code = "  alm-02  ",
            Location = "Bodega 2",
            IsActive = true
        });

        Assert.Equal("ALM-02", almacen.Code);
        Assert.Equal("Depósito", almacen.Name);
    }

    [Fact]
    public async Task CreateAsync_CodigoRepetidoAunEnDistintaCaja_LanzaAppException()
    {
        await Seed.AlmacenAsync(_db.Db, codigo: "ALM-01");

        var excepcion = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new WarehouseRequest
        {
            Name = "Otro",
            Code = "alm-01",
            IsActive = true
        }));

        Assert.Equal("Ya existe un almacén con el código \"ALM-01\".", excepcion.Message);
    }

    [Fact]
    public async Task CreateAsync_SinCodigoOLetra_LanzaAppException()
    {
        await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new WarehouseRequest
        {
            Name = "Sin código",
            Code = "   ",
            IsActive = true
        }));

        var excepcion = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new WarehouseRequest
        {
            Name = "",
            Code = "ALM-09",
            IsActive = true
        }));
        Assert.Equal("El nombre del almacén es obligatorio.", excepcion.Message);
    }

    [Fact]
    public async Task GetAsync_OrdenaPorCodigo()
    {
        await Seed.AlmacenAsync(_db.Db, codigo: "Z-01", nombre: "Zurich");
        await Seed.AlmacenAsync(_db.Db, codigo: "A-01", nombre: "Amsterdam");

        var resultado = await _service.GetAsync(null, includeInactive: false);

        Assert.Equal(new[] { "A-01", "Z-01" }, resultado.Select(w => w.Code));
    }

    [Fact]
    public async Task GetAsync_ConBusqueda_CoincidePorNombreOCodigo()
    {
        await Seed.AlmacenAsync(_db.Db, codigo: "NORTE", nombre: "Bodega Norte");
        await Seed.AlmacenAsync(_db.Db, codigo: "SUR", nombre: "Bodega Sur");

        var porCodigo = await _service.GetAsync("norte", includeInactive: false);
        var porNombre = await _service.GetAsync("BODEGA", includeInactive: false);

        Assert.Single(porCodigo);
        Assert.Equal(2, porNombre.Count);
    }

    [Fact]
    public async Task DeleteAsync_SinMovimientosNiExistencias_BorraFisicamente()
    {
        var almacen = await Seed.AlmacenAsync(_db.Db, codigo: "VACIO");

        await _service.DeleteAsync(almacen.Id);

        Assert.False(await _db.Db.Almacenes.AnyAsync(w => w.Id == almacen.Id));
    }

    [Fact]
    public async Task DeleteAsync_ConMovimientos_SoloLoDesactiva()
    {
        var almacen = await Seed.AlmacenAsync(_db.Db, codigo: "HIST");
        await CrearMovimientoEnAsync(almacen.Id);

        await _service.DeleteAsync(almacen.Id);

        var superviviente = await _db.Db.Almacenes.SingleAsync(w => w.Id == almacen.Id);
        Assert.False(superviviente.Activo);
    }

    [Fact]
    public async Task DeleteAsync_ConStockPositivo_SoloLoDesactiva()
    {
        var almacen = await Seed.AlmacenAsync(_db.Db, codigo: "CON-STOCK");
        var categoria = await Seed.CategoriaAsync(_db.Db);
        var producto = await Seed.ProductoAsync(_db.Db, categoria.Id);
        await Seed.StockAsync(_db.Db, producto.Id, almacen.Id, cantidad: 4);

        await _service.DeleteAsync(almacen.Id);

        var superviviente = await _db.Db.Almacenes.SingleAsync(w => w.Id == almacen.Id);
        Assert.False(superviviente.Activo);
    }

    [Fact]
    public async Task DeleteAsync_IdInexistente_LanzaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(777));
    }

    private async Task CrearMovimientoEnAsync(int almacenId)
    {
        var rol = await Seed.RolAsync(_db.Db);
        var usuario = await Seed.UsuarioAsync(_db.Db, rol.Id, "operador");
        var categoria = await Seed.CategoriaAsync(_db.Db);
        var producto = await Seed.ProductoAsync(_db.Db, categoria.Id);

        await Seed.MovimientoAsync(_db.Db, producto.Id, almacenId, usuario.Id, MovementType.Entrada);
    }
}

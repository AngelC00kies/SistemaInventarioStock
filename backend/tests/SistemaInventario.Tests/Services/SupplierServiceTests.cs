using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Infrastructure.Services;
using SistemaInventario.Tests.Infrastructure;
using Xunit;

namespace SistemaInventario.Tests.Services;

/// <summary>Pruebas de <see cref="SupplierService"/>: validación del correo y borrado protegido.</summary>
public class SupplierServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly SupplierService _service;

    public SupplierServiceTests() => _service = new SupplierService(_db.Db);

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateAsync_GuardaLosDatosDeContacto()
    {
        var proveedor = await _service.CreateAsync(new SupplierRequest
        {
            Name = "Distribuidora Norte",
            ContactName = "  Carla Soto  ",
            Phone = "+56 9 1234 5678",
            Email = "contacto@norte.cl",
            Address = "Av. Siempre Viva 742",
            IsActive = true
        });

        Assert.True(proveedor.Id > 0);
        Assert.Equal("Carla Soto", proveedor.ContactName);
        Assert.Equal("contacto@norte.cl", proveedor.Email);
    }

    [Fact]
    public async Task CreateAsync_CorreoSinArroba_LanzaAppException()
    {
        var excepcion = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new SupplierRequest
        {
            Name = "Sin Correo",
            Email = "esto-no-es-un-correo"
        }));

        Assert.Equal("El correo electrónico no es válido.", excepcion.Message);
    }

    [Fact]
    public async Task CreateAsync_NombreRepetido_LanzaAppException()
    {
        await Seed.ProveedorAsync(_db.Db, "ACME");

        var excepcion = await Assert.ThrowsAsync<AppException>(
            () => _service.CreateAsync(new SupplierRequest { Name = "acme" }));

        Assert.Contains("Ya existe un proveedor", excepcion.Message);
    }

    [Fact]
    public async Task GetAsync_BuscaPorNombreOPorContacto()
    {
        await Seed.ProveedorAsync(_db.Db, "Distribuidora Sur");
        await _service.CreateAsync(new SupplierRequest
        {
            Name = "Importadora Este",
            ContactName = "Rodrigo Díaz",
            IsActive = true
        });

        var porNombre = await _service.GetAsync("importadora", includeInactive: false);
        var porContacto = await _service.GetAsync("rodrigo", includeInactive: false);

        Assert.Single(porNombre);
        Assert.Single(porContacto);
        Assert.Equal("Importadora Este", porContacto[0].Name);
    }

    [Fact]
    public async Task GetAsync_IdInexistente_LanzaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAsync(4242));
    }

    [Fact]
    public async Task DeleteAsync_SinProductos_BorraFisicamente()
    {
        var proveedor = await Seed.ProveedorAsync(_db.Db, "Descartable");

        await _service.DeleteAsync(proveedor.Id);

        Assert.False(await _db.Db.Proveedores.AnyAsync(p => p.Id == proveedor.Id));
    }

    [Fact]
    public async Task DeleteAsync_ConProductos_SoloLoDesactiva()
    {
        var proveedor = await Seed.ProveedorAsync(_db.Db, "ACME");
        await Seed.ProductoAsync(_db.Db, categoriaId: await CrearCategoriaAsync(), proveedorId: proveedor.Id);

        await _service.DeleteAsync(proveedor.Id);

        var superviviente = await _db.Db.Proveedores.SingleAsync(p => p.Id == proveedor.Id);
        Assert.False(superviviente.Activo);
        Assert.True(await _db.Db.Productos.AnyAsync(p => p.ProveedorId == proveedor.Id));
    }

    private async Task<int> CrearCategoriaAsync() => (await Seed.CategoriaAsync(_db.Db)).Id;
}

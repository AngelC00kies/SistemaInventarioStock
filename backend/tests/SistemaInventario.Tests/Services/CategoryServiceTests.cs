using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Infrastructure.Services;
using SistemaInventario.Tests.Infrastructure;
using Xunit;

namespace SistemaInventario.Tests.Services;

/// <summary>Pruebas de <see cref="CategoryService"/>: alta, listado con filtros y borrado protegido.</summary>
public class CategoryServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly CategoryService _service;

    public CategoryServiceTests() => _service = new CategoryService(_db.Db);

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetAsync_PorDefecto_SoloDevuelveLasActivas()
    {
        await Seed.CategoriaAsync(_db.Db, "Electrónica", activo: true);
        await Seed.CategoriaAsync(_db.Db, "Retirada", activo: false);

        var soloActivas = await _service.GetAsync(search: null, includeInactive: false);
        var todas = await _service.GetAsync(search: null, includeInactive: true);

        Assert.Single(soloActivas);
        Assert.Equal(2, todas.Count);
    }

    [Fact]
    public async Task GetAsync_ConBusqueda_FiltraPorNombreSinDistinguirMayusculas()
    {
        await Seed.CategoriaAsync(_db.Db, "Electrónica");
        await Seed.CategoriaAsync(_db.Db, "Oficina");

        var resultado = await _service.GetAsync("ELECTR", includeInactive: false);

        var unica = Assert.Single(resultado);
        Assert.Equal("Electrónica", unica.Name);
    }

    [Fact]
    public async Task GetAsync_OrdenaAlfabeticamente()
    {
        await Seed.CategoriaAsync(_db.Db, "Zapatos");
        await Seed.CategoriaAsync(_db.Db, "Audífonos");
        await Seed.CategoriaAsync(_db.Db, "Mesa");

        var resultado = await _service.GetAsync(null, includeInactive: false);

        Assert.Equal(new[] { "Audífonos", "Mesa", "Zapatos" }, resultado.Select(c => c.Name));
    }

    [Fact]
    public async Task CreateAsync_GuardaLosDatosYDevuelveElId()
    {
        var creada = await _service.CreateAsync(new CategoryRequest
        {
            Name = "  Herramientas  ",
            Description = "  Taladros y sierras  ",
            IsActive = true
        });

        Assert.True(creada.Id > 0);
        // Los espacios de los extremos se recortan al guardar.
        Assert.Equal("Herramientas", creada.Name);
        Assert.Equal("Taladros y sierras", creada.Description);
        Assert.True(creada.IsActive);
    }

    [Fact]
    public async Task CreateAsync_NombreVacio_LanzaAppException()
    {
        var excepcion = await Assert.ThrowsAsync<AppException>(
            () => _service.CreateAsync(new CategoryRequest { Name = "   " }));

        Assert.Equal("El nombre de la categoría es obligatorio.", excepcion.Message);
        Assert.Equal(400, excepcion.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_NombreYaUsado_LanzaAppException()
    {
        await Seed.CategoriaAsync(_db.Db, "Electrónica");

        var excepcion = await Assert.ThrowsAsync<AppException>(
            () => _service.CreateAsync(new CategoryRequest { Name = "electrónica" }));

        Assert.Contains("Ya existe una categoría", excepcion.Message);
    }

    [Fact]
    public async Task UpdateAsync_NombreRepetidoPorOtraCategoria_LanzaAppException()
    {
        var primera = await Seed.CategoriaAsync(_db.Db, "Electrónica");
        var segunda = await Seed.CategoriaAsync(_db.Db, "Oficina");

        await Assert.ThrowsAsync<AppException>(() => _service.UpdateAsync(segunda.Id, new CategoryRequest
        {
            Name = "ELECTRÓNICA",
            IsActive = true
        }));

        // La propia categoría puede conservar su nombre sin chocar consigo misma.
        var propia = await _service.UpdateAsync(primera.Id, new CategoryRequest
        {
            Name = "Electrónica",
            Description = "Ahora con descripción",
            IsActive = true
        });
        Assert.Equal("Ahora con descripción", propia.Description);
    }

    [Fact]
    public async Task GetAsync_IdInexistente_LanzaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAsync(999));
    }

    [Fact]
    public async Task UpdateAsync_IdInexistente_LanzaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(999, new CategoryRequest
        {
            Name = "Nada",
            IsActive = true
        }));
    }

    [Fact]
    public async Task DeleteAsync_SinProductos_BorraFisicamente()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db, "Temporal");

        await _service.DeleteAsync(categoria.Id);

        Assert.False(await _db.Db.Categorias.AnyAsync(c => c.Id == categoria.Id));
    }

    [Fact]
    public async Task DeleteAsync_ConProductosAsociados_SoloLaDesactiva()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db, "Electrónica");
        await Seed.ProductoAsync(_db.Db, categoria.Id);

        await _service.DeleteAsync(categoria.Id);

        var superviviente = await _db.Db.Categorias.SingleAsync(c => c.Id == categoria.Id);
        Assert.False(superviviente.Activo);
    }
}

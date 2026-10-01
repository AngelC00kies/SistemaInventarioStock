using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Infrastructure.Services;
using SistemaInventario.Tests.Infrastructure;
using Xunit;

namespace SistemaInventario.Tests.Services;

/// <summary>Pruebas de <see cref="ProductService"/>: validaciones, estados derivados y paginación.</summary>
public class ProductServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly ProductService _service;

    public ProductServiceTests() => _service = new ProductService(_db.Db);

    public void Dispose() => _db.Dispose();

    // ---------- validación del alta ----------

    [Fact]
    public async Task CreateAsync_SinCodigoOLetra_LanzaAppException()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);

        var sinCodigo = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new ProductRequest
        {
            Code = "  ",
            Name = "Cable",
            CategoryId = categoria.Id,
            PurchasePrice = 100,
            SalePrice = 200
        }));
        Assert.Equal("El código del producto es obligatorio.", sinCodigo.Message);

        var sinNombre = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new ProductRequest
        {
            Code = "P-100",
            Name = "",
            CategoryId = categoria.Id,
            PurchasePrice = 100,
            SalePrice = 200
        }));
        Assert.Equal("El nombre del producto es obligatorio.", sinNombre.Message);
    }

    [Fact]
    public async Task CreateAsync_PreciosIncoherentes_LanzaAppException()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);

        var negativo = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new ProductRequest
        {
            Code = "P-101",
            Name = "Negativo",
            CategoryId = categoria.Id,
            PurchasePrice = -1,
            SalePrice = 200
        }));
        Assert.Equal("Los precios no pueden ser negativos.", negativo.Message);

        var ventaMenor = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new ProductRequest
        {
            Code = "P-102",
            Name = "Venta menor",
            CategoryId = categoria.Id,
            PurchasePrice = 500,
            SalePrice = 100
        }));
        Assert.Equal("El precio de venta no puede ser menor al precio de compra.", ventaMenor.Message);
    }

    [Fact]
    public async Task CreateAsync_StockMinimoNegativo_LanzaAppException()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);

        var excepcion = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new ProductRequest
        {
            Code = "P-103",
            Name = "Sin mínimos",
            CategoryId = categoria.Id,
            PurchasePrice = 100,
            SalePrice = 200,
            MinStock = -5
        }));

        Assert.Equal("El stock mínimo no puede ser negativo.", excepcion.Message);
    }

    [Fact]
    public async Task CreateAsync_CategoriaInactivaOInexistente_LanzaAppException()
    {
        var activa = await Seed.CategoriaAsync(_db.Db, "Activa");
        var inactiva = await Seed.CategoriaAsync(_db.Db, "Inactiva", activo: false);

        var inexistente = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new ProductRequest
        {
            Code = "P-104",
            Name = "Sin categoría",
            CategoryId = 999,
            PurchasePrice = 100,
            SalePrice = 200
        }));
        Assert.Equal("La categoría seleccionada no es válida.", inexistente.Message);

        await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new ProductRequest
        {
            Code = "P-105",
            Name = "Categoría baja",
            CategoryId = inactiva.Id,
            PurchasePrice = 100,
            SalePrice = 200
        }));

        // La categoría activa sí permite el alta.
        var creada = await _service.CreateAsync(new ProductRequest
        {
            Code = "P-106",
            Name = "Válida",
            CategoryId = activa.Id,
            PurchasePrice = 100,
            SalePrice = 200
        });
        Assert.True(creada.Id > 0);
    }

    [Fact]
    public async Task CreateAsync_ProveedorInactivo_LanzaAppException()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);
        var proveedor = await Seed.ProveedorAsync(_db.Db, "Baja", activo: false);

        var excepcion = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new ProductRequest
        {
            Code = "P-107",
            Name = "Con proveedor baja",
            CategoryId = categoria.Id,
            SupplierId = proveedor.Id,
            PurchasePrice = 100,
            SalePrice = 200
        }));

        Assert.Equal("El proveedor seleccionado no es válido.", excepcion.Message);
    }

    [Fact]
    public async Task CreateAsync_CodigoRepetido_LanzaAppException()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);
        await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "P-200");

        var excepcion = await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new ProductRequest
        {
            Code = "p-200",
            Name = "Otro",
            CategoryId = categoria.Id,
            PurchasePrice = 100,
            SalePrice = 200
        }));

        Assert.Equal("Ya existe un producto con el código \"P-200\".", excepcion.Message);
    }

    // ---------- estados derivados y filtros ----------

    [Theory]
    [InlineData(20, 10, "ok")]
    [InlineData(10, 10, "low")]
    [InlineData(5, 10, "critical")]
    [InlineData(0, 10, "empty")]
    public async Task GetAsync_DerivaElEstadoDelStockTotal(int cantidad, int minimo, string esperado)
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);
        var almacen = await Seed.AlmacenAsync(_db.Db);
        var producto = await Seed.ProductoAsync(_db.Db, categoria.Id, stockMinimo: minimo);
        await Seed.StockAsync(_db.Db, producto.Id, almacen.Id, cantidad);

        var dto = await _service.GetAsync(producto.Id);

        Assert.Equal(cantidad, dto.TotalStock);
        Assert.Equal(esperado, dto.Status);
    }

    [Fact]
    public async Task GetAsync_FiltroStockBajo_SeleccionaLoQueNoSuperaElMinimo()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);
        var almacen = await Seed.AlmacenAsync(_db.Db);
        var bajo = await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "B-001", stockMinimo: 10);
        var sano = await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "S-001", stockMinimo: 10);
        await Seed.StockAsync(_db.Db, bajo.Id, almacen.Id, cantidad: 3);
        await Seed.StockAsync(_db.Db, sano.Id, almacen.Id, cantidad: 99);

        var resultado = await _service.GetAsync(new ProductFilter { OnlyLowStock = true });

        var unico = Assert.Single(resultado.Items);
        Assert.Equal("B-001", unico.Code);
        Assert.Equal(1, resultado.Total);
    }

    [Fact]
    public async Task GetAsync_FiltroPorCategoriaYProveedor()
    {
        var catA = await Seed.CategoriaAsync(_db.Db, "Auriculares");
        var catB = await Seed.CategoriaAsync(_db.Db, "Bolsas");
        var proveedor = await Seed.ProveedorAsync(_db.Db, "ACME");
        await Seed.ProductoAsync(_db.Db, catA.Id, codigo: "A-001");
        await Seed.ProductoAsync(_db.Db, catA.Id, proveedorId: proveedor.Id, codigo: "A-002");
        await Seed.ProductoAsync(_db.Db, catB.Id, codigo: "B-002");

        var porCategoria = await _service.GetAsync(new ProductFilter { CategoryId = catA.Id });
        var porProveedor = await _service.GetAsync(new ProductFilter { SupplierId = proveedor.Id });

        Assert.Equal(2, porCategoria.Total);
        var unico = Assert.Single(porProveedor.Items);
        Assert.Equal("A-002", unico.Code);
    }

    [Fact]
    public async Task GetAsync_BusquedaCoincideEnCodigoNombreYDescripcion()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);
        await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "HDMI-2", nombre: "Cable HDMI");
        await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "VGA-1", nombre: "Adaptador VGA");

        var porCodigo = await _service.GetAsync(new ProductFilter { Search = " hdmi " });
        var porNombre = await _service.GetAsync(new ProductFilter { Search = "ADAPTADOR" });

        Assert.Single(porCodigo.Items);
        Assert.Single(porNombre.Items);
    }

    [Fact]
    public async Task GetAsync_PorDefecto_ExcluyeLosInactivos()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);
        await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "ACT-1", activo: true);
        await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "DES-1", activo: false);

        var porDefecto = await _service.GetAsync(new ProductFilter());
        var conInactivos = await _service.GetAsync(new ProductFilter { IncludeInactive = true });

        Assert.Equal(1, porDefecto.Total);
        Assert.Equal(2, conInactivos.Total);
    }

    [Fact]
    public async Task GetAsync_AcotalamenteElTamanoDePaginaEntre1Y200()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);
        await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "X-1");

        var demasiadoGrande = await _service.GetAsync(new ProductFilter { PageSize = 9999 });
        var demasiadoPequeno = await _service.GetAsync(new ProductFilter { PageSize = 0, Page = -4 });

        Assert.Equal(200, demasiadoGrande.PageSize);
        Assert.Equal(1, demasiadoPequeno.Page);
        Assert.Equal(1, demasiadoPequeno.PageSize);
    }

    [Fact]
    public async Task GetAsync_ConAlmacen_FiltraElDesglosePeroNoElTotal()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);
        var almacenA = await Seed.AlmacenAsync(_db.Db, codigo: "A-01", nombre: "Norte");
        var almacenB = await Seed.AlmacenAsync(_db.Db, codigo: "B-01", nombre: "Sur");
        var producto = await Seed.ProductoAsync(_db.Db, categoria.Id, stockMinimo: 5);
        await Seed.StockAsync(_db.Db, producto.Id, almacenA.Id, cantidad: 7);
        await Seed.StockAsync(_db.Db, producto.Id, almacenB.Id, cantidad: 1);

        var soloA = await _service.GetAsync(new ProductFilter { WarehouseId = almacenA.Id });

        var unico = Assert.Single(soloA.Items);
        Assert.Single(unico.StockByWarehouse);
        Assert.Equal("Norte", unico.StockByWarehouse[0].WarehouseName);
        Assert.Equal(8, unico.TotalStock);
    }

    [Fact]
    public async Task GetAsync_PaginaLosResultados()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);
        for (var i = 1; i <= 5; i++)
            await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: $"PAG-{i:D2}", nombre: $"Producto {i}");

        var pagina1 = await _service.GetAsync(new ProductFilter { Page = 1, PageSize = 2 });
        var pagina3 = await _service.GetAsync(new ProductFilter { Page = 3, PageSize = 2 });

        Assert.Equal(5, pagina1.Total);
        Assert.Equal(2, pagina1.Items.Count);
        Assert.Equal(3, pagina1.TotalPages);
        Assert.Single(pagina3.Items);
        Assert.Equal("PAG-05", pagina3.Items[0].Code);
    }

    // ---------- edición y borrado ----------

    [Fact]
    public async Task UpdateAsync_NormalizaElCodigoYAplicaLosPrecios()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);
        var producto = await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "V-1", precioCompra: 100, precioVenta: 200);

        var actualizado = await _service.UpdateAsync(producto.Id, new ProductRequest
        {
            Code = " v-1 ",
            Name = "Renombrado",
            CategoryId = categoria.Id,
            PurchasePrice = 150,
            SalePrice = 300,
            Unit = "",
            MinStock = 12,
            IsActive = true
        });

        Assert.Equal("V-1", actualizado.Code);
        Assert.Equal("Renombrado", actualizado.Name);
        Assert.Equal(150m, actualizado.PurchasePrice);
        Assert.Equal(300m, actualizado.SalePrice);
        // Unidad vacía cae al valor por defecto.
        Assert.Equal("Unidad", actualizado.Unit);
        Assert.Equal(12, actualizado.MinStock);
    }

    [Fact]
    public async Task DeleteAsync_SinMovimientos_BorraFisicamente()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);
        var producto = await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "BORRA-1");

        await _service.DeleteAsync(producto.Id);

        Assert.False(await _db.Db.Productos.AnyAsync(p => p.Id == producto.Id));
    }

    [Fact]
    public async Task DeleteAsync_ConMovimientos_SoloLoDesactiva()
    {
        var rol = await Seed.RolAsync(_db.Db);
        var usuario = await Seed.UsuarioAsync(_db.Db, rol.Id, "operador");
        var categoria = await Seed.CategoriaAsync(_db.Db);
        var almacen = await Seed.AlmacenAsync(_db.Db);
        var producto = await Seed.ProductoAsync(_db.Db, categoria.Id, codigo: "HIST-1");
        await Seed.MovimientoAsync(_db.Db, producto.Id, almacen.Id, usuario.Id);

        await _service.DeleteAsync(producto.Id);

        var superviviente = await _db.Db.Productos.SingleAsync(p => p.Id == producto.Id);
        Assert.False(superviviente.Activo);
    }

    [Fact]
    public async Task DeleteAsync_IdInexistente_LanzaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(555));
    }
}

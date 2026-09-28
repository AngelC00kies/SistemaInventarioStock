using InventorySystem.Application.Common;
using InventorySystem.Application.Dtos;
using InventorySystem.Application.Services;
using InventorySystem.Domain.Enums;
using InventorySystem.Domain.Exceptions;

namespace InventorySystem.Tests;

public class ProductServiceTests : ServiceTestBase
{
    private ProductService CreateService() => new(Db);

    private ProductRequest NewRequest(string code = "P-NEW") => new()
    {
        Code = code,
        Name = "Producto nuevo",
        Description = "Descripción",
        CategoryId = CategoryId,
        SupplierId = null,
        UnitOfMeasure = UnitOfMeasure.Unidad,
        PurchasePrice = 80,
        SalePrice = 120,
        MinimumStock = 5,
    };

    [Fact]
    public async Task Crear_Producto_GuardaYDevuelveMapeo()
    {
        var service = CreateService();

        var dto = await service.CreateAsync(NewRequest());

        Assert.True(dto.Id > 0);
        Assert.Equal("P-NEW", dto.Code);
        Assert.Equal(CategoryId, dto.CategoryId);
        Assert.Equal(UnitOfMeasure.Unidad, dto.UnitOfMeasure);
        Assert.Equal(0, dto.TotalStock);
        Assert.True(dto.IsActive);
        Assert.True(dto.IsLowStock); // 0 unidades <= mínimo 5
    }

    [Fact]
    public async Task Crear_CodigoDuplicado_LanzaError()
    {
        var service = CreateService();
        await service.CreateAsync(NewRequest("P-DUP"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateAsync(NewRequest("p-dup"))); // mayúsculas/minúsculas no diferencian

        Assert.Contains("código", ex.Message);
    }

    [Fact]
    public async Task Crear_CategoriaInexistente_LanzaError()
    {
        var service = CreateService();
        var request = NewRequest();
        request.CategoryId = 999999;

        var ex = await Assert.ThrowsAsync<AppException>(() => service.CreateAsync(request));
        Assert.Contains("categoría", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Actualizar_CambiaPrecios_YStockMinimo()
    {
        var service = CreateService();
        var created = await service.CreateAsync(NewRequest());

        var request = NewRequest();
        request.Name = "Producto actualizado";
        request.PurchasePrice = 90;
        request.SalePrice = 140;
        request.MinimumStock = 20;

        var updated = await service.UpdateAsync(created.Id, request);

        Assert.Equal("Producto actualizado", updated.Name);
        Assert.Equal(90, updated.PurchasePrice);
        Assert.Equal(140, updated.SalePrice);
        Assert.Equal(20, updated.MinimumStock);
    }

    [Fact]
    public async Task Actualizar_ProductoInexistente_LanzaNotFound()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(999999, NewRequest()));
    }

    [Fact]
    public async Task Eliminar_EsBajaLogica_NoBorraElRegistro()
    {
        var service = CreateService();
        var created = await service.CreateAsync(NewRequest());

        await service.DeleteAsync(created.Id);

        var entity = Db.Products.Single(p => p.Id == created.Id);
        Assert.False(entity.IsActive); // baja lógica
        Assert.Equal(1, Db.Products.Count());
    }

    [Fact]
    public async Task Get_ContaStockTotal_MarcaStockBajo()
    {
        var product = CreateProduct("P-STOCK", minimumStock: 10);
        var movement = new MovementService(Db, CurrentUser);

        await movement.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = product.Id,
            WarehouseId = WarehouseId,
            Quantity = 6,
            Reason = "Inicial",
        });

        var service = CreateService();
        var result = await service.GetAsync(new ProductQuery());

        var dto = Assert.Single(result.Items);
        Assert.Equal(6, dto.TotalStock);
        Assert.True(dto.IsLowStock); // 6 <= 10
    }

    [Fact]
    public async Task Get_FiltraPorCodigoYNombre()
    {
        CreateProduct("P-ALFA", "Teclado mecánico");
        CreateProduct("P-BETA", "Ratón inalámbrico");

        var service = CreateService();

        var porCodigo = await service.GetAsync(new ProductQuery { Search = "P-ALF" });
        Assert.Single(porCodigo.Items);

        var porNombre = await service.GetAsync(new ProductQuery { Search = "atón" });
        Assert.Single(porNombre.Items);

        var sinResultados = await service.GetAsync(new ProductQuery { Search = "no-existe" });
        Assert.Equal(0, sinResultados.TotalCount);
    }

    [Fact]
    public async Task Get_FiltroStockBajo_SoloDevuelveCriticos()
    {
        CreateProduct("P-OK", minimumStock: 5);   // sin stock => TotalStock 0 <= 5 => bajo
        CreateProduct("P-CRIT", minimumStock: 10);

        var movement = new MovementService(Db, CurrentUser);
        var stockOk = Db.Products.Single(p => p.Code == "P-OK");

        await movement.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = stockOk.Id,
            WarehouseId = WarehouseId,
            Quantity = 50, // por encima del mínimo
            Reason = "Reposición",
        });

        var service = CreateService();
        var result = await service.GetAsync(new ProductQuery { LowStockOnly = true });

        Assert.Single(result.Items);
        Assert.Equal("P-CRIT", result.Items[0].Code);
    }

    [Fact]
    public async Task GetStock_DevuelveDesglosePorAlmacen()
    {
        var product = CreateProduct();
        var movement = new MovementService(Db, CurrentUser);

        await movement.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = product.Id,
            WarehouseId = WarehouseId,
            Quantity = 10,
            Reason = "Inicial",
        });

        await movement.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = product.Id,
            WarehouseId = Warehouse2Id,
            Quantity = 4,
            Reason = "Traslado",
        });

        var service = CreateService();
        var entries = await service.GetStockAsync(product.Id);

        Assert.Equal(2, entries.Count);
        Assert.Equal(14, entries.Sum(e => e.Quantity));
        Assert.Contains(entries, e => e.WarehouseId == WarehouseId && e.Quantity == 10);
        Assert.Contains(entries, e => e.WarehouseId == Warehouse2Id && e.Quantity == 4);
    }

    [Fact]
    public async Task GetStock_ProductoInexistente_LanzaNotFound()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetStockAsync(999999));
    }
}

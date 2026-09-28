using InventorySystem.Application.Dtos;
using InventorySystem.Application.Services;
using InventorySystem.Domain.Enums;
using InventorySystem.Domain.Exceptions;

namespace InventorySystem.Tests;

public class MovementServiceTests : ServiceTestBase
{
    private MovementService CreateService() => new(Db, CurrentUser);

    // ─────────────── Entradas ───────────────

    [Fact]
    public async Task Entrada_CreaStock_YRegistraMovimiento()
    {
        var product = CreateProduct();
        var service = CreateService();

        var result = await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = product.Id,
            WarehouseId = WarehouseId,
            Quantity = 25,
            Reason = "Compra a proveedor",
            DocumentReference = "OC-001",
        });

        Assert.Equal(25, result.Quantity);
        Assert.Equal(25, result.StockAfter);
        Assert.Equal(25, StockOf(product.Id, WarehouseId));
        Assert.Equal("OC-001", result.DocumentReference);
        Assert.Equal(CurrentUser.UserId, result.UserId);
        Assert.Equal(CurrentUser.UserName, result.UserName);
    }

    [Fact]
    public async Task Entrada_Acumula_EnAlmacenMultiple()
    {
        var product = CreateProduct();
        var service = CreateService();

        await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = product.Id,
            WarehouseId = WarehouseId,
            Quantity = 10,
            Reason = "Primera compra",
        });

        await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = product.Id,
            WarehouseId = WarehouseId,
            Quantity = 5,
            Reason = "Segunda compra",
        });

        Assert.Equal(15, StockOf(product.Id, WarehouseId));

        // Otro almacén queda independiente
        await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = product.Id,
            WarehouseId = Warehouse2Id,
            Quantity = 7,
            Reason = "Traslado",
        });

        Assert.Equal(15, StockOf(product.Id, WarehouseId));
        Assert.Equal(7, StockOf(product.Id, Warehouse2Id));
    }

    // ─────────────── Salidas ───────────────

    [Fact]
    public async Task Salida_ReduceStock_YGuardaStockResultante()
    {
        var product = CreateProduct();
        var service = CreateService();

        await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = product.Id,
            WarehouseId = WarehouseId,
            Quantity = 30,
            Reason = "Stock inicial",
        });

        var salida = await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Salida,
            ProductId = product.Id,
            WarehouseId = WarehouseId,
            Quantity = 12,
            Reason = "Venta a cliente",
        });

        Assert.Equal(18, salida.StockAfter);
        Assert.Equal(18, StockOf(product.Id, WarehouseId));
        Assert.Equal(MovementType.Salida, salida.Type);
    }

    [Fact]
    public async Task Salida_QueExcedeStock_LanzaError_YNoModificaNada()
    {
        var product = CreateProduct();
        var service = CreateService();

        await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = product.Id,
            WarehouseId = WarehouseId,
            Quantity = 5,
            Reason = "Stock inicial",
        });

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateAsync(new MovementRequest
            {
                Type = MovementType.Salida,
                ProductId = product.Id,
                WarehouseId = WarehouseId,
                Quantity = 6,
                Reason = "Venta mayor al stock",
            }));

        Assert.Contains("Stock insuficiente", ex.Message);
        // El stock queda intacto: nunca negativo
        Assert.Equal(5, StockOf(product.Id, WarehouseId));
    }

    [Fact]
    public async Task Salida_SinStockRegistrado_LanzaError()
    {
        var product = CreateProduct();
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateAsync(new MovementRequest
            {
                Type = MovementType.Salida,
                ProductId = product.Id,
                WarehouseId = WarehouseId,
                Quantity = 1,
                Reason = "Venta sin stock",
            }));

        Assert.Contains("no tiene stock registrado", ex.Message);
        Assert.Equal(0, StockOf(product.Id, WarehouseId));
    }

    // ─────────────── Reglas de negocio ───────────────

    [Fact]
    public async Task Movimiento_ProductoInactivo_Rechazado()
    {
        var product = CreateProduct();
        product.IsActive = false;
        Db.SaveChanges();

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateAsync(new MovementRequest
            {
                Type = MovementType.Entrada,
                ProductId = product.Id,
                WarehouseId = WarehouseId,
                Quantity = 1,
                Reason = "Intento con producto inactivo",
            }));

        Assert.Contains("inactivo", ex.Message);
    }

    [Fact]
    public async Task Movimiento_AlmacenInactivo_Rechazado()
    {
        var product = CreateProduct();
        var warehouse = Db.Warehouses.First(w => w.Id == WarehouseId);
        warehouse.IsActive = false;
        Db.SaveChanges();

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateAsync(new MovementRequest
            {
                Type = MovementType.Entrada,
                ProductId = product.Id,
                WarehouseId = WarehouseId,
                Quantity = 1,
                Reason = "Intento con almacén inactivo",
            }));

        Assert.Contains("almacén inactivo", ex.Message);
    }

    [Fact]
    public async Task Movimiento_SinUsuarioAutenticado_Rechazado()
    {
        var product = CreateProduct();
        var anon = new FakeCurrentUser(null, null);
        var service = new MovementService(Db, anon);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.CreateAsync(new MovementRequest
            {
                Type = MovementType.Entrada,
                ProductId = product.Id,
                WarehouseId = WarehouseId,
                Quantity = 1,
                Reason = "Sin sesión",
            }));
    }

    // ─────────────── Alertas de stock bajo ───────────────

    [Fact]
    public async Task Salida_QueCruzaMinimo_GeneraAlerta()
    {
        var product = CreateProduct(minimumStock: 10);
        var service = CreateService();

        await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = product.Id,
            WarehouseId = WarehouseId,
            Quantity = 12,
            Reason = "Stock inicial",
        });

        Assert.Empty(Db.LowStockNotifications); // 12 > 10: sin alerta

        await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Salida,
            ProductId = product.Id,
            WarehouseId = WarehouseId,
            Quantity = 4, // queda 8 <= 10
            Reason = "Venta",
        });

        var alert = Assert.Single(Db.LowStockNotifications);
        Assert.False(alert.IsResolved);
        Assert.Equal(8, alert.QuantityAtDetection);
        Assert.Equal(10, alert.MinimumStock);
        Assert.Equal(product.Id, alert.ProductId);
    }

    [Fact]
    public async Task ReposicionSobreMinimo_ResolveLaAlerta()
    {
        var product = CreateProduct(minimumStock: 10);
        var service = CreateService();

        await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = product.Id,
            WarehouseId = WarehouseId,
            Quantity = 10, // igual al mínimo => alerta
            Reason = "Stock inicial",
        });

        var alert = Assert.Single(Db.LowStockNotifications);
        Assert.False(alert.IsResolved);

        await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = product.Id,
            WarehouseId = WarehouseId,
            Quantity = 5, // 15 > 10 => se resuelve
            Reason = "Reposición",
        });

        Db.Entry(alert).Reload();
        Assert.True(alert.IsResolved);
        Assert.NotNull(alert.ResolvedAt);
    }

    [Fact]
    public async Task Alerta_VuelveACrear_SiSeVuelveABajar()
    {
        var product = CreateProduct(minimumStock: 10);
        var service = CreateService();

        await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = product.Id,
            WarehouseId = WarehouseId,
            Quantity = 10,
            Reason = "Inicial",
        });

        await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = product.Id,
            WarehouseId = WarehouseId,
            Quantity = 5,
            Reason = "Sube sobre mínimo",
        });

        await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Salida,
            ProductId = product.Id,
            WarehouseId = WarehouseId,
            Quantity = 8, // 15 - 8 = 7 <= 10
            Reason = "Vuelve a bajar",
        });

        Assert.Equal(2, Db.LowStockNotifications.Count());
        Assert.Equal(1, Db.LowStockNotifications.Count(n => !n.IsResolved));
    }

    // ─────────────── Consultas ───────────────

    [Fact]
    public async Task Get_FiltraPorProductoTipoYAlmacen()
    {
        var productA = CreateProduct("P-A", "Producto A");
        var productB = CreateProduct("P-B", "Producto B");
        var service = CreateService();

        await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = productA.Id,
            WarehouseId = WarehouseId,
            Quantity = 10,
            Reason = "entrada A",
        });

        await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Salida,
            ProductId = productA.Id,
            WarehouseId = WarehouseId,
            Quantity = 2,
            Reason = "salida A",
        });

        await service.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = productB.Id,
            WarehouseId = Warehouse2Id,
            Quantity = 4,
            Reason = "entrada B",
        });

        var soloA = await service.GetAsync(new MovementQuery { ProductId = productA.Id });
        Assert.Equal(2, soloA.TotalCount);

        var soloEntradas = await service.GetAsync(new MovementQuery { Type = MovementType.Entrada });
        Assert.Equal(2, soloEntradas.TotalCount);
        Assert.All(soloEntradas.Items, m => Assert.Equal(MovementType.Entrada, m.Type));

        var soloAlmacen1 = await service.GetAsync(new MovementQuery { WarehouseId = WarehouseId });
        Assert.Equal(2, soloAlmacen1.TotalCount);

        var porCodigo = await service.GetAsync(new MovementQuery { Search = "P-B" });
        Assert.Single(porCodigo.Items);
        Assert.Equal(productB.Id, porCodigo.Items[0].ProductId);
    }

    [Fact]
    public async Task GetById_MovimientoInexistente_LanzaNotFound()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(999999));
    }
}

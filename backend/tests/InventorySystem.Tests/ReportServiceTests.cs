using InventorySystem.Application.Dtos;
using InventorySystem.Application.Services;
using InventorySystem.Domain.Enums;

namespace InventorySystem.Tests;

public class ReportServiceTests : ServiceTestBase
{
    private ReportService CreateService() => new(Db);

    private async Task SeedAsync()
    {
        var productA = CreateProduct("P-ALFA", "Teclado", minimumStock: 5);
        var productB = CreateProduct("P-BETA", "Monitor", minimumStock: 3);
        var movement = new MovementService(Db, CurrentUser);

        await movement.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = productA.Id,
            WarehouseId = WarehouseId,
            Quantity = 20,
            Reason = "Compra",
        });

        await movement.CreateAsync(new MovementRequest
        {
            Type = MovementType.Salida,
            ProductId = productA.Id,
            WarehouseId = WarehouseId,
            Quantity = 3,
            Reason = "Venta",
        });

        await movement.CreateAsync(new MovementRequest
        {
            Type = MovementType.Entrada,
            ProductId = productB.Id,
            WarehouseId = Warehouse2Id,
            Quantity = 2, // <= mínimo 3 => crítico
            Reason = "Compra parcial",
        });
    }

    [Fact]
    public async Task ReporteStock_IncluyeTodasLasLineasConStock()
    {
        await SeedAsync();

        var rows = await CreateService().GetStockReportAsync(new StockReportQuery());

        Assert.Equal(2, rows.Count);
        Assert.Contains(rows, r => r.ProductCode == "P-ALFA" && r.Quantity == 17);
        Assert.Contains(rows, r => r.ProductCode == "P-BETA" && r.Quantity == 2);
    }

    [Fact]
    public async Task ReporteStock_CalculaValorizacionYEstado()
    {
        await SeedAsync();

        var rows = await CreateService().GetStockReportAsync(new StockReportQuery());
        var teclado = rows.Single(r => r.ProductCode == "P-ALFA");

        // Precio de compra 100 * 17 unidades
        Assert.Equal(1700, teclado.StockValue);
        Assert.Equal("Disponible", teclado.Status); // 17 > mínimo 5

        var monitor = rows.Single(r => r.ProductCode == "P-BETA");
        Assert.Equal("Stock bajo", monitor.Status); // 2 <= mínimo 3
    }

    [Fact]
    public async Task ReporteStock_FiltroCritico_SoloDevuelveCriticos()
    {
        await SeedAsync();

        var rows = await CreateService()
            .GetStockReportAsync(new StockReportQuery { CriticalOnly = true });

        var row = Assert.Single(rows);
        Assert.Equal("P-BETA", row.ProductCode);
        Assert.Equal("Stock bajo", row.Status);
    }

    [Fact]
    public async Task ReporteStock_FiltroPorAlmacen()
    {
        await SeedAsync();

        var rows = await CreateService()
            .GetStockReportAsync(new StockReportQuery { WarehouseId = Warehouse2Id });

        var row = Assert.Single(rows);
        Assert.Equal("P-BETA", row.ProductCode);
        Assert.Equal("Almacén Test 2", row.Warehouse);
    }

    [Fact]
    public async Task ReporteMovimientos_OrdenaYPoseeTrazabilidad()
    {
        await SeedAsync();

        var rows = await CreateService().GetMovementsReportAsync(new MovementReportQuery());

        Assert.Equal(3, rows.Count);
        Assert.All(rows, r => Assert.Equal(CurrentUser.UserName, r.UserName));

        // Ordenado descendente por fecha
        Assert.Equal(rows.OrderByDescending(r => r.Date).Select(r => r.Date), rows.Select(r => r.Date));
    }

    [Fact]
    public async Task ReporteMovimientos_FiltraPorTipo()
    {
        await SeedAsync();

        var service = CreateService();

        var entradas = await service.GetMovementsReportAsync(
            new MovementReportQuery { Type = MovementType.Entrada });
        Assert.Equal(2, entradas.Count);
        Assert.All(entradas, r => Assert.Equal("Entrada", r.Type));

        var salidas = await service.GetMovementsReportAsync(
            new MovementReportQuery { Type = MovementType.Salida });
        var salida = Assert.Single(salidas);
        Assert.Equal("Salida", salida.Type);
        Assert.Equal(17, salida.StockAfter); // trazabilidad del stock resultante
    }

    [Fact]
    public async Task ReporteMovimientos_FiltraPorProducto()
    {
        await SeedAsync();
        var productB = Db.Products.Single(p => p.Code == "P-BETA");

        var rows = await CreateService().GetMovementsReportAsync(
            new MovementReportQuery { ProductId = productB.Id });

        var row = Assert.Single(rows);
        Assert.Equal("P-BETA", row.ProductCode);
    }

    [Fact]
    public async Task Valorizacion_Inventario_AcuertaTotales()
    {
        await SeedAsync();

        var dto = await CreateService().GetInventoryValueAsync();

        Assert.Equal(2, dto.TotalProducts);
        Assert.Equal(19, dto.TotalUnits);          // 17 + 2
        Assert.Equal(1900, dto.TotalPurchaseValue); // 19 * 100
        Assert.Equal(1, dto.CriticalProducts);      // P-BETA
    }

    // ─────────────── Dashboard y notificaciones ───────────────

    [Fact]
    public async Task Dashboard_ResumenConsistente()
    {
        await SeedAsync();

        var dto = await new DashboardService(Db).GetAsync();

        Assert.Equal(2, dto.ActiveProducts);
        Assert.Equal(19, dto.TotalUnits);
        Assert.Equal(2, dto.Warehouses); // la base de test crea 2 almacenes activos
        Assert.Equal(1, dto.LowStockCount);
        Assert.Equal(3, dto.RecentMovements.Count);
        Assert.Single(dto.LowStockAlerts);
    }

    [Fact]
    public async Task Notificaciones_PendientesYAcuse()
    {
        await SeedAsync();

        var service = new NotificationService(Db);

        var pending = await service.GetAsync(pendingOnly: true);
        var alert = Assert.Single(pending);
        Assert.Equal(1, await service.GetPendingCountAsync());

        await service.AcknowledgeAsync(alert.Id);

        Assert.Equal(0, await service.GetPendingCountAsync());
        Assert.Empty(await service.GetAsync(pendingOnly: true));
        Assert.Single(await service.GetAsync(pendingOnly: false));
    }

    [Fact]
    public async Task Notificaciones_AcuseInexistente_LanzaNotFound()
    {
        var service = new NotificationService(Db);

        await Assert.ThrowsAsync<InventorySystem.Domain.Exceptions.NotFoundException>(
            () => service.AcknowledgeAsync(999999));
    }
}

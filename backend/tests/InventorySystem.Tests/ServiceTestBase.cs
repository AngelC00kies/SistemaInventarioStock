using InventorySystem.Application.Common;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using InventorySystem.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Tests;

/// <summary>
/// Base para tests de servicios: crea una base SQLite en memoria con el esquema completo
/// y datos mínimos (categoría, almacén, producto) reutilizables.
/// </summary>
public abstract class ServiceTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly ApplicationDbContext Db;
    protected readonly FakeCurrentUser CurrentUser = new("user-1", "tester");

    protected int CategoryId { get; }
    protected int WarehouseId { get; }
    protected int Warehouse2Id { get; }

    protected ServiceTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new ApplicationDbContext(options);
        Db.Database.EnsureCreated();

        var category = new Category { Name = "Test", Description = "Categoria de prueba" };
        var warehouse = new Warehouse { Code = "ALM-T1", Name = "Almacén Test" };
        var warehouse2 = new Warehouse { Code = "ALM-T2", Name = "Almacén Test 2" };
        Db.AddRange(category, warehouse, warehouse2);
        Db.SaveChanges();

        CategoryId = category.Id;
        WarehouseId = warehouse.Id;
        Warehouse2Id = warehouse2.Id;
    }

    protected Product CreateProduct(
        string code = "P-TEST",
        string name = "Producto test",
        int minimumStock = 5,
        bool save = true)
    {
        var product = new Product
        {
            Code = code,
            Name = name,
            CategoryId = CategoryId,
            UnitOfMeasure = UnitOfMeasure.Unidad,
            PurchasePrice = 100,
            SalePrice = 150,
            MinimumStock = minimumStock,
        };

        Db.Products.Add(product);
        if (save) Db.SaveChanges();
        return product;
    }

    protected int StockOf(int productId, int warehouseId) =>
        Db.Stocks
            .Where(s => s.ProductId == productId && s.WarehouseId == warehouseId)
            .Select(s => (int?)s.Quantity)
            .FirstOrDefault() ?? 0;

    protected IApplicationDbContext Ctx => Db;

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>Usuario autenticado falso para los tests.</summary>
public sealed class FakeCurrentUser : ICurrentUserService
{
    public FakeCurrentUser(string? userId, string? userName)
    {
        UserId = userId;
        UserName = userName;
    }

    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public bool IsAuthenticated => UserId is not null;
    public bool IsInRole(string role) => false;
}

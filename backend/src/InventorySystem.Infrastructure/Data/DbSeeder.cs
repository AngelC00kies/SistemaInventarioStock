using InventorySystem.Application.Common;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using InventorySystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InventorySystem.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<ApplicationDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = sp.GetRequiredService<ILogger<ApplicationDbContext>>();

        await db.Database.MigrateAsync();

        // ── Roles ──
        foreach (var role in new[] { "Admin", "Usuario", "Auditor" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // ── Usuario administrador por defecto ──
        if (await userManager.FindByNameAsync("admin") is null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@inventario.local",
                FullName = "Administrador",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                logger.LogInformation("Usuario admin creado (admin / Admin123!)");
            }
            else
            {
                logger.LogError("No se pudo crear el usuario admin: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // ── Datos de ejemplo ──
        if (!await db.Warehouses.AnyAsync())
        {
            var central = new Warehouse { Code = "ALM-01", Name = "Almacén Central", Address = "Av. Principal 100" };
            var sucursal = new Warehouse { Code = "ALM-02", Name = "Almacén Sucursal Norte", Address = "Calle Norte 45" };
            db.Warehouses.AddRange(central, sucursal);

            var electronicos = new Category { Name = "Electrónica", Description = "Dispositivos y accesorios electrónicos" };
            var oficina = new Category { Name = "Oficina", Description = "Materiales de oficina" };
            var limpieza = new Category { Name = "Limpieza", Description = "Insumos de limpieza" };
            db.Categories.AddRange(electronicos, oficina, limpieza);

            var proveedor1 = new Supplier
            {
                Name = "TecnoDistribuidora S.A.",
                ContactName = "María Gómez",
                Phone = "+54 11 5555-0001",
                Email = "ventas@tecno.com",
                Address = "Industrial 230"
            };
            var proveedor2 = new Supplier
            {
                Name = "Ofimax",
                ContactName = "Juan Pérez",
                Phone = "+54 11 5555-0002",
                Email = "contacto@ofimax.com",
                Address = "Comercio 45"
            };
            db.Suppliers.AddRange(proveedor1, proveedor2);

            await db.SaveChangesAsync();

            var products = new[]
            {
                new Product { Code = "P-0001", Name = "Teclado Mecánico", Description = "Teclado mecánico RGB", CategoryId = electronicos.Id, SupplierId = proveedor1.Id, UnitOfMeasure = UnitOfMeasure.Unidad, PurchasePrice = 25000m, SalePrice = 39999m, MinimumStock = 10 },
                new Product { Code = "P-0002", Name = "Mouse Inalámbrico", Description = "Mouse óptico inalámbrico", CategoryId = electronicos.Id, SupplierId = proveedor1.Id, UnitOfMeasure = UnitOfMeasure.Unidad, PurchasePrice = 12000m, SalePrice = 19999m, MinimumStock = 15 },
                new Product { Code = "P-0003", Name = "Monitor 24\"", Description = "Monitor LED 24 pulgadas", CategoryId = electronicos.Id, SupplierId = proveedor1.Id, UnitOfMeasure = UnitOfMeasure.Unidad, PurchasePrice = 180000m, SalePrice = 249999m, MinimumStock = 5 },
                new Product { Code = "P-0004", Name = "Resma A4", Description = "Resma de papel A4 75g", CategoryId = oficina.Id, SupplierId = proveedor2.Id, UnitOfMeasure = UnitOfMeasure.Unidad, PurchasePrice = 4500m, SalePrice = 6999m, MinimumStock = 20 },
                new Product { Code = "P-0005", Name = "Tóner HP 85A", Description = "Tóner negro compatible", CategoryId = oficina.Id, SupplierId = proveedor2.Id, UnitOfMeasure = UnitOfMeasure.Unidad, PurchasePrice = 32000m, SalePrice = 47999m, MinimumStock = 8 },
                new Product { Code = "P-0006", Name = "Detergente 5L", Description = "Detergente multiuso", CategoryId = limpieza.Id, SupplierId = null, UnitOfMeasure = UnitOfMeasure.Litro, PurchasePrice = 8000m, SalePrice = 12999m, MinimumStock = 12 },
                new Product { Code = "P-0007", Name = "Guantes de Nitrilo (x100)", Description = "Guantes descartables", CategoryId = limpieza.Id, SupplierId = null, UnitOfMeasure = UnitOfMeasure.Paquete, PurchasePrice = 9500m, SalePrice = 15999m, MinimumStock = 10 },
                new Product { Code = "P-0008", Name = "Auriculares USB", Description = "Auriculares con micrófono", CategoryId = electronicos.Id, SupplierId = proveedor1.Id, UnitOfMeasure = UnitOfMeasure.Unidad, PurchasePrice = 15000m, SalePrice = 25999m, MinimumStock = 6 }
            };
            db.Products.AddRange(products);

            await db.SaveChangesAsync();

            // Stock inicial vía movimientos de entrada
            var adminUser = await userManager.FindByNameAsync("admin");
            var stockInicial = new (int ProductoIndex, int Almacen, int Cantidad)[]
            {
                (0, central.Id, 25), (1, central.Id, 40), (2, central.Id, 8),
                (3, central.Id, 120), (4, central.Id, 6),
                (5, central.Id, 30), (6, central.Id, 5),
                (7, sucursal.Id, 10), (0, sucursal.Id, 5), (3, sucursal.Id, 45)
            };

            foreach (var (idx, almacenId, cantidad) in stockInicial)
            {
                var p = products[idx];
                db.Stocks.Add(new Stock { ProductId = p.Id, WarehouseId = almacenId, Quantity = cantidad });
                db.Movements.Add(new Movement
                {
                    Date = DateTime.UtcNow.AddDays(-7),
                    Type = MovementType.Entrada,
                    Reason = "Carga inicial de inventario",
                    Quantity = cantidad,
                    StockAfter = cantidad,
                    ProductId = p.Id,
                    WarehouseId = almacenId,
                    UserId = adminUser?.Id ?? string.Empty,
                    UserName = adminUser?.UserName ?? "admin"
                });
            }

            await db.SaveChangesAsync();

            // Alertas de ejemplo: productos que ya nacen por debajo del mínimo
            foreach (var p in products.Where(p => p.MinimumStock > 0))
            {
                var total = await db.Stocks
                    .Where(s => s.ProductId == p.Id)
                    .SumAsync(s => (int?)s.Quantity) ?? 0;

                if (total > p.MinimumStock) continue;

                foreach (var s in await db.Stocks.Where(s => s.ProductId == p.Id).ToListAsync())
                {
                    if (s.Quantity > p.MinimumStock) continue;

                    db.LowStockNotifications.Add(new LowStockNotification
                    {
                        ProductId = p.Id,
                        WarehouseId = s.WarehouseId,
                        QuantityAtDetection = s.Quantity,
                        MinimumStock = p.MinimumStock,
                        DetectedAt = DateTime.UtcNow.AddDays(-2)
                    });
                }
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Datos de ejemplo creados correctamente.");
        }
    }
}

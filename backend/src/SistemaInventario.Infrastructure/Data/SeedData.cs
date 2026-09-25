using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Entities;
using SistemaInventario.Core.Enums;
using SistemaInventario.Infrastructure.Auth;

namespace SistemaInventario.Infrastructure.Data;

public static class SeedData
{
    public static readonly string AdminPassword = "Admin123!";
    public static readonly string UserPassword = "Usuario123!";

    public static async Task InitializeAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var roles = new List<Role>
        {
            new() { Name = "Admin", Description = "Acceso total al sistema" },
            new() { Name = "Usuario", Description = "Gestión operativa de inventario" },
            new() { Name = "Auditor", Description = "Solo lectura y consultas" }
        };
        db.AddRange(roles);
        await db.SaveChangesAsync();

        var admin = new User
        {
            Username = "admin",
            PasswordHash = PasswordHasher.Hash(AdminPassword),
            FullName = "Administrador General",
            Email = "admin@sistema.com",
            RoleId = roles[0].Id
        };
        var operatorUser = new User
        {
            Username = "operador",
            PasswordHash = PasswordHasher.Hash(UserPassword),
            FullName = "Operador de Almacén",
            Email = "operador@sistema.com",
            RoleId = roles[1].Id
        };
        var auditor = new User
        {
            Username = "auditor",
            PasswordHash = PasswordHasher.Hash(UserPassword),
            FullName = "Auditor de Inventarios",
            Email = "auditor@sistema.com",
            RoleId = roles[2].Id
        };
        db.AddRange(admin, operatorUser, auditor);
        await db.SaveChangesAsync();

        var categories = new List<Category>
        {
            new() { Name = "Electrónica", Description = "Equipos y componentes electrónicos" },
            new() { Name = "Oficina", Description = "Suministros y materiales de oficina" },
            new() { Name = "Herramientas", Description = "Herramientas manuales y eléctricas" },
            new() { Name = "Limpieza", Description = "Productos de limpieza y aseo" },
            new() { Name = "Seguridad", Description = "Equipos de protección y seguridad" }
        };
        db.AddRange(categories);
        await db.SaveChangesAsync();

        var suppliers = new List<Supplier>
        {
            new() { Name = "Distribuidora Nacional S.A.", ContactName = "Carlos Méndez", Phone = "+56 2 2345 6789", Email = "ventas@dnacional.cl", Address = "Av. Providencia 1234, Santiago" },
            new() { Name = "TecnoImport SpA", ContactName = "Lucía Fernández", Phone = "+56 2 2987 4321", Email = "contacto@tecnoimport.cl", Address = "Calle Industrial 567, Quilicura" },
            new() { Name = "Suministros del Sur Ltda.", ContactName = "Roberto Soto", Phone = "+56 4 2234 8899", Email = "pedidos@sursuministros.cl", Address = "Ruta 5 Sur Km 12, Talca" }
        };
        db.AddRange(suppliers);
        await db.SaveChangesAsync();

        var warehouses = new List<Warehouse>
        {
            new() { Name = "Almacén Central", Code = "ALM-01", Location = "Santiago — Bodega principal" },
            new() { Name = "Almacén Norte", Code = "ALM-02", Location = "Antofagasta" },
            new() { Name = "Almacén Sur", Code = "ALM-03", Location = "Temuco" }
        };
        db.AddRange(warehouses);
        await db.SaveChangesAsync();

        var products = new List<Product>
        {
            new() { Code = "PRD-0001", Name = "Notebook 14\" Core i5", Description = "Notebook corporativo 16 GB RAM, 512 GB SSD", CategoryId = categories[0].Id, SupplierId = suppliers[1].Id, PurchasePrice = 520000, SalePrice = 699990, Unit = "Unidad", MinStock = 5 },
            new() { Code = "PRD-0002", Name = "Mouse inalámbrico ergonómico", Description = "Mouse óptico sin cable, USB 2.4 GHz", CategoryId = categories[0].Id, SupplierId = suppliers[1].Id, PurchasePrice = 7500, SalePrice = 12990, Unit = "Unidad", MinStock = 20 },
            new() { Code = "PRD-0003", Name = "Teclado mecánico RGB", Description = "Switches rojos, retroiluminación configurable", CategoryId = categories[0].Id, SupplierId = suppliers[1].Id, PurchasePrice = 25000, SalePrice = 39990, Unit = "Unidad", MinStock = 10 },
            new() { Code = "PRD-0004", Name = "Monitor 24\" Full HD", Description = "Panel IPS 75 Hz, HDMI + VGA", CategoryId = categories[0].Id, SupplierId = suppliers[0].Id, PurchasePrice = 98000, SalePrice = 139990, Unit = "Unidad", MinStock = 8 },
            new() { Code = "PRD-0005", Name = "Resma papel carta 75 g", Description = "500 hojas, blanco natural", CategoryId = categories[1].Id, SupplierId = suppliers[2].Id, PurchasePrice = 3900, SalePrice = 5490, Unit = "Resma", MinStock = 30 },
            new() { Code = "PRD-0006", Name = "Tóner negro 2.6k páginas", Description = "Compatible con impresoras láser A4", CategoryId = categories[1].Id, SupplierId = suppliers[0].Id, PurchasePrice = 32000, SalePrice = 45990, Unit = "Unidad", MinStock = 6 },
            new() { Code = "PRD-0007", Name = "Set desarmadores 12 piezas", Description = "Varillas de acero con mango antideslizante", CategoryId = categories[2].Id, SupplierId = suppliers[2].Id, PurchasePrice = 9800, SalePrice = 15990, Unit = "Set", MinStock = 12 },
            new() { Code = "PRD-0008", Name = "Taladro percutor 650 W", Description = "Mandril 13 mm, 2 velocidades", CategoryId = categories[2].Id, SupplierId = suppliers[1].Id, PurchasePrice = 54000, SalePrice = 74990, Unit = "Unidad", MinStock = 4 },
            new() { Code = "PRD-0009", Name = "Detergente industrial 5 L", Description = "Concentrado para limpieza de pisos", CategoryId = categories[3].Id, SupplierId = suppliers[2].Id, PurchasePrice = 8200, SalePrice = 11990, Unit = "Bidón", MinStock = 15 },
            new() { Code = "PRD-0010", Name = "Guantes de nitrilo (caja 100)", Description = "Talla M, uso general", CategoryId = categories[4].Id, SupplierId = suppliers[0].Id, PurchasePrice = 6500, SalePrice = 9990, Unit = "Caja", MinStock = 25 },
            new() { Code = "PRD-0011", Name = "Casco de seguridad blanco", Description = "Casco tipo M con ajuste de leva", CategoryId = categories[4].Id, SupplierId = suppliers[2].Id, PurchasePrice = 4800, SalePrice = 7990, Unit = "Unidad", MinStock = 20 },
            new() { Code = "PRD-0012", Name = "Cable de red Cat6 (305 m)", Description = "Bobina UTP sin oxígeno", CategoryId = categories[0].Id, SupplierId = suppliers[1].Id, PurchasePrice = 78000, SalePrice = 109990, Unit = "Bobina", MinStock = 3 }
        };
        db.AddRange(products);
        await db.SaveChangesAsync();

        var random = new Random(20260924);
        var now = DateTime.UtcNow;

        foreach (var product in products)
        {
            foreach (var warehouse in warehouses)
            {
                var qty = random.Next(0, 120);
                db.Add(new StockLevel { ProductId = product.Id, WarehouseId = warehouse.Id, Quantity = qty });
            }
        }
        await db.SaveChangesAsync();

        var reasons = new List<string>
        {
            "Ingreso por compra a proveedor",
            "Despacho a área de ventas",
            "Ajuste por inventario cíclico",
            "Devolución de cliente",
            "Traspaso entre almacenes",
            "Merma por rotura"
        };

        var movements = new List<Movement>();
        for (var i = 0; i < 90; i++)
        {
            var product = products[random.Next(products.Count)];
            var warehouse = warehouses[random.Next(warehouses.Count)];
            var type = random.Next(100) < 55 ? MovementType.Entrada : MovementType.Salida;
            var daysBack = random.Next(0, 45);
            var date = now.AddDays(-daysBack).AddHours(-random.Next(0, 12));
            var qty = random.Next(1, 25);

            var stock = await db.StockLevels
                .FirstOrDefaultAsync(s => s.ProductId == product.Id && s.WarehouseId == warehouse.Id);

            if (type == MovementType.Salida)
            {
                if (stock is null || stock.Quantity <= 0) continue;
                qty = Math.Min(qty, stock.Quantity);
                stock.Quantity -= qty;
            }
            else
            {
                if (stock is null)
                {
                    stock = new StockLevel { ProductId = product.Id, WarehouseId = warehouse.Id, Quantity = 0 };
                    db.Add(stock);
                }
                stock.Quantity += qty;
            }

            movements.Add(new Movement
            {
                Date = date,
                Type = type,
                Reason = reasons[random.Next(reasons.Count)],
                Quantity = qty,
                ProductId = product.Id,
                WarehouseId = warehouse.Id,
                UserId = random.Next(100) < 70 ? admin.Id : operatorUser.Id,
                DocumentReference = type == MovementType.Entrada ? $"OC-{random.Next(1000, 9999)}" : $"GD-{random.Next(1000, 9999)}",
                StockAfter = stock.Quantity,
                UnitPrice = type == MovementType.Entrada ? product.PurchasePrice : product.SalePrice
            });
        }
        db.AddRange(movements);
        await db.SaveChangesAsync();

        await GenerateLowStockNotificationsAsync(db);
        await db.SaveChangesAsync();
    }

    public static async Task GenerateLowStockNotificationsAsync(AppDbContext db)
    {
        var lows = await db.StockLevels
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.Product.IsActive && s.Quantity <= s.Product.MinStock)
            .ToListAsync();

        foreach (var level in lows)
        {
            var exists = await db.Notifications.AnyAsync(n =>
                n.ProductId == level.ProductId &&
                n.WarehouseId == level.WarehouseId &&
                !n.IsRead);

            if (exists) continue;

            db.Add(new AppNotification
            {
                ProductId = level.ProductId,
                WarehouseId = level.WarehouseId,
                Level = level.Quantity <= level.Product.MinStock / 2.0
                    ? NotificationLevel.Critical
                    : NotificationLevel.Warning,
                Message = $"Stock bajo: \"{level.Product.Name}\" tiene {level.Quantity} {level.Product.Unit.ToLower()} " +
                          $"(mínimo {level.Product.MinStock}) en {level.Warehouse.Name}."
            });
        }
    }
}

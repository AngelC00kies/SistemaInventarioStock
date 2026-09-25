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
        if (await db.Usuarios.AnyAsync()) return;

        var roles = new List<Rol>
        {
            new() { Nombre = "Admin", Descripcion = "Acceso total al sistema" },
            new() { Nombre = "Usuario", Descripcion = "Gestión operativa de inventario" },
            new() { Nombre = "Auditor", Descripcion = "Solo lectura y consultas" }
        };
        db.AddRange(roles);
        await db.SaveChangesAsync();

        var admin = new Usuario
        {
            NombreUsuario = "admin",
            ContrasenaHash = PasswordHasher.Hash(AdminPassword),
            NombreCompleto = "Administrador General",
            Correo = "admin@sistema.com",
            RolId = roles[0].Id
        };
        var operatorUser = new Usuario
        {
            NombreUsuario = "operador",
            ContrasenaHash = PasswordHasher.Hash(UserPassword),
            NombreCompleto = "Operador de Almacén",
            Correo = "operador@sistema.com",
            RolId = roles[1].Id
        };
        var auditor = new Usuario
        {
            NombreUsuario = "auditor",
            ContrasenaHash = PasswordHasher.Hash(UserPassword),
            NombreCompleto = "Auditor de Inventarios",
            Correo = "auditor@sistema.com",
            RolId = roles[2].Id
        };
        db.AddRange(admin, operatorUser, auditor);
        await db.SaveChangesAsync();

        var categorias = new List<Categoria>
        {
            new() { Nombre = "Electrónica", Descripcion = "Equipos y componentes electrónicos" },
            new() { Nombre = "Oficina", Descripcion = "Suministros y materiales de oficina" },
            new() { Nombre = "Herramientas", Descripcion = "Herramientas manuales y eléctricas" },
            new() { Nombre = "Limpieza", Descripcion = "Productos de limpieza y aseo" },
            new() { Nombre = "Seguridad", Descripcion = "Equipos de protección y seguridad" }
        };
        db.AddRange(categorias);
        await db.SaveChangesAsync();

        var proveedores = new List<Proveedor>
        {
            new() { Nombre = "Distribuidora Nacional S.A.", NombreContacto = "Carlos Méndez", Telefono = "+56 2 2345 6789", Correo = "ventas@dnacional.cl", Direccion = "Av. Providencia 1234, Santiago" },
            new() { Nombre = "TecnoImport SpA", NombreContacto = "Lucía Fernández", Telefono = "+56 2 2987 4321", Correo = "contacto@tecnoimport.cl", Direccion = "Calle Industrial 567, Quilicura" },
            new() { Nombre = "Suministros del Sur Ltda.", NombreContacto = "Roberto Soto", Telefono = "+56 4 2234 8899", Correo = "pedidos@sursuministros.cl", Direccion = "Ruta 5 Sur Km 12, Talca" }
        };
        db.AddRange(proveedores);
        await db.SaveChangesAsync();

        var almacenes = new List<Almacen>
        {
            new() { Nombre = "Almacén Central", Codigo = "ALM-01", Ubicacion = "Santiago — Bodega principal" },
            new() { Nombre = "Almacén Norte", Codigo = "ALM-02", Ubicacion = "Antofagasta" },
            new() { Nombre = "Almacén Sur", Codigo = "ALM-03", Ubicacion = "Temuco" }
        };
        db.AddRange(almacenes);
        await db.SaveChangesAsync();

        var productos = new List<Producto>
        {
            new() { Codigo = "PRD-0001", Nombre = "Notebook 14\" Core i5", Descripcion = "Notebook corporativo 16 GB RAM, 512 GB SSD", CategoriaId = categorias[0].Id, ProveedorId = proveedores[1].Id, PrecioCompra = 520000, PrecioVenta = 699990, Unidad = "Unidad", StockMinimo = 5 },
            new() { Codigo = "PRD-0002", Nombre = "Mouse inalámbrico ergonómico", Descripcion = "Mouse óptico sin cable, USB 2.4 GHz", CategoriaId = categorias[0].Id, ProveedorId = proveedores[1].Id, PrecioCompra = 7500, PrecioVenta = 12990, Unidad = "Unidad", StockMinimo = 20 },
            new() { Codigo = "PRD-0003", Nombre = "Teclado mecánico RGB", Descripcion = "Switches rojos, retroiluminación configurable", CategoriaId = categorias[0].Id, ProveedorId = proveedores[1].Id, PrecioCompra = 25000, PrecioVenta = 39990, Unidad = "Unidad", StockMinimo = 10 },
            new() { Codigo = "PRD-0004", Nombre = "Monitor 24\" Full HD", Descripcion = "Panel IPS 75 Hz, HDMI + VGA", CategoriaId = categorias[0].Id, ProveedorId = proveedores[0].Id, PrecioCompra = 98000, PrecioVenta = 139990, Unidad = "Unidad", StockMinimo = 8 },
            new() { Codigo = "PRD-0005", Nombre = "Resma papel carta 75 g", Descripcion = "500 hojas, blanco natural", CategoriaId = categorias[1].Id, ProveedorId = proveedores[2].Id, PrecioCompra = 3900, PrecioVenta = 5490, Unidad = "Resma", StockMinimo = 30 },
            new() { Codigo = "PRD-0006", Nombre = "Tóner negro 2.6k páginas", Descripcion = "Compatible con impresoras láser A4", CategoriaId = categorias[1].Id, ProveedorId = proveedores[0].Id, PrecioCompra = 32000, PrecioVenta = 45990, Unidad = "Unidad", StockMinimo = 6 },
            new() { Codigo = "PRD-0007", Nombre = "Set desarmadores 12 piezas", Descripcion = "Varillas de acero con mango antideslizante", CategoriaId = categorias[2].Id, ProveedorId = proveedores[2].Id, PrecioCompra = 9800, PrecioVenta = 15990, Unidad = "Set", StockMinimo = 12 },
            new() { Codigo = "PRD-0008", Nombre = "Taladro percutor 650 W", Descripcion = "Mandril 13 mm, 2 velocidades", CategoriaId = categorias[2].Id, ProveedorId = proveedores[1].Id, PrecioCompra = 54000, PrecioVenta = 74990, Unidad = "Unidad", StockMinimo = 4 },
            new() { Codigo = "PRD-0009", Nombre = "Detergente industrial 5 L", Descripcion = "Concentrado para limpieza de pisos", CategoriaId = categorias[3].Id, ProveedorId = proveedores[2].Id, PrecioCompra = 8200, PrecioVenta = 11990, Unidad = "Bidón", StockMinimo = 15 },
            new() { Codigo = "PRD-0010", Nombre = "Guantes de nitrilo (caja 100)", Descripcion = "Talla M, uso general", CategoriaId = categorias[4].Id, ProveedorId = proveedores[0].Id, PrecioCompra = 6500, PrecioVenta = 9990, Unidad = "Caja", StockMinimo = 25 },
            new() { Codigo = "PRD-0011", Nombre = "Casco de seguridad blanco", Descripcion = "Casco tipo M con ajuste de leva", CategoriaId = categorias[4].Id, ProveedorId = proveedores[2].Id, PrecioCompra = 4800, PrecioVenta = 7990, Unidad = "Unidad", StockMinimo = 20 },
            new() { Codigo = "PRD-0012", Nombre = "Cable de red Cat6 (305 m)", Descripcion = "Bobina UTP sin oxígeno", CategoriaId = categorias[0].Id, ProveedorId = proveedores[1].Id, PrecioCompra = 78000, PrecioVenta = 109990, Unidad = "Bobina", StockMinimo = 3 }
        };
        db.AddRange(productos);
        await db.SaveChangesAsync();

        var random = new Random(20260924);
        var now = DateTime.UtcNow;

        foreach (var producto in productos)
        {
            foreach (var almacen in almacenes)
            {
                var qty = random.Next(0, 120);
                db.Add(new NivelStock { ProductoId = producto.Id, AlmacenId = almacen.Id, Cantidad = qty });
            }
        }
        await db.SaveChangesAsync();

        var motivos = new List<string>
        {
            "Ingreso por compra a proveedor",
            "Despacho a área de ventas",
            "Ajuste por inventario cíclico",
            "Devolución de cliente",
            "Traspaso entre almacenes",
            "Merma por rotura"
        };

        var movimientos = new List<Movimiento>();
        for (var i = 0; i < 90; i++)
        {
            var producto = productos[random.Next(productos.Count)];
            var almacen = almacenes[random.Next(almacenes.Count)];
            var type = random.Next(100) < 55 ? MovementType.Entrada : MovementType.Salida;
            var daysBack = random.Next(0, 45);
            var date = now.AddDays(-daysBack).AddHours(-random.Next(0, 12));
            var qty = random.Next(1, 25);

            var stock = await db.NivelesStock
                .FirstOrDefaultAsync(s => s.ProductoId == producto.Id && s.AlmacenId == almacen.Id);

            if (type == MovementType.Salida)
            {
                if (stock is null || stock.Cantidad <= 0) continue;
                qty = Math.Min(qty, stock.Cantidad);
                stock.Cantidad -= qty;
            }
            else
            {
                if (stock is null)
                {
                    stock = new NivelStock { ProductoId = producto.Id, AlmacenId = almacen.Id, Cantidad = 0 };
                    db.Add(stock);
                }
                stock.Cantidad += qty;
            }

            movimientos.Add(new Movimiento
            {
                Fecha = date,
                Tipo = type,
                Motivo = motivos[random.Next(motivos.Count)],
                Cantidad = qty,
                ProductoId = producto.Id,
                AlmacenId = almacen.Id,
                UsuarioId = random.Next(100) < 70 ? admin.Id : operatorUser.Id,
                DocumentoReferencia = type == MovementType.Entrada ? $"OC-{random.Next(1000, 9999)}" : $"GD-{random.Next(1000, 9999)}",
                StockResultante = stock.Cantidad,
                PrecioUnitario = type == MovementType.Entrada ? producto.PrecioCompra : producto.PrecioVenta
            });
        }
        db.AddRange(movimientos);
        await db.SaveChangesAsync();

        await GenerateLowStockNotificationsAsync(db);
        await db.SaveChangesAsync();
    }

    public static async Task GenerateLowStockNotificationsAsync(AppDbContext db)
    {
        var lows = await db.NivelesStock
            .Include(s => s.Producto)
            .Include(s => s.Almacen)
            .Where(s => s.Producto.Activo && s.Cantidad <= s.Producto.StockMinimo)
            .ToListAsync();

        foreach (var level in lows)
        {
            var exists = await db.Notificaciones.AnyAsync(n =>
                n.ProductoId == level.ProductoId &&
                n.AlmacenId == level.AlmacenId &&
                !n.Leida);

            if (exists) continue;

            db.Add(new Notificacion
            {
                ProductoId = level.ProductoId,
                AlmacenId = level.AlmacenId,
                Nivel = level.Cantidad <= level.Producto.StockMinimo / 2.0
                    ? NotificationLevel.Critical
                    : NotificationLevel.Warning,
                Mensaje = $"Stock bajo: \"{level.Producto.Nombre}\" tiene {level.Cantidad} {level.Producto.Unidad.ToLower()} " +
                          $"(mínimo {level.Producto.StockMinimo}) en {level.Almacen.Nombre}."
            });
        }
    }
}

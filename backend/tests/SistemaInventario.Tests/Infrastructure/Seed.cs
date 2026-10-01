using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Entities;
using SistemaInventario.Core.Enums;
using SistemaInventario.Infrastructure.Auth;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Tests.Infrastructure;

/// <summary>
/// Constructores de datos de partida para las pruebas. Cada método da de alta la entidad,
/// la guarda y la devuelve, de modo que un test solo expresa lo que necesita empezar.
///
/// Los valores por defecto (código, nombre, código de almacén...) son fijos: al tener cada
/// prueba su propia base, no hace falta hacerlos únicos a mano. Cuando un test necesita
/// varias filas de la misma tabla se le pasan valores explícitos.
/// </summary>
internal static class Seed
{
    public static async Task<Rol> RolAsync(AppDbContext db, string nombre = "Admin", string descripcion = "Administrador")
    {
        var rol = new Rol { Nombre = nombre, Descripcion = descripcion };
        db.Roles.Add(rol);
        await db.SaveChangesAsync();
        return rol;
    }

    public static async Task<Usuario> UsuarioAsync(
        AppDbContext db,
        int rolId,
        string nombreUsuario = "admin",
        string? contrasena = null,
        bool activo = true)
    {
        var usuario = new Usuario
        {
            NombreUsuario = nombreUsuario,
            // Solo se guarda el hash: ninguna prueba debe dejar la contraseña en claro.
            ContrasenaHash = PasswordHasher.Hash(contrasena ?? "Admin123!"),
            NombreCompleto = nombreUsuario.ToUpperInvariant(),
            Correo = $"{nombreUsuario}@correo.cl",
            RolId = rolId,
            Activo = activo
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return usuario;
    }

    public static async Task<Categoria> CategoriaAsync(AppDbContext db, string nombre = "Electrónica", bool activo = true)
    {
        var categoria = new Categoria { Nombre = nombre, Activo = activo };
        db.Categorias.Add(categoria);
        await db.SaveChangesAsync();
        return categoria;
    }

    public static async Task<Proveedor> ProveedorAsync(AppDbContext db, string nombre = "ACME", bool activo = true)
    {
        var proveedor = new Proveedor { Nombre = nombre, Activo = activo };
        db.Proveedores.Add(proveedor);
        await db.SaveChangesAsync();
        return proveedor;
    }

    public static async Task<Almacen> AlmacenAsync(
        AppDbContext db,
        string codigo = "ALM-01",
        string nombre = "Principal",
        bool activo = true)
    {
        var almacen = new Almacen { Codigo = codigo, Nombre = nombre, Activo = activo };
        db.Almacenes.Add(almacen);
        await db.SaveChangesAsync();
        return almacen;
    }

    public static async Task<Producto> ProductoAsync(
        AppDbContext db,
        int categoriaId,
        int? proveedorId = null,
        string codigo = "P-001",
        string nombre = "Producto",
        int stockMinimo = 5,
        bool activo = true,
        decimal precioCompra = 1000m,
        decimal precioVenta = 1500m)
    {
        var producto = new Producto
        {
            Codigo = codigo,
            Nombre = nombre,
            CategoriaId = categoriaId,
            ProveedorId = proveedorId,
            PrecioCompra = precioCompra,
            PrecioVenta = precioVenta,
            StockMinimo = stockMinimo,
            Activo = activo
        };
        db.Productos.Add(producto);
        await db.SaveChangesAsync();
        return producto;
    }

    /// <summary>Crea o actualiza las existencias de un producto en un almacén.</summary>
    public static async Task<NivelStock> StockAsync(AppDbContext db, int productoId, int almacenId, int cantidad)
    {
        var nivel = await db.NivelesStock
            .FirstOrDefaultAsync(s => s.ProductoId == productoId && s.AlmacenId == almacenId);

        if (nivel is null)
        {
            nivel = new NivelStock { ProductoId = productoId, AlmacenId = almacenId };
            db.NivelesStock.Add(nivel);
        }

        nivel.Cantidad = cantidad;
        await db.SaveChangesAsync();
        return nivel;
    }

    public static async Task<Movimiento> MovimientoAsync(
        AppDbContext db,
        int productoId,
        int almacenId,
        int usuarioId,
        MovementType tipo = MovementType.Entrada,
        int cantidad = 10,
        DateTime? fecha = null,
        decimal? precioUnitario = null,
        string motivo = "Reposición")
    {
        var movimiento = new Movimiento
        {
            // Por defecto "hace una hora": así cae dentro de hoy y del mes en curso del panel.
            Fecha = fecha ?? DateTime.UtcNow.AddHours(-1),
            Tipo = tipo,
            Motivo = motivo,
            Cantidad = cantidad,
            ProductoId = productoId,
            AlmacenId = almacenId,
            UsuarioId = usuarioId,
            StockResultante = cantidad,
            PrecioUnitario = precioUnitario ?? 1000m
        };
        db.Movimientos.Add(movimiento);
        await db.SaveChangesAsync();
        return movimiento;
    }

    public static async Task<Notificacion> NotificacionAsync(
        AppDbContext db,
        int productoId,
        int almacenId,
        NotificationLevel nivel = NotificationLevel.Warning,
        bool leida = false,
        DateTime? fecha = null)
    {
        var notificacion = new Notificacion
        {
            ProductoId = productoId,
            AlmacenId = almacenId,
            Nivel = nivel,
            Leida = leida,
            Mensaje = "Stock bajo",
            FechaCreacion = fecha ?? DateTime.UtcNow
        };
        db.Notificaciones.Add(notificacion);
        await db.SaveChangesAsync();
        return notificacion;
    }
}

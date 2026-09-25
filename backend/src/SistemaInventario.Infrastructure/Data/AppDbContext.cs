using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Entities;
using SistemaInventario.Core.Enums;

namespace SistemaInventario.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Almacen> Almacenes => Set<Almacen>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<NivelStock> NivelesStock => Set<NivelStock>();
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("Roles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.NombreUsuario).HasMaxLength(60).IsRequired();
            e.Property(x => x.ContrasenaHash).HasMaxLength(200).IsRequired();
            e.Property(x => x.NombreCompleto).HasMaxLength(150).IsRequired();
            e.Property(x => x.Correo).HasMaxLength(150);
            e.HasIndex(x => x.NombreUsuario).IsUnique();
            e.HasOne(x => x.Rol).WithMany(r => r.Usuarios).HasForeignKey(x => x.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Categoria>(e =>
        {
            e.ToTable("Categorias");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(400);
            e.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<Proveedor>(e =>
        {
            e.ToTable("Proveedores");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
            e.Property(x => x.NombreContacto).HasMaxLength(150);
            e.Property(x => x.Telefono).HasMaxLength(50);
            e.Property(x => x.Correo).HasMaxLength(150);
            e.Property(x => x.Direccion).HasMaxLength(300);
            e.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<Almacen>(e =>
        {
            e.ToTable("Almacenes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            e.Property(x => x.Codigo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Ubicacion).HasMaxLength(200);
            e.HasIndex(x => x.Codigo).IsUnique();
        });

        modelBuilder.Entity<Producto>(e =>
        {
            e.ToTable("Productos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Codigo).HasMaxLength(40).IsRequired();
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(800);
            e.Property(x => x.Unidad).HasMaxLength(30).IsRequired();
            e.Property(x => x.PrecioCompra).HasPrecision(18, 2);
            e.Property(x => x.PrecioVenta).HasPrecision(18, 2);
            e.HasIndex(x => x.Codigo).IsUnique();
            e.HasOne(x => x.Categoria).WithMany(c => c.Productos).HasForeignKey(x => x.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Proveedor).WithMany(s => s.Productos).HasForeignKey(x => x.ProveedorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<NivelStock>(e =>
        {
            e.ToTable("NivelesStock");
            e.HasKey(x => new { x.ProductoId, x.AlmacenId });
            e.HasOne(x => x.Producto).WithMany(p => p.NivelesStock).HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Almacen).WithMany(w => w.NivelesStock).HasForeignKey(x => x.AlmacenId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Movimiento>(e =>
        {
            e.ToTable("Movimientos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Motivo).HasMaxLength(300).IsRequired();
            e.Property(x => x.DocumentoReferencia).HasMaxLength(80);
            e.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
            e.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.Fecha);
            e.HasOne(x => x.Producto).WithMany(p => p.Movimientos).HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Almacen).WithMany(w => w.Movimientos).HasForeignKey(x => x.AlmacenId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Usuario).WithMany(u => u.Movimientos).HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notificacion>(e =>
        {
            e.ToTable("Notificaciones");
            e.HasKey(x => x.Id);
            e.Property(x => x.Mensaje).HasMaxLength(400).IsRequired();
            e.Property(x => x.Nivel).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.Leida);
            e.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Almacen).WithMany().HasForeignKey(x => x.AlmacenId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

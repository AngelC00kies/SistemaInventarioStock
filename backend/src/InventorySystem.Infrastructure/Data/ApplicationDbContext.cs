using InventorySystem.Application.Common;
using InventorySystem.Domain.Common;
using InventorySystem.Domain.Entities;
using InventorySystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<Movement> Movements => Set<Movement>();
    public DbSet<LowStockNotification> LowStockNotifications => Set<LowStockNotification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        MapIdentityToSpanish(builder);

        // Índices únicos
        builder.Entity<Category>().HasIndex(c => c.Name).IsUnique();
        builder.Entity<Supplier>().HasIndex(s => s.Name).IsUnique();
        builder.Entity<Warehouse>().HasIndex(w => w.Code).IsUnique();
        builder.Entity<Product>().HasIndex(p => p.Code).IsUnique();

        // Precios monetarios
        builder.Entity<Product>().Property(p => p.PurchasePrice).HasPrecision(18, 2);
        builder.Entity<Product>().Property(p => p.SalePrice).HasPrecision(18, 2);

        // Enums como texto (legibles en reportes/SQL)
        builder.Entity<Product>().Property(p => p.UnitOfMeasure)
            .HasConversion<string>().HasMaxLength(30);
        builder.Entity<Movement>().Property(m => m.Type)
            .HasConversion<string>().HasMaxLength(20);

        // Auditoría global
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType)
                    .Property<DateTime>(nameof(BaseEntity.CreatedAt));
                builder.Entity(entityType.ClrType)
                    .Property<DateTime?>(nameof(BaseEntity.UpdatedAt));
                builder.Entity(entityType.ClrType)
                    .Property<bool>(nameof(BaseEntity.IsActive));
            }
        }

        // Stock: PK compuesta producto + almacén
        builder.Entity<Stock>(e =>
        {
            e.HasKey(s => new { s.ProductId, s.WarehouseId });
            e.HasOne(s => s.Product)
                .WithMany(p => p.Stocks)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Warehouse)
                .WithMany(w => w.Stocks)
                .HasForeignKey(s => s.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Relaciones con protección de borrado
        builder.Entity<Category>()
            .HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Supplier>()
            .HasMany(s => s.Products)
            .WithOne(p => p.Supplier)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Movement>(e =>
        {
            e.HasOne(m => m.Product)
                .WithMany(p => p.Movements)
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.Warehouse)
                .WithMany()
                .HasForeignKey(m => m.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(m => m.Date);
            e.HasIndex(m => new { m.ProductId, m.Date });
            e.HasIndex(m => m.UserId);
        });

        builder.Entity<LowStockNotification>(e =>
        {
            e.HasOne(n => n.Product)
                .WithMany()
                .HasForeignKey(n => n.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(n => n.Warehouse)
                .WithMany()
                .HasForeignKey(n => n.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(n => new { n.ProductId, n.WarehouseId, n.IsResolved });
        });
    }

    /// <summary>
    /// Traduce al español las tablas y columnas de ASP.NET Core Identity
    /// (por defecto se crean como AspNetUsers, AspNetRoles, …).
    /// </summary>
    private static void MapIdentityToSpanish(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(e =>
        {
            e.ToTable("Usuarios");
            e.Property(u => u.UserName).HasColumnName("NombreUsuario");
            e.Property(u => u.NormalizedUserName).HasColumnName("NombreUsuarioNormalizado");
            e.Property(u => u.FullName).HasColumnName("NombreCompleto");
            e.Property(u => u.NormalizedEmail).HasColumnName("EmailNormalizado");
            e.Property(u => u.EmailConfirmed).HasColumnName("EmailConfirmado");
            e.Property(u => u.IsActive).HasColumnName("Activo");
            e.Property(u => u.RefreshToken).HasColumnName("TokenRefresco");
            e.Property(u => u.RefreshTokenExpiry).HasColumnName("TokenRefrescoVencimiento");
            e.Property(u => u.PasswordHash).HasColumnName("HashContrasena");
            e.Property(u => u.SecurityStamp).HasColumnName("MarcaSeguridad");
            e.Property(u => u.ConcurrencyStamp).HasColumnName("MarcaConcurrencia");
            e.Property(u => u.PhoneNumber).HasColumnName("Telefono");
            e.Property(u => u.PhoneNumberConfirmed).HasColumnName("TelefonoConfirmado");
            e.Property(u => u.TwoFactorEnabled).HasColumnName("DosFactoresHabilitado");
            e.Property(u => u.LockoutEnd).HasColumnName("BloqueoHasta");
            e.Property(u => u.LockoutEnabled).HasColumnName("BloqueoHabilitado");
            e.Property(u => u.AccessFailedCount).HasColumnName("IntentosFallidos");
        });

        builder.Entity<IdentityRole>(e =>
        {
            e.ToTable("Roles");
            e.Property(r => r.Name).HasColumnName("Nombre");
            e.Property(r => r.NormalizedName).HasColumnName("NombreNormalizado");
            e.Property(r => r.ConcurrencyStamp).HasColumnName("MarcaConcurrencia");
        });

        builder.Entity<IdentityUserRole<string>>(e =>
        {
            e.ToTable("UsuariosRoles");
            e.Property(r => r.UserId).HasColumnName("UsuarioId");
            e.Property(r => r.RoleId).HasColumnName("RolId");
        });

        builder.Entity<IdentityUserClaim<string>>(e =>
        {
            e.ToTable("UsuariosClaims");
            e.Property(c => c.UserId).HasColumnName("UsuarioId");
            e.Property(c => c.ClaimType).HasColumnName("TipoReclamacion");
            e.Property(c => c.ClaimValue).HasColumnName("ValorReclamacion");
        });

        builder.Entity<IdentityRoleClaim<string>>(e =>
        {
            e.ToTable("RolesClaims");
            e.Property(c => c.RoleId).HasColumnName("RolId");
            e.Property(c => c.ClaimType).HasColumnName("TipoReclamacion");
            e.Property(c => c.ClaimValue).HasColumnName("ValorReclamacion");
        });

        builder.Entity<IdentityUserLogin<string>>(e =>
        {
            e.ToTable("UsuariosLogins");
            e.Property(l => l.UserId).HasColumnName("UsuarioId");
            e.Property(l => l.LoginProvider).HasColumnName("ProveedorLogin");
            e.Property(l => l.ProviderKey).HasColumnName("ClaveProveedor");
            e.Property(l => l.ProviderDisplayName).HasColumnName("NombreProveedor");
        });

        builder.Entity<IdentityUserToken<string>>(e =>
        {
            e.ToTable("UsuariosTokens");
            e.Property(t => t.UserId).HasColumnName("UsuarioId");
            e.Property(t => t.LoginProvider).HasColumnName("ProveedorLogin");
            e.Property(t => t.Name).HasColumnName("Nombre");
            e.Property(t => t.Value).HasColumnName("Valor");
        });
    }
}

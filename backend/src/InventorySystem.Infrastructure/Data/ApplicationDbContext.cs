using InventorySystem.Application.Common;
using InventorySystem.Domain.Common;
using InventorySystem.Domain.Entities;
using InventorySystem.Infrastructure.Identity;
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
}

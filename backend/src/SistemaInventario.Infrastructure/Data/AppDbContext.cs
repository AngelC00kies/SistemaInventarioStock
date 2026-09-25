using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Entities;
using SistemaInventario.Core.Enums;

namespace SistemaInventario.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<Movement> Movements => Set<Movement>();
    public DbSet<AppNotification> Notifications => Set<AppNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("Roles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Username).HasMaxLength(60).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150);
            e.HasIndex(x => x.Username).IsUnique();
            e.HasOne(x => x.Role).WithMany(r => r.Users).HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("Categories");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(400);
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Supplier>(e =>
        {
            e.ToTable("Suppliers");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.ContactName).HasMaxLength(150);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Address).HasMaxLength(300);
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Warehouse>(e =>
        {
            e.ToTable("Warehouses");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.Code).HasMaxLength(30).IsRequired();
            e.Property(x => x.Location).HasMaxLength(200);
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("Products");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(40).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(800);
            e.Property(x => x.Unit).HasMaxLength(30).IsRequired();
            e.Property(x => x.PurchasePrice).HasPrecision(18, 2);
            e.Property(x => x.SalePrice).HasPrecision(18, 2);
            e.HasIndex(x => x.Code).IsUnique();
            e.HasOne(x => x.Category).WithMany(c => c.Products).HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Supplier).WithMany(s => s.Products).HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StockLevel>(e =>
        {
            e.ToTable("StockLevels");
            e.HasKey(x => new { x.ProductId, x.WarehouseId });
            e.HasOne(x => x.Product).WithMany(p => p.StockLevels).HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Warehouse).WithMany(w => w.StockLevels).HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Movement>(e =>
        {
            e.ToTable("Movements");
            e.HasKey(x => x.Id);
            e.Property(x => x.Reason).HasMaxLength(300).IsRequired();
            e.Property(x => x.DocumentReference).HasMaxLength(80);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.Date);
            e.HasOne(x => x.Product).WithMany(p => p.Movements).HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Warehouse).WithMany(w => w.Movements).HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.User).WithMany(u => u.Movements).HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppNotification>(e =>
        {
            e.ToTable("Notifications");
            e.HasKey(x => x.Id);
            e.Property(x => x.Message).HasMaxLength(400).IsRequired();
            e.Property(x => x.Level).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.IsRead);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

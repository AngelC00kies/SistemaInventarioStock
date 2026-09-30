using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaInventario.Core.Interfaces;
using SistemaInventario.Infrastructure.Data;
using SistemaInventario.Infrastructure.Services;

namespace SistemaInventario.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registra el DbContext de SQL Server y los servicios de infraestructura con ámbito scoped.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Las migraciones se declaran en este ensamblado para que "dotnet ef" las aplique sin referenciar la API.
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IMovementService, MovementService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}

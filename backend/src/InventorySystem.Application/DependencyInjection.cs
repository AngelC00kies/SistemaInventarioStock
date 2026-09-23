using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using InventorySystem.Application.Interfaces;
using InventorySystem.Application.Services;

namespace InventorySystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IMovementService, MovementService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();

        services.AddValidatorsFromAssemblyContaining<Validators.ProductRequestValidator>();

        return services;
    }
}

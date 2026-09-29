using InventorySystem.API.Middleware;
using InventorySystem.Infrastructure.Data;

namespace InventorySystem.API.Extensions;

/// <summary>Configuración del pipeline HTTP y de la base de datos.</summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Crea/aplica las migraciones y siembra roles, usuario administrador,
    /// catálogos, productos y alertas iniciales. Un fallo no detiene la API:
    /// se registra el error y el resto de la aplicación sigue funcionando.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            await DbSeeder.SeedAsync(scope.ServiceProvider);
            logger.LogInformation("Base de datos inicializada correctamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "No se pudo inicializar la base de datos. Verifique la cadena de conexión 'DefaultConnection'. La API continúa sin datos.");
        }
    }

    /// <summary>Middleware global, Swagger, CORS, autenticación y rutas.</summary>
    public static void UseInventoryPipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("Frontend");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapGet("/", () => Results.Redirect("/swagger"));
        app.MapGet("/health", () => Results.Ok(new { status = "healthy", time = DateTime.UtcNow }));
    }
}

using System.Text.Json;
using FluentValidation;
using InventorySystem.Domain.Exceptions;

namespace InventorySystem.API.Middleware;

/// <summary>Convierte excepciones de negocio y de validación en respuestas HTTP consistentes.</summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Excepción de negocio: {Message}", ex.Message);
            await WriteAsync(context, ex.StatusCode, ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Errores de validación");
            var errors = ex.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => char.ToLowerInvariant(g.Key[0]) + g.Key[1..], g => g.Select(e => e.ErrorMessage).ToArray());
            await WriteValidationAsync(context, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado");
            var message = _env.IsDevelopment()
                ? ex.Message
                : "Ocurrió un error inesperado. Intente nuevamente.";
            await WriteAsync(context, 500, message);
        }
    }

    private static async Task WriteAsync(HttpContext context, int status, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
    }

    private static async Task WriteValidationAsync(HttpContext context, Dictionary<string, string[]> errors)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            message = "Errores de validación",
            errors
        }));
    }
}

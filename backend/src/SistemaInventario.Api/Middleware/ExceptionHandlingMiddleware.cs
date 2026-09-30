using System.Text.Json;
using SistemaInventario.Core.Exceptions;

namespace SistemaInventario.Api.Middleware;

/// <summary>Convierte las excepciones de la petición en una respuesta JSON { message } con su código HTTP.</summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        // AppException y NotFoundException ya traen su código HTTP; lo no previsto se oculta tras un 500 genérico
        catch (AppException ex)
        {
            await WriteAsync(context, ex.StatusCode, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteAsync(context, 404, ex.Message);
        }
        catch (BadHttpRequestException ex)
        {
            await WriteAsync(context, 400, "Solicitud inválida: " + ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado al procesar {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await WriteAsync(context, 500, "Ocurrió un error inesperado. Intente nuevamente.");
        }
    }

    private static async Task WriteAsync(HttpContext context, int status, string message)
    {
        // Si la respuesta ya empezó a enviarse no puede reescribirse ni el estado ni el cuerpo
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new { message });
        await context.Response.WriteAsync(payload);
    }
}

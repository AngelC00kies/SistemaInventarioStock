namespace SistemaInventario.Core.Exceptions;

/// <summary>Error de negocio controlado: el middleware HTTP responde con su StatusCode y un JSON { message }.</summary>
public class AppException : Exception
{
    /// <summary>Código HTTP devuelto al cliente (400 por defecto; 401/403 para fallos de autenticación y acceso).</summary>
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

/// <summary>Recurso inexistente: el middleware la traduce siempre a un 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

namespace SistemaInventario.Core.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

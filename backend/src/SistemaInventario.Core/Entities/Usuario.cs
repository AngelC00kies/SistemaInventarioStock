namespace SistemaInventario.Core.Entities;

/// <summary>Cuenta de acceso al sistema, con su rol y su estado de habilitación.</summary>
public class Usuario
{
    public int Id { get; set; }
    /// <summary>Identificador visible con el que se entra; único en la tabla.</summary>
    public string NombreUsuario { get; set; } = string.Empty;
    /// <summary>Hash de la contraseña: nunca se guarda ni se devuelve el texto plano.</summary>
    public string ContrasenaHash { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    /// <summary>Null cuando el usuario no tiene correo registrado (no es obligatorio).</summary>
    public string? Correo { get; set; }
    public int RolId { get; set; }
    // Cuenta deshabilitada: existe y conserva su histórico, pero el login la rechaza con un 403.
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Rol Rol { get; set; } = null!;
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}

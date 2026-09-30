namespace SistemaInventario.Core.Entities;

/// <summary>Empresa de la que se compran los productos; conserva sus datos de contacto y qué productos le corresponden.</summary>
public class Proveedor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? NombreContacto { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    // Un proveedor inactivo se conserva por historial, pero ya no puede asignarse a productos nuevos.
    public bool Activo { get; set; } = true;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

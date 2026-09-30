namespace SistemaInventario.Core.Entities;

/// <summary>Ubicación física donde se acumula stock (tienda, depósito, sucursal...).</summary>
public class Almacen
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Código corto único entre almacenes, usado como identificador en la UI y en los informes.</summary>
    public string Codigo { get; set; } = string.Empty;
    public string? Ubicacion { get; set; }
    // Un almacén inactivo deja de aceptar movimientos nuevos; sus existencias siguen sumando en los totales.
    public bool Activo { get; set; } = true;

    public ICollection<NivelStock> NivelesStock { get; set; } = new List<NivelStock>();
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}

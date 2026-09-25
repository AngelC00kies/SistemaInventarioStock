namespace SistemaInventario.Core.Entities;

public class Almacen
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string? Ubicacion { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<NivelStock> NivelesStock { get; set; } = new List<NivelStock>();
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}

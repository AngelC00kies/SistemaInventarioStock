namespace SistemaInventario.Core.Entities;

/// <summary>Agrupación de productos usada para filtrar el catálogo y segmentar los informes.</summary>
public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    // Si tiene productos la eliminación es lógica: sólo se borra la fila cuando no referencia nada.
    public bool Activo { get; set; } = true;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

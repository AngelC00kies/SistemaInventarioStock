namespace SistemaInventario.Core.Entities;

/// <summary>Perfil de permisos que decide qué puede hacer cada usuario en la aplicación.</summary>
public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}

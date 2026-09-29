using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySystem.Domain.Common;

/// <summary>
/// Entidad base con auditoría y baja lógica.
/// Los nombres de columna se declaran en español: la propiedad en C# mantiene
/// el nombre en inglés (API/DTO) y EF Core hace el mapeo a SQL.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }

    [Column("FechaCreacion")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("FechaActualizacion")]
    public DateTime? UpdatedAt { get; set; }

    [Column("Activo")]
    public bool IsActive { get; set; } = true;
}

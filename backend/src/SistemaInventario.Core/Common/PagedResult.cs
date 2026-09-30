namespace SistemaInventario.Core.Common;

/// <summary>Página de resultados de una consulta paginada: los ítems junto a los totales que necesita la paginación.</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    /// <summary>Registros que cumplen el filtro en total, sin aplicar el corte por página.</summary>
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    // 0 cuando PageSize <= 0, para no dividir por cero antes de que el servicio recorte la página.
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);
}

namespace SistemaInventario.Core.Dtos;

/// <summary>Categoría con el contador que muestra su fila en el listado.</summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    /// <summary>Sólo cuenta productos activos: los desactivados no lo suman.</summary>
    public int ProductCount { get; set; }
}

/// <summary>Alta o edición de categoría; el nombre debe ser único.</summary>
public class CategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>Proveedor con su contacto y el contador de productos que abastece.</summary>
public class SupplierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    /// <summary>Sólo cuenta productos activos: los desactivados no lo suman.</summary>
    public int ProductCount { get; set; }
}

/// <summary>Alta o edición de proveedor; el nombre debe ser único.</summary>
public class SupplierRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>Almacén con sus dos métricas de ocupación.</summary>
public class WarehouseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    /// <summary>Productos con existencias (&gt; 0) aquí; también cuenta los de productos inactivos si aún tienen stock.</summary>
    public int ProductCount { get; set; }
    /// <summary>Suma de unidades de todos los productos guardados en este almacén.</summary>
    public int TotalUnits { get; set; }
}

/// <summary>Alta o edición de almacén; el código debe ser único.</summary>
public class WarehouseRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
}

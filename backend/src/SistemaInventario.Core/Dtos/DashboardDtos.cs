namespace SistemaInventario.Core.Dtos;

/// <summary>Respuesta única del panel de inicio: KPIs, gráficos y tablas resumen en una sola llamada.</summary>
public class DashboardDto
{
    /// <summary>Productos activos; los inactivos no cuentan.</summary>
    public int TotalProducts { get; set; }
    /// <summary>Unidades en existencia sumando todos los almacenes.</summary>
    public int TotalUnits { get; set; }
    /// <summary>Productos cuyo stock total no supera su mínimo.</summary>
    public int LowStockProducts { get; set; }
    /// <summary>Movimientos registrados desde medianoche (UTC).</summary>
    public int MovementsToday { get; set; }
    /// <summary>Valor (cantidad × precio unitario) de las entradas del mes en curso.</summary>
    public decimal MonthlyInValue { get; set; }
    /// <summary>Ídem de las salidas: valor de lo que salió del inventario este mes.</summary>
    public decimal MonthlyOutValue { get; set; }
    public int ActiveWarehouses { get; set; }
    /// <summary>Gráfico de flujo diario: las últimas 30 jornadas, aunque no hayan tenido movimientos.</summary>
    public List<DailyFlowDto> DailyFlow { get; set; } = new();
    public List<CategoryShareDto> CategoryShare { get; set; } = new();
    /// <summary>Hasta 8 productos más urgentes, del más desabastecido al menos.</summary>
    public List<LowStockDto> CriticalProducts { get; set; } = new();
    public List<MovementDto> RecentMovements { get; set; } = new();
}

/// <summary>Un día del gráfico de entradas y salidas, en unidades.</summary>
public class DailyFlowDto
{
    /// <summary>Fecha como texto "yyyy-MM-dd", lista para usarla como eje del gráfico.</summary>
    public string Date { get; set; } = string.Empty;
    public int Entries { get; set; }
    public int Exits { get; set; }
}

/// <summary>Peso de una categoría sobre el total: sus productos y sus unidades almacenadas.</summary>
public class CategoryShareDto
{
    public string Name { get; set; } = string.Empty;
    public int Products { get; set; }
    public int Units { get; set; }
}

/// <summary>Fila del listado de stock crítico, medida por almacén y no por producto.</summary>
public class LowStockDto
{
    public int ProductId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    /// <summary>Existencias en ese almacén, no el total del producto.</summary>
    public int Quantity { get; set; }
    public int MinStock { get; set; }
    /// <summary>Estado calculado frente al mínimo: "empty", "critical" o "low".</summary>
    public string Status { get; set; } = "low";
}

using SistemaInventario.Core.Enums;

namespace SistemaInventario.Core.Dtos;

/// <summary>Línea del informe de stock actual: una fila por combinación producto-almacén, ya aplanada.</summary>
public class StockReportRow
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    /// <summary>Nombre del proveedor, o "—" cuando el producto no tiene asignado.</summary>
    public string Supplier { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinStock { get; set; }
    public decimal SalePrice { get; set; }
    /// <summary>Valor de la línea: Quantity × precio de venta.</summary>
    public decimal StockValue { get; set; }
    /// <summary>Estado ya traducido al español ("OK", "Stock bajo", "Crítico", "Sin stock") para la celda del informe.</summary>
    public string Status { get; set; } = "ok";
}

/// <summary>Criterios del informe de stock; todo opcional y combinable.</summary>
public class StockReportFilter
{
    public int? WarehouseId { get; set; }
    public int? CategoryId { get; set; }
    /// <summary>Sólo líneas cuya cantidad no supera el mínimo del producto.</summary>
    public bool CriticalOnly { get; set; }
    public string? Search { get; set; }
}

/// <summary>Línea del informe de movimientos: el histórico de entradas y salidas con su valor.</summary>
public class MovementReportRow
{
    public DateTime Date { get; set; }
    /// <summary>Tipo escrito ("Entrada"/"Salida"), ya en texto para la celda del informe.</summary>
    public string Type { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public int StockAfter { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? DocumentReference { get; set; }
}

/// <summary>Informe generado: bytes del fichero y metadatos para servirlo como descarga.</summary>
public class ReportFile
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    /// <summary>Nombre sugerido para el guardado, con fecha y hora para que no se solapen.</summary>
    public string FileName { get; set; } = string.Empty;
    /// <summary>MIME del contenido: application/pdf o el de la hoja de cálculo de Excel.</summary>
    public string ContentType { get; set; } = string.Empty;
}

/// <summary>Selección y filtros del informe a exportar; los campos que el reporte no usa se ignoran.</summary>
public class ReportRequest
{
    public int? WarehouseId { get; set; }
    public int? CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public int? ProductId { get; set; }
    public int? UserId { get; set; }
    public MovementType? Type { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Search { get; set; }
    /// <summary>Deja sólo las líneas con stock igual o por debajo del mínimo.</summary>
    public bool CriticalOnly { get; set; }
    /// <summary>Incluye productos y almacenes desactivados, que por defecto se omiten.</summary>
    public bool IncludeInactive { get; set; }
}

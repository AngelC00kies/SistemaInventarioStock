using SistemaInventario.Core.Enums;

namespace SistemaInventario.Core.Dtos;

/// <summary>Movimiento del histórico con producto, almacén y usuario ya resueltos, para pintar la tabla sin joins.</summary>
public class MovementDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public MovementType Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    /// <summary>Unidades movidas, siempre positivas: el signo lo aporta Type.</summary>
    public int Quantity { get; set; }
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? DocumentReference { get; set; }
    /// <summary>Stock que quedó en el almacén tras este movimiento (fotografía, no valor actual).</summary>
    public int StockAfter { get; set; }
    /// <summary>Coste en las entradas y precio de venta en las salidas, registrado al momento del movimiento.</summary>
    public decimal UnitPrice { get; set; }
    // Calculado en memoria, no se persiste: el valor de la línea del informe.
    public decimal Total => Quantity * UnitPrice;
}

/// <summary>Alta de un movimiento; el backend valida cantidad, motivo, stock disponible y aplica el cambio en transacción.</summary>
public class CreateMovementRequest
{
    public MovementType Type { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    /// <summary>Debe ser mayor que cero; el signo lo define Type.</summary>
    public int Quantity { get; set; }
    /// <summary>Obligatorio: además entra en la búsqueda del histórico.</summary>
    public string Reason { get; set; } = string.Empty;
    public string? DocumentReference { get; set; }
    /// <summary>Null = se valora con el precio por defecto del tipo (PrecioCompra en entradas, PrecioVenta en salidas).</summary>
    public decimal? UnitPrice { get; set; }
    /// <summary>Null = fecha y hora actuales en UTC; permite registrar movimientos atrasados.</summary>
    public DateTime? Date { get; set; }
}

/// <summary>Filtros del histórico; todo lo opcional se combina y Page/PageSize por defecto son 1 y 15.</summary>
public class MovementFilter
{
    public int? ProductId { get; set; }
    public int? WarehouseId { get; set; }
    public int? UserId { get; set; }
    public MovementType? Type { get; set; }
    /// <summary>Fecha inicial convertida a UTC; To abarca el día completo seleccionado.</summary>
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    /// <summary>Búsqueda insensible a mayúsculas sobre nombre y código de producto, motivo y documento de referencia.</summary>
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
}

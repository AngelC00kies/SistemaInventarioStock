namespace SistemaInventario.Core.Enums;

/// <summary>Sentido del movimiento de almacén; de él depende cómo se ajusta el stock y con qué precio se valora.</summary>
public enum MovementType
{
    // Entrada: suma unidades a NivelStock.Cantidad y se valora por defecto a PrecioCompra.
    Entrada = 1,
    // Salida: descuenta unidades (validando que haya stock suficiente) y se valora por defecto a PrecioVenta.
    Salida = 2
}

/// <summary>Gravedad de un aviso de stock bajo, para ordenar y resaltar las notificaciones.</summary>
public enum NotificationLevel
{
    Info = 1,
    // Stock en el mínimo o por debajo, pero todavía por encima de la mitad del mínimo.
    Warning = 2,
    // Stock hasta la mitad del mínimo (o menos): requiere reposición inmediata.
    Critical = 3
}

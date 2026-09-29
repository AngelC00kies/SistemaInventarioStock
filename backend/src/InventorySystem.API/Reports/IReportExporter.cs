using InventorySystem.Application.Dtos;

namespace InventorySystem.API.Reports;

/// <summary>
/// Resultado de exportar un reporte: bytes + tipo MIME + nombre de archivo.
/// <see cref="FileName"/> nulo indica que el contenido se devuelve en línea
/// (p. ej. JSON), sin cabecera Content-Disposition de descarga.
/// </summary>
public sealed record ReportFile(byte[] Content, string ContentType, string? FileName = null)
{
    public bool IsDownload => FileName is not null;
}

/// <summary>
/// Estrategia de exportación. Cada formato (JSON, Excel, PDF) es una clase
/// independiente: para añadir un formato nuevo basta con implementar esta
/// interfaz y registrarlo en DI, sin modificar el controlador ni los demás
/// exportadores (Principios Abierto/Cerrado y Responsabilidad Única).
/// </summary>
public interface IReportExporter
{
    /// <summary>Formato que atiende: "json", "excel", "pdf".</summary>
    string Format { get; }

    /// <summary>Formato por defecto cuando la petición no especifica ninguno válido.</summary>
    bool IsDefault { get; }

    ReportFile ExportStock(IReadOnlyList<StockReportRow> rows, string title);

    ReportFile ExportMovements(IReadOnlyList<MovementReportRow> rows, string title);
}

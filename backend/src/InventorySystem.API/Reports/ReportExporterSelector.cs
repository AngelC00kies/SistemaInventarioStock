namespace InventorySystem.API.Reports;

/// <summary>Elige el exportador adecuado según el formato pedido en la query string.</summary>
public interface IReportExporterSelector
{
    IReportExporter Select(string? format);
}

/// <summary>
/// Resuelve el exportador registrado para un formato dado. Si el formato es
/// desconocido o falta, devuelve el exportador por defecto (JSON), de modo que
/// el controlador nunca tiene que conocer los formatos existentes.
/// </summary>
public sealed class ReportExporterSelector(IEnumerable<IReportExporter> exporters) : IReportExporterSelector
{
    private readonly IReadOnlyList<IReportExporter> _exporters = exporters.ToList();

    public IReportExporter Select(string? format) =>
        (!string.IsNullOrWhiteSpace(format)
            ? _exporters.FirstOrDefault(e => e.Format.Equals(format, StringComparison.OrdinalIgnoreCase))
            : null)
        ?? _exporters.First(e => e.IsDefault);
}

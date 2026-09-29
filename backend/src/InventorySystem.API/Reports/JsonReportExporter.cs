using System.Text;
using System.Text.Json;
using InventorySystem.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace InventorySystem.API.Reports;

/// <summary>
/// Formato por defecto: devuelve el reporte como JSON en línea, con las mismas
/// opciones de serialización (camelCase + enums como texto) que la API.
/// </summary>
public sealed class JsonReportExporter(IOptions<JsonOptions> mvcOptions) : IReportExporter
{
    private readonly JsonSerializerOptions _options = mvcOptions.Value.JsonSerializerOptions;

    public string Format => "json";
    public bool IsDefault => true;

    public ReportFile ExportStock(IReadOnlyList<StockReportRow> rows, string title)
        => Serialize(rows);

    public ReportFile ExportMovements(IReadOnlyList<MovementReportRow> rows, string title)
        => Serialize(rows);

    private ReportFile Serialize<T>(T payload) => new(
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload, _options)),
        "application/json");
}

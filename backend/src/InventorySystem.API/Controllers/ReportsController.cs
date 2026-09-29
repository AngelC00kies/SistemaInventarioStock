using InventorySystem.Application.Dtos;
using InventorySystem.Application.Interfaces;
using InventorySystem.API.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;
    private readonly IReportExporterSelector _exporters;

    public ReportsController(IReportService reports, IReportExporterSelector exporters)
    {
        _reports = reports;
        _exporters = exporters;
    }

    /// <summary>Reporte de stock actual. format: json | excel | pdf</summary>
    [HttpGet("stock")]
    public async Task<IActionResult> GetStock(
        [FromQuery] StockReportQuery query,
        [FromQuery] string format = "json",
        CancellationToken ct = default)
    {
        var rows = await _reports.GetStockReportAsync(query, ct);
        var title = query.CriticalOnly ? "Reporte de Stock Crítico" : "Reporte de Stock Actual";

        return ToActionResult(_exporters.Select(format).ExportStock(rows, title));
    }

    /// <summary>Reporte de movimientos por producto, fecha o usuario.</summary>
    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements(
        [FromQuery] MovementReportQuery query,
        [FromQuery] string format = "json",
        CancellationToken ct = default)
    {
        var rows = await _reports.GetMovementsReportAsync(query, ct);
        const string title = "Reporte de Movimientos";

        return ToActionResult(_exporters.Select(format).ExportMovements(rows, title));
    }

    /// <summary>Valorización total del inventario.</summary>
    [HttpGet("inventory-value")]
    public async Task<ActionResult<InventoryValueDto>> GetInventoryValue(CancellationToken ct)
        => Ok(await _reports.GetInventoryValueAsync(ct));

    /// <summary>
    /// Sin nombre de archivo se devuelve en línea (JSON); con nombre, como descarga.
    /// </summary>
    private IActionResult ToActionResult(ReportFile file) =>
        file.IsDownload
            ? File(file.Content, file.ContentType, file.FileName)
            : File(file.Content, file.ContentType);
}

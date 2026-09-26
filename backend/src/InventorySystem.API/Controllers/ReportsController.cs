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

    public ReportsController(IReportService reports) => _reports = reports;

    /// <summary>Reporte de stock actual. format: json | excel | pdf</summary>
    [HttpGet("stock")]
    public async Task<IActionResult> GetStock(
        [FromQuery] StockReportQuery query,
        [FromQuery] string format = "json",
        CancellationToken ct = default)
    {
        var rows = await _reports.GetStockReportAsync(query, ct);
        var title = query.CriticalOnly ? "Reporte de Stock Crítico" : "Reporte de Stock Actual";

        return format.ToLowerInvariant() switch
        {
            "excel" => File(ReportGenerator.StockToExcel(rows, title),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName(title, "xlsx")),
            "pdf" => File(ReportGenerator.StockToPdf(rows, title),
                "application/pdf", FileName(title, "pdf")),
            _ => Ok(rows)
        };
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

        return format.ToLowerInvariant() switch
        {
            "excel" => File(ReportGenerator.MovementsToExcel(rows, title),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName(title, "xlsx")),
            "pdf" => File(ReportGenerator.MovementsToPdf(rows, title),
                "application/pdf", FileName(title, "pdf")),
            _ => Ok(rows)
        };
    }

    /// <summary>Valorización total del inventario.</summary>
    [HttpGet("inventory-value")]
    public async Task<ActionResult<InventoryValueDto>> GetInventoryValue(CancellationToken ct)
        => Ok(await _reports.GetInventoryValueAsync(ct));

    private static string FileName(string title, string extension) =>
        $"{title.Replace(' ', '-').ToLower()}-{DateTime.Now:yyyyMMdd-HHmm}.{extension}";
}

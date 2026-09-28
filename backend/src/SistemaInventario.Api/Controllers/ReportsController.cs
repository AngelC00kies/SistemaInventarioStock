using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Interfaces;

namespace SistemaInventario.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports) => _reports = reports;

    [HttpGet("{report}/{format}")]
    public async Task<IActionResult> Export(
        string report,
        string format,
        [FromQuery] ReportRequest request,
        CancellationToken ct)
    {
        if (report.Equals("users", StringComparison.OrdinalIgnoreCase) && !User.IsInRole("Admin"))
            return StatusCode(403, new { message = "Solo un administrador puede exportar el listado de usuarios." });

        var file = await _reports.ExportAsync(report, format, request, ct);
        return File(file.Content, file.ContentType, file.FileName);
    }
}

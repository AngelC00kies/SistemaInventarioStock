using InventorySystem.Application.Dtos;

namespace InventorySystem.API.Reports;

/// <summary>Exporta los reportes a Excel (.xlsx) con ClosedXML.</summary>
public sealed class ExcelReportExporter : IReportExporter
{
    public string Format => "excel";
    public bool IsDefault => false;

    public ReportFile ExportStock(IReadOnlyList<StockReportRow> rows, string title) =>
        new(ReportGenerator.StockToExcel(rows, title),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ReportFileName(title, "xlsx"));

    public ReportFile ExportMovements(IReadOnlyList<MovementReportRow> rows, string title) =>
        new(ReportGenerator.MovementsToExcel(rows, title),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ReportFileName(title, "xlsx"));

    private static string ReportFileName(string title, string extension) =>
        $"{title.Replace(' ', '-').ToLower()}-{DateTime.Now:yyyyMMdd-HHmm}.{extension}";
}

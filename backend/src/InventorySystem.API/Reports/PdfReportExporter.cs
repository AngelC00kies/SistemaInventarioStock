using InventorySystem.Application.Dtos;

namespace InventorySystem.API.Reports;

/// <summary>Exporta los reportes a PDF con QuestPDF.</summary>
public sealed class PdfReportExporter : IReportExporter
{
    public string Format => "pdf";
    public bool IsDefault => false;

    public ReportFile ExportStock(IReadOnlyList<StockReportRow> rows, string title) =>
        new(ReportGenerator.StockToPdf(rows, title), "application/pdf",
            ReportFileName(title));

    public ReportFile ExportMovements(IReadOnlyList<MovementReportRow> rows, string title) =>
        new(ReportGenerator.MovementsToPdf(rows, title), "application/pdf",
            ReportFileName(title));

    private static string ReportFileName(string title) =>
        $"{title.Replace(' ', '-').ToLower()}-{DateTime.Now:yyyyMMdd-HHmm}.pdf";
}

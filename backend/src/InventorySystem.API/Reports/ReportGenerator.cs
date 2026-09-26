using ClosedXML.Excel;
using InventorySystem.Application.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InventorySystem.API.Reports;

/// <summary>Genera reportes de inventario en PDF y Excel.</summary>
public class ReportGenerator
{
    private static void StyleHeader(IXLRow row)
    {
        row.Style.Font.Bold = true;
        row.Style.Fill.BackgroundColor = XLColor.FromArgb(37, 99, 235);
        row.Style.Font.FontColor = XLColor.White;
    }

    // ─────────────────────────── EXCEL ───────────────────────────

    public static byte[] StockToExcel(IReadOnlyList<StockReportRow> rows, string title)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Stock");

        ws.Cell(1, 1).Value = title;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(2, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

        var headers = new[]
        {
            "Código", "Producto", "Categoría", "Proveedor", "Almacén",
            "Cantidad", "Stock Mín.", "U.M.", "P. Compra", "P. Venta", "Valor Stock", "Estado"
        };

        var headerRow = ws.Row(4);
        for (var i = 0; i < headers.Length; i++)
            headerRow.Cell(i + 1).Value = headers[i];
        StyleHeader(headerRow);

        var r = 5;
        foreach (var row in rows)
        {
            ws.Cell(r, 1).Value = row.ProductCode;
            ws.Cell(r, 2).Value = row.ProductName;
            ws.Cell(r, 3).Value = row.Category;
            ws.Cell(r, 4).Value = row.Supplier ?? "-";
            ws.Cell(r, 5).Value = row.Warehouse;
            ws.Cell(r, 6).Value = row.Quantity;
            ws.Cell(r, 7).Value = row.MinimumStock;
            ws.Cell(r, 8).Value = row.UnitOfMeasure.ToString();
            ws.Cell(r, 9).Value = row.PurchasePrice;
            ws.Cell(r, 10).Value = row.SalePrice;
            ws.Cell(r, 11).Value = row.StockValue;
            ws.Cell(r, 12).Value = row.Status;

            if (row.Status is "Crítico" or "Sin stock")
                ws.Row(r).Style.Fill.BackgroundColor = XLColor.FromArgb(254, 202, 202);
            else if (row.Status is "Stock bajo" or "Ajustado")
                ws.Row(r).Style.Fill.BackgroundColor = XLColor.FromArgb(254, 243, 199);

            r++;
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(4);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public static byte[] MovementsToExcel(IReadOnlyList<MovementReportRow> rows, string title)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Movimientos");

        ws.Cell(1, 1).Value = title;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(2, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

        var headers = new[]
        {
            "Fecha", "Tipo", "Código", "Producto", "Almacén",
            "Cantidad", "Stock Result.", "Motivo", "Documento", "Usuario"
        };

        var headerRow = ws.Row(4);
        for (var i = 0; i < headers.Length; i++)
            headerRow.Cell(i + 1).Value = headers[i];
        StyleHeader(headerRow);

        var r = 5;
        foreach (var row in rows)
        {
            ws.Cell(r, 1).Value = row.Date.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            ws.Cell(r, 2).Value = row.Type;
            ws.Cell(r, 3).Value = row.ProductCode;
            ws.Cell(r, 4).Value = row.ProductName;
            ws.Cell(r, 5).Value = row.Warehouse;
            ws.Cell(r, 6).Value = row.Quantity;
            ws.Cell(r, 7).Value = row.StockAfter;
            ws.Cell(r, 8).Value = row.Reason;
            ws.Cell(r, 9).Value = row.DocumentReference ?? "-";
            ws.Cell(r, 10).Value = row.UserName;

            if (row.Type == "Entrada")
                ws.Row(r).Style.Font.FontColor = XLColor.FromArgb(22, 163, 74);
            else
                ws.Row(r).Style.Font.FontColor = XLColor.FromArgb(220, 38, 38);

            r++;
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(4);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // ──────────────────────────── PDF ────────────────────────────

    public static byte[] StockToPdf(IReadOnlyList<StockReportRow> rows, string title)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(t => t.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text(title).FontSize(16).Bold();
                    col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}  |  Registros: {rows.Count}")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    string[] headers = { "Código", "Producto", "Almacén", "Cant.", "Mín.", "Valor", "Estado" };
                    float[] widths = { 12, 26, 18, 9, 8, 14, 13 };

                    table.ColumnsDefinition(cols =>
                    {
                        foreach (var w in widths)
                            cols.RelativeColumn(w);
                    });

                    table.Header(header =>
                    {
                        foreach (var h in headers)
                        {
                            header.Cell().Background(Colors.Blue.Darken2)
                                .Padding(4).Text(h).FontColor(Colors.White).Bold().FontSize(8);
                        }
                    });

                    foreach (var row in rows)
                    {
                        var background = row.Status is "Crítico" or "Sin stock"
                            ? Colors.Red.Medium
                            : row.Status is "Stock bajo" or "Ajustado"
                                ? Colors.Amber.Medium
                                : Colors.White;

                        void Cell(string text)
                        {
                            table.Cell().Background(background).Padding(4).Text(text).FontSize(8);
                        }

                        Cell(row.ProductCode);
                        Cell(row.ProductName);
                        Cell(row.Warehouse);
                        Cell(row.Quantity.ToString());
                        Cell(row.MinimumStock.ToString());
                        Cell(row.StockValue.ToString("N2"));
                        Cell(row.Status);
                    }
                });

                page.Footer().AlignCenter()
                    .Text(t =>
                    {
                        t.Span("Página ").FontSize(8);
                        t.CurrentPageNumber().FontSize(8);
                        t.Span(" de ").FontSize(8);
                        t.TotalPages().FontSize(8);
                    });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    public static byte[] MovementsToPdf(IReadOnlyList<MovementReportRow> rows, string title)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(28);
                page.DefaultTextStyle(t => t.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text(title).FontSize(16).Bold();
                    col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}  |  Registros: {rows.Count}")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    string[] headers = { "Fecha", "Tipo", "Producto", "Almacén", "Cant.", "Stock Res.", "Motivo", "Documento", "Usuario" };
                    float[] widths = { 13, 8, 20, 14, 7, 9, 17, 10, 12 };

                    table.ColumnsDefinition(cols =>
                    {
                        foreach (var w in widths)
                            cols.RelativeColumn(w);
                    });

                    table.Header(header =>
                    {
                        foreach (var h in headers)
                        {
                            header.Cell().Background(Colors.Blue.Darken2)
                                .Padding(4).Text(h).FontColor(Colors.White).Bold().FontSize(8);
                        }
                    });

                    foreach (var row in rows)
                    {
                        void Cell(string text, string? color = null)
                        {
                            var c = table.Cell().Padding(4);
                            var t = c.Text(text).FontSize(8);
                            if (color is not null)
                                t.FontColor(color);
                        }

                        Cell(row.Date.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
                        Cell(row.Type, row.Type == "Entrada" ? Colors.Green.Darken2 : Colors.Red.Darken2);
                        Cell($"{row.ProductCode} - {row.ProductName}");
                        Cell(row.Warehouse);
                        Cell(row.Quantity.ToString());
                        Cell(row.StockAfter.ToString());
                        Cell(row.Reason);
                        Cell(row.DocumentReference ?? "-");
                        Cell(row.UserName);
                    }
                });

                page.Footer().AlignCenter()
                    .Text(t =>
                    {
                        t.Span("Página ").FontSize(8);
                        t.CurrentPageNumber().FontSize(8);
                        t.Span(" de ").FontSize(8);
                        t.TotalPages().FontSize(8);
                    });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }
}

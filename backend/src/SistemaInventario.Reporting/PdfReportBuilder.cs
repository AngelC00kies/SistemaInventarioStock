using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SistemaInventario.Reporting;

public static class PdfReportBuilder
{
    private const string Primary = "#1E3A5F";
    private const string Accent = "#2563EB";
    private const string Muted = "#64748B";
    private const string Band = "#F8FAFC";
    private const string Border = "#E2E8F0";
    private const string Font = "Arial";

    public static byte[] Build(ReportTable table)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(28);
                page.DefaultTextStyle(t => t.FontFamily(Font).FontSize(8.5f).FontColor("#0F172A"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("SISTEMA DE INVENTARIO")
                                .FontSize(9).Bold().FontColor(Accent).LetterSpacing(0.6f);
                            c.Item().PaddingTop(2).Text(table.Title)
                                .FontSize(17).Bold().FontColor(Primary);
                            if (!string.IsNullOrWhiteSpace(table.Subtitle))
                                c.Item().PaddingTop(3).Text(table.Subtitle)
                                    .FontSize(8.5f).FontColor(Muted);
                        });

                        row.ConstantItem(210).AlignRight().Column(c =>
                        {
                            c.Item().AlignRight().Text($"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(8.5f).FontColor(Muted);
                            c.Item().AlignRight().Text($"Registros: {table.Rows.Count}")
                                .FontSize(8.5f).FontColor(Muted);
                            c.Item().AlignRight().Text("Documento interno")
                                .FontSize(8.5f).FontColor(Muted);
                        });
                    });

                    col.Item().PaddingTop(8).LineHorizontal(1.2f).LineColor(Primary);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    if (table.Summary.Count > 0)
                    {
                        col.Item().Row(row =>
                        {
                            foreach (var (label, value) in table.Summary)
                            {
                                row.RelativeItem().PaddingRight(10).Border(0.7f).BorderColor(Border)
                                    .Padding(6).Column(c =>
                                    {
                                        c.Item().Text(label.ToUpper()).FontSize(7).FontColor(Muted)
                                            .LetterSpacing(0.4f);
                                        c.Item().PaddingTop(1).Text(value).FontSize(11).Bold().FontColor(Primary);
                                    });
                            }
                        });
                        col.Item().PaddingTop(10);
                    }

                    col.Item().Table(t =>
                    {
                        var totalWeight = table.Widths.Count == table.Columns.Count
                            ? table.Widths.Sum(w => w.Weight)
                            : table.Columns.Count;

                        t.ColumnsDefinition(columns =>
                        {
                            for (var i = 0; i < table.Columns.Count; i++)
                            {
                                var weight = table.Widths.Count == table.Columns.Count
                                    ? table.Widths[i].Weight
                                    : 1;
                                columns.RelativeColumn((float)(weight / totalWeight * table.Columns.Count));
                            }
                        });

                        t.Header(h =>
                        {
                            for (var i = 0; i < table.Columns.Count; i++)
                            {
                                h.Cell().Background(Primary).Padding(5)
                                    .Text(table.Columns[i]).FontSize(8).Bold().FontColor("#FFFFFF");
                            }
                        });

                        for (var r = 0; r < table.Rows.Count; r++)
                        {
                            var background = r % 2 == 1 ? Band : "#FFFFFF";

                            for (var c = 0; c < table.Rows[r].Count; c++)
                            {
                                var value = table.Rows[r][c];
                                t.Cell()
                                    .Background(background)
                                    .BorderBottom(0.5f).BorderColor(Border)
                                    .PaddingVertical(4).PaddingHorizontal(5)
                                    .Text(value ?? string.Empty).FontSize(8);
                            }
                        }
                    });
                });

                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Sistema de Inventario — ").FontColor(Muted);
                        t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontColor(Muted);
                    });

                    row.ConstantItem(160).AlignRight().Text(t =>
                    {
                        t.Span("Página ").FontColor(Muted);
                        t.CurrentPageNumber().FontColor(Muted);
                        t.Span(" de ").FontColor(Muted);
                        t.TotalPages().FontColor(Muted);
                    });
                });
            });
        }).GeneratePdf();
    }
}

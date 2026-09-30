using ClosedXML.Excel;

namespace SistemaInventario.Reporting;

/// <summary>Compone el XLSX del reporte con ClosedXML: bloque de título, tabla con autofiltro y resumen.</summary>
public static class ExcelReportBuilder
{
    private static readonly XLColor Primary = XLColor.FromHtml("#1E3A5F");
    private static readonly XLColor Accent = XLColor.FromHtml("#2563EB");
    private static readonly XLColor Band = XLColor.FromHtml("#F1F5F9");

    public static byte[] Build(ReportTable table)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Reporte");

        // Título
        sheet.Cell(1, 1).Value = "Sistema de Inventario";
        sheet.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(16).Font.SetFontColor(Primary);
        sheet.Range(1, 1, 1, Math.Max(1, table.Columns.Count)).Merge();

        sheet.Cell(2, 1).Value = table.Title;
        sheet.Cell(2, 1).Style.Font.SetBold().Font.SetFontSize(12).Font.SetFontColor(Accent);
        sheet.Range(2, 1, 2, Math.Max(1, table.Columns.Count)).Merge();

        var meta = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        if (!string.IsNullOrWhiteSpace(table.Subtitle)) meta += $"   |   {table.Subtitle}";
        sheet.Cell(3, 1).Value = meta;
        sheet.Cell(3, 1).Style.Font.SetFontSize(9).Font.SetFontColor(XLColor.FromHtml("#64748B"));
        sheet.Range(3, 1, 3, Math.Max(1, table.Columns.Count)).Merge();

        // Las filas 1 a 3 llevan el bloque de título (fusionado al ancho de la tabla); la 4 queda libre
        // y la cabecera de datos se escribe en la 5, punto desde el que se congelan y filtran las filas
        var headerRow = 5;

        // Cabecera
        for (var i = 0; i < table.Columns.Count; i++)
        {
            var cell = sheet.Cell(headerRow, i + 1);
            cell.Value = table.Columns[i];
            cell.Style.Font.SetBold().Font.SetFontColor(XLColor.White).Font.SetFontSize(10);
            cell.Style.Fill.SetBackgroundColor(Primary);
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.White;
        }
        sheet.Row(headerRow).Height = 22;

        // Datos
        for (var r = 0; r < table.Rows.Count; r++)
        {
            for (var c = 0; c < table.Rows[r].Count; c++)
            {
                var cell = sheet.Cell(headerRow + 1 + r, c + 1);
                cell.Value = table.Rows[r][c];
                cell.Style.Font.SetFontSize(10);
                cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.BottomBorderColor = XLColor.FromHtml("#E2E8F0");

                if (r % 2 == 1) cell.Style.Fill.SetBackgroundColor(Band);

                // Detección heurística de cifras: se quitan separadores y se prueba la conversión;
                // si resulta numérica, la celda se alinea a la derecha como en una hoja de cálculo
                var isNumeric = decimal.TryParse(table.Rows[r][c].Replace(",", "").Replace(".", ""),
                    out _);
                if (isNumeric) cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            }
        }

        var lastRow = headerRow + table.Rows.Count;

        // Resumen
        if (table.Summary.Count > 0)
        {
            lastRow += 2;
            sheet.Cell(lastRow, 1).Value = "Resumen";
            sheet.Cell(lastRow, 1).Style.Font.SetBold().Font.SetFontSize(11).Font.SetFontColor(Primary);
            sheet.Range(lastRow, 1, lastRow, Math.Max(1, table.Columns.Count)).Merge();

            for (var i = 0; i < table.Summary.Count; i++)
            {
                var row = lastRow + 1 + i;
                sheet.Cell(row, 1).Value = table.Summary[i].Label;
                sheet.Cell(row, 1).Style.Font.SetBold();
                sheet.Cell(row, 2).Value = table.Summary[i].Value;
            }
        }

        // Anchos de columna
        for (var c = 1; c <= table.Columns.Count; c++)
        {
            var column = sheet.Column(c);
            column.Width = Math.Clamp(column.Width, 10, 45);
            column.Style.Alignment.SetWrapText(false);
        }

        // Congela la cabecera, ajusta a una página de ancho y activa el autofiltro sobre los datos
        sheet.SheetView.FreezeRows(headerRow);
        sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        sheet.PageSetup.FitToPages(1, 0);
        sheet.Range(headerRow, 1, Math.Max(headerRow, headerRow + table.Rows.Count), table.Columns.Count)
            .SetAutoFilter();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}

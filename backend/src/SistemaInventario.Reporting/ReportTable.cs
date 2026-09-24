namespace SistemaInventario.Reporting;

public class ReportTable
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string FileBaseName { get; set; } = "reporte";
    public List<string> Columns { get; set; } = new();
    public List<ReportColumnWidth> Widths { get; set; } = new();
    public List<List<string>> Rows { get; set; } = new();
    public List<(string Label, string Value)> Summary { get; set; } = new();

    public ReportTable WithColumns(params string[] columns)
    {
        Columns = columns.ToList();
        return this;
    }

    public ReportTable WithWidths(params double[] widths)
    {
        Widths = widths.Select(w => new ReportColumnWidth(w)).ToList();
        return this;
    }
}

public readonly record struct ReportColumnWidth(double Weight);

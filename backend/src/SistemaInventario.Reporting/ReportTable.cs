namespace SistemaInventario.Reporting;

/// <summary>Tabla intermedia independiente del formato: encabezados, filas ya formateadas y bloque de totales.</summary>
public class ReportTable
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string FileBaseName { get; set; } = "reporte";
    public List<string> Columns { get; set; } = new();
    // Pesos relativos de ancho por columna; si no coinciden con Columns, cada columna vale 1
    public List<ReportColumnWidth> Widths { get; set; } = new();
    // Cada fila es una lista de textos alineada 1:1 con Columns (cifras ya llevan su formato)
    public List<List<string>> Rows { get; set; } = new();
    // Pares etiqueta/valor que se dibujan como tarjetas de totales antes de la tabla
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

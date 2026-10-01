using SistemaInventario.Reporting;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Tests.Infrastructure;
using Xunit;

namespace SistemaInventario.Tests.Services;

/// <summary>Pruebas de <see cref="ReportService"/>: resolución del reporte y del formato, y fichero resultante.</summary>
public class ReportServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();
    private readonly ReportService _service;

    public ReportServiceTests() => _service = new ReportService(_db.Db);

    public void Dispose() => _db.Dispose();

    [Theory]
    [InlineData("stock")]
    [InlineData("critical-stock")]
    [InlineData("movements")]
    [InlineData("products")]
    [InlineData("categories")]
    [InlineData("suppliers")]
    [InlineData("users")]
    public async Task ExportAsync_TodosLosReportes_SeanEnPdfYEnExcel(string report)
    {
        var pdf = await _service.ExportAsync(report, "pdf", new ReportRequest());
        var excel = await _service.ExportAsync(report, "xlsx", new ReportRequest());

        Assert.NotEmpty(pdf.Content);
        Assert.NotEmpty(excel.Content);

        // Cabecera mágica del PDF y del ZIP que envuelve las hojas de cálculo.
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf.Content, 0, 4));
        Assert.Equal("PK", System.Text.Encoding.ASCII.GetString(excel.Content, 0, 2));

        Assert.EndsWith(".pdf", pdf.FileName);
        Assert.EndsWith(".xlsx", excel.FileName);
        Assert.Equal("application/pdf", pdf.ContentType);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excel.ContentType);
    }

    [Fact]
    public async Task ExportAsync_ElNombreDelReporteNoDistingueMayusculas()
    {
        var mayusculas = await _service.ExportAsync("STOCK", "pdf", new ReportRequest());
        var minusculas = await _service.ExportAsync("stock", "pdf", new ReportRequest());

        Assert.Equal(mayusculas.ContentType, minusculas.ContentType);
        Assert.NotEmpty(mayusculas.Content);
    }

    [Fact]
    public async Task ExportAsync_UsaElNombreDeFalloDeStockParaElCritico()
    {
        // "critical-stock" comparte la tabla con "stock" pero con el filtro de faltantes activado.
        var normal = await _service.ExportAsync("stock", "xlsx", new ReportRequest());
        var critico = await _service.ExportAsync("critical-stock", "xlsx", new ReportRequest());

        Assert.NotEmpty(normal.Content);
        Assert.NotEmpty(critico.Content);
        Assert.NotEqual(normal.FileName, critico.FileName);
    }

    [Fact]
    public async Task ExportAsync_ReporteInexistente_LanzaAppException()
    {
        var excepcion = await Assert.ThrowsAsync<AppException>(
            () => _service.ExportAsync("inventado", "pdf", new ReportRequest()));

        Assert.Equal("El reporte \"inventado\" no existe.", excepcion.Message);
        Assert.Equal(400, excepcion.StatusCode);
    }

    [Fact]
    public async Task ExportAsync_FormatoNoSoportado_LanzaAppException()
    {
        var excepcion = await Assert.ThrowsAsync<AppException>(
            () => _service.ExportAsync("stock", "csv", new ReportRequest()));

        Assert.Equal("Formato no soportado. Use pdf o xlsx.", excepcion.Message);
    }

    [Fact]
    public async Task ExportAsync_ElFormatoExcelTambienAceptaElAliasExcel()
    {
        var porExtension = await _service.ExportAsync("stock", "xlsx", new ReportRequest());
        var porNombre = await _service.ExportAsync("stock", "excel", new ReportRequest());

        Assert.NotEmpty(porNombre.Content);
        Assert.Equal(".xlsx", porNombre.FileName[^5..]);
        Assert.Equal(porExtension.ContentType, porNombre.ContentType);
    }

    [Fact]
    public async Task ExportAsync_ConDatosDelInventario_GeneraElFicheroSinFallo()
    {
        var categoria = await Seed.CategoriaAsync(_db.Db);
        var almacen = await Seed.AlmacenAsync(_db.Db);
        var proveedor = await Seed.ProveedorAsync(_db.Db);
        var producto = await Seed.ProductoAsync(_db.Db, categoria.Id, proveedorId: proveedor.Id, stockMinimo: 10);
        await Seed.StockAsync(_db.Db, producto.Id, almacen.Id, cantidad: 3);

        var rol = await Seed.RolAsync(_db.Db);
        var usuario = await Seed.UsuarioAsync(_db.Db, rol.Id, "operador");
        await Seed.MovimientoAsync(_db.Db, producto.Id, almacen.Id, usuario.Id);

        foreach (var report in new[] { "stock", "critical-stock", "movements", "products", "categories", "suppliers", "users" })
        {
            var fichero = await _service.ExportAsync(report, "xlsx", new ReportRequest());
            Assert.NotEmpty(fichero.Content);
        }
    }

    [Fact]
    public async Task ExportAsync_LaFechaDelNombreLlevaLaMarcaDeTiempo()
    {
        var fichero = await _service.ExportAsync("products", "pdf", new ReportRequest());

        // El nombre interno va en español: productos_yyyyMMdd_HHmm.pdf
        var cuerpo = Path.GetFileNameWithoutExtension(fichero.FileName);
        Assert.Matches(@"productos_\d{8}_\d{4}$", cuerpo);
    }
}

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Enums;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Core.Interfaces;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Reporting;

/// <summary>Arma la tabla de cada reporte (consulta, filtros, columnas y totales) y la entrega al generador.</summary>
public class ReportService : IReportService
{
    // Formato chileno: miles ".", decimales "," y símbolo de moneda "$" en los totales
    private static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-CL");
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db) => _db = db;

    /// <summary>Resuelve el reporte y el formato pedidos y devuelve el fichero listo para descargar.</summary>
    public async Task<ReportFile> ExportAsync(string report, string format, ReportRequest request, CancellationToken ct = default)
    {
        // Cada BuildXXX compone Columns (encabezados), Widths (pesos de ancho),
        // Rows (celdas ya formateadas) y Summary (totales) de un mismo ReportTable
        var table = report.ToLowerInvariant() switch
        {
            "stock" => await BuildStockAsync(request, criticalOnly: false, ct),
            "critical-stock" => await BuildStockAsync(request, criticalOnly: true, ct),
            "movements" => await BuildMovementsAsync(request, ct),
            "products" => await BuildProductsAsync(request, ct),
            "categories" => await BuildCategoriesAsync(request, ct),
            "suppliers" => await BuildSuppliersAsync(request, ct),
            "users" => await BuildUsersAsync(request, ct),
            _ => throw new AppException($"El reporte \"{report}\" no existe.")
        };

        var isPdf = format.ToLowerInvariant() switch
        {
            "pdf" => true,
            "xlsx" or "excel" => false,
            _ => throw new AppException("Formato no soportado. Use pdf o xlsx.")
        };

        var content = isPdf ? PdfReportBuilder.Build(table) : ExcelReportBuilder.Build(table);

        // El nombre lleva la marca de tiempo para que dos exportaciones seguidas no se sobrescriban
        return new ReportFile
        {
            Content = content,
            FileName = $"{table.FileBaseName}_{DateTime.Now:yyyyMMdd_HHmm}.{(isPdf ? "pdf" : "xlsx")}",
            ContentType = isPdf
                ? "application/pdf"
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
    }

    private async Task<ReportTable> BuildStockAsync(ReportRequest f, bool criticalOnly, CancellationToken ct)
    {
        // Una fila = un nivel de stock (producto × almacén); por eso el mismo producto puede repetirse
        var query = _db.NivelesStock.AsNoTracking()
            .Include(s => s.Producto).ThenInclude(p => p.Categoria)
            .Include(s => s.Producto).ThenInclude(p => p.Proveedor)
            .Include(s => s.Almacen)
            .AsQueryable();

        // Filtros opcionales; por defecto solo activos y el modo crítico recorta a cantidad <= stock mínimo
        if (!f.IncludeInactive) query = query.Where(s => s.Producto.Activo && s.Almacen.Activo);
        if (f.WarehouseId.HasValue) query = query.Where(s => s.AlmacenId == f.WarehouseId.Value);
        if (f.CategoryId.HasValue) query = query.Where(s => s.Producto.CategoriaId == f.CategoryId.Value);
        if (f.SupplierId.HasValue) query = query.Where(s => s.Producto.ProveedorId == f.SupplierId.Value);
        if (criticalOnly || f.CriticalOnly) query = query.Where(s => s.Cantidad <= s.Producto.StockMinimo);

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var term = f.Search.Trim().ToLower();
            query = query.Where(s =>
                s.Producto.Nombre.ToLower().Contains(term) ||
                s.Producto.Codigo.ToLower().Contains(term));
        }

        var data = await query
            .OrderBy(s => s.Producto.Codigo)
            .ThenBy(s => s.Almacen.Codigo)
            .Select(s => new
            {
                s.Producto.Codigo,
                s.Producto.Nombre,
                Category = s.Producto.Categoria.Nombre,
                Supplier = s.Producto.Proveedor != null ? s.Producto.Proveedor.Nombre : "—",
                s.Producto.Unidad,
                Warehouse = s.Almacen.Nombre,
                s.Cantidad,
                s.Producto.StockMinimo,
                s.Producto.PrecioVenta
            })
            .ToListAsync(ct);

        // Estructura de columnas: identificación (código, producto, clasificación), ubicación,
        // cifras de stock (cantidad, mínimo, precio) y de cierre (valorización y estado)
        var table = new ReportTable
        {
            Title = criticalOnly || f.CriticalOnly ? "Reporte de stock crítico" : "Reporte de stock actual",
            Subtitle = Describe(f),
            FileBaseName = criticalOnly || f.CriticalOnly ? "stock_critico" : "stock_actual",
            Columns = new List<string>
            {
                "Código", "Producto", "Categoría", "Proveedor", "Almacén", "Unidad",
                "Cantidad", "Mínimo", "P. venta", "Valor stock", "Estado"
            }
        }.WithWidths(0.8, 2.2, 1.3, 1.6, 1.3, 0.8, 0.8, 0.7, 1, 1.1, 0.9);

        decimal totalValue = 0;
        var totalUnits = 0;

        // Valorización del stock: cantidad × precio de venta, acumulada por cada fila producto-almacén
        foreach (var row in data)
        {
            var value = row.Cantidad * row.PrecioVenta;
            totalValue += value;
            totalUnits += row.Cantidad;

            table.Rows.Add(new List<string>
            {
                row.Codigo,
                row.Nombre,
                row.Category,
                row.Supplier,
                row.Warehouse,
                row.Unidad,
                row.Cantidad.ToString("N0", Es),
                row.StockMinimo.ToString("N0", Es),
                row.PrecioVenta.ToString("N2", Es),
                value.ToString("N0", Es),
                StatusText(row.Cantidad, row.StockMinimo)
            });
        }

        // Un mismo SKU puede estar en varios almacenes: se cuentan códigos distintos, no filas
        table.Summary.Add(("SKU con stock", data.Select(d => d.Codigo).Distinct().Count().ToString("N0", Es)));
        table.Summary.Add(("Unidades totales", totalUnits.ToString("N0", Es)));
        table.Summary.Add(("Valor de stock", "$" + totalValue.ToString("N0", Es)));

        return table;
    }

    private async Task<ReportTable> BuildMovementsAsync(ReportRequest f, CancellationToken ct)
    {
        var query = _db.Movimientos.AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.Almacen)
            .Include(m => m.Usuario)
            .AsQueryable();

        if (f.ProductId.HasValue) query = query.Where(m => m.ProductoId == f.ProductId.Value);
        if (f.WarehouseId.HasValue) query = query.Where(m => m.AlmacenId == f.WarehouseId.Value);
        if (f.UserId.HasValue) query = query.Where(m => m.UsuarioId == f.UserId.Value);
        if (f.Type.HasValue) query = query.Where(m => m.Tipo == f.Type.Value);
        // Filtro de fechas: "hasta" es el día completo, por eso se amplía hasta el último tick de esa jornada
        if (f.From.HasValue) query = query.Where(m => m.Fecha >= f.From.Value.ToUniversalTime());
        if (f.To.HasValue) query = query.Where(m => m.Fecha <= f.To.Value.ToUniversalTime().AddDays(1).AddTicks(-1));

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var term = f.Search.Trim().ToLower();
            query = query.Where(m =>
                m.Producto.Nombre.ToLower().Contains(term) ||
                m.Producto.Codigo.ToLower().Contains(term) ||
                m.Motivo.ToLower().Contains(term) ||
                (m.DocumentoReferencia != null && m.DocumentoReferencia.ToLower().Contains(term)));
        }

        var data = await query.OrderByDescending(m => m.Fecha)
            .Select(m => new
            {
                m.Fecha,
                Type = m.Tipo.ToString(),
                m.Producto.Codigo,
                Product = m.Producto.Nombre,
                Warehouse = m.Almacen.Nombre,
                User = m.Usuario.NombreCompleto,
                m.Cantidad,
                m.PrecioUnitario,
                m.StockResultante,
                m.Motivo,
                m.DocumentoReferencia
            })
            .ToListAsync(ct);

        var table = new ReportTable
        {
            Title = "Reporte de movimientos de inventario",
            Subtitle = Describe(f),
            FileBaseName = "movimientos",
            Columns = new List<string>
            {
                "Fecha", "Tipo", "Código", "Producto", "Almacén", "Usuario",
                "Cantidad", "P. unitario", "Total", "Stock resultante", "Motivo", "Documento"
            }
        }.WithWidths(1.2, 0.7, 0.8, 2, 1.2, 1.5, 0.7, 0.9, 1, 0.9, 2, 0.9);

        var entries = 0;
        var exits = 0;
        decimal entriesValue = 0;
        decimal exitsValue = 0;

        // Total de la fila = cantidad × precio unitario del movimiento (entradas a precio de compra,
        // salidas a precio de venta); el resumen separa unidades y valor de cada sentido
        foreach (var row in data)
        {
            var total = row.Cantidad * row.PrecioUnitario;
            if (row.Type == nameof(MovementType.Entrada))
            {
                entries += row.Cantidad;
                entriesValue += total;
            }
            else
            {
                exits += row.Cantidad;
                exitsValue += total;
            }

            table.Rows.Add(new List<string>
            {
                row.Fecha.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                row.Type,
                row.Codigo,
                row.Product,
                row.Warehouse,
                row.User,
                row.Cantidad.ToString("N0", Es),
                row.PrecioUnitario.ToString("N2", Es),
                total.ToString("N0", Es),
                row.StockResultante.ToString("N0", Es),
                row.Motivo,
                row.DocumentoReferencia ?? "—"
            });
        }

        table.Summary.Add(("Movimientos", data.Count.ToString("N0", Es)));
        table.Summary.Add(("Entradas (unidades)", entries.ToString("N0", Es)));
        table.Summary.Add(("Valor entradas", "$" + entriesValue.ToString("N0", Es)));
        table.Summary.Add(("Salidas (unidades)", exits.ToString("N0", Es)));
        table.Summary.Add(("Valor salidas", "$" + exitsValue.ToString("N0", Es)));

        return table;
    }

    private async Task<ReportTable> BuildProductsAsync(ReportRequest f, CancellationToken ct)
    {
        var query = _db.Productos.AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
            .Include(p => p.NivelesStock)
            .AsQueryable();

        if (!f.IncludeInactive) query = query.Where(p => p.Activo);
        if (f.CategoryId.HasValue) query = query.Where(p => p.CategoriaId == f.CategoryId.Value);
        if (f.SupplierId.HasValue) query = query.Where(p => p.ProveedorId == f.SupplierId.Value);

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var term = f.Search.Trim().ToLower();
            query = query.Where(p =>
                p.Nombre.ToLower().Contains(term) || p.Codigo.ToLower().Contains(term));
        }

        var data = await query.OrderBy(p => p.Codigo).ToListAsync(ct);

        var table = new ReportTable
        {
            Title = "Catálogo de productos",
            Subtitle = Describe(f),
            FileBaseName = "productos",
            Columns = new List<string>
            {
                "Código", "Producto", "Categoría", "Proveedor", "Unidad",
                "Stock", "Mínimo", "P. compra", "P. venta", "Estado"
            }
        }.WithWidths(0.8, 2.2, 1.3, 1.6, 0.8, 0.7, 0.7, 1, 1, 0.9);

        // El stock mostrado es la suma de los niveles de todos los almacenes, no el de uno solo
        foreach (var p in data)
        {
            var stock = p.NivelesStock.Sum(s => s.Cantidad);
            table.Rows.Add(new List<string>
            {
                p.Codigo,
                p.Nombre,
                p.Categoria.Nombre,
                p.Proveedor?.Nombre ?? "—",
                p.Unidad,
                stock.ToString("N0", Es),
                p.StockMinimo.ToString("N0", Es),
                p.PrecioCompra.ToString("N2", Es),
                p.PrecioVenta.ToString("N2", Es),
                // El servicio devuelve una clave ("ok"/"low"/"critical"/"empty") que aquí se traduce a etiqueta;
                // un producto inactivo se muestra como tal sin evaluar su stock
                p.Activo ? Mapping.ProductStatus(stock, p.StockMinimo) switch
                {
                    "ok" => "Activo / OK",
                    "low" => "Stock bajo",
                    "critical" => "Crítico",
                    _ => "Sin stock"
                } : "Inactivo"
            });
        }

        table.Summary.Add(("Productos", data.Count.ToString("N0", Es)));
        table.Summary.Add(("Activos", data.Count(p => p.Activo).ToString("N0", Es)));
        table.Summary.Add(("Con stock bajo", data.Count(p => p.NivelesStock.Sum(s => s.Cantidad) <= p.StockMinimo).ToString("N0", Es)));

        return table;
    }

    private async Task<ReportTable> BuildCategoriesAsync(ReportRequest f, CancellationToken ct)
    {
        var query = _db.Categorias.AsNoTracking().Include(c => c.Productos).AsQueryable();
        if (!f.IncludeInactive) query = query.Where(c => c.Activo);
        var data = await query.OrderBy(c => c.Nombre).ToListAsync(ct);

        var table = new ReportTable
        {
            Title = "Reporte de categorías",
            Subtitle = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            FileBaseName = "categorias",
            Columns = new List<string> { "Categoría", "Descripción", "Productos activos", "Productos totales", "Estado" }
        }.WithWidths(1.5, 3, 1.2, 1.2, 1);

        foreach (var c in data)
        {
            table.Rows.Add(new List<string>
            {
                c.Nombre,
                c.Descripcion ?? "—",
                c.Productos.Count(p => p.Activo).ToString("N0", Es),
                c.Productos.Count.ToString("N0", Es),
                c.Activo ? "Activa" : "Inactiva"
            });
        }

        table.Summary.Add(("Categorías", data.Count.ToString("N0", Es)));
        table.Summary.Add(("Activas", data.Count(c => c.Activo).ToString("N0", Es)));

        return table;
    }

    private async Task<ReportTable> BuildSuppliersAsync(ReportRequest f, CancellationToken ct)
    {
        var query = _db.Proveedores.AsNoTracking().Include(s => s.Productos).AsQueryable();
        if (!f.IncludeInactive) query = query.Where(s => s.Activo);

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var term = f.Search.Trim().ToLower();
            query = query.Where(s => s.Nombre.ToLower().Contains(term));
        }

        var data = await query.OrderBy(s => s.Nombre).ToListAsync(ct);

        var table = new ReportTable
        {
            Title = "Reporte de proveedores",
            Subtitle = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            FileBaseName = "proveedores",
            Columns = new List<string> { "Proveedor", "Contacto", "Teléfono", "Correo", "Dirección", "Productos", "Estado" }
        }.WithWidths(2, 1.4, 1.2, 1.8, 2.4, 0.9, 0.9);

        foreach (var s in data)
        {
            table.Rows.Add(new List<string>
            {
                s.Nombre,
                s.NombreContacto ?? "—",
                s.Telefono ?? "—",
                s.Correo ?? "—",
                s.Direccion ?? "—",
                s.Productos.Count(p => p.Activo).ToString("N0", Es),
                s.Activo ? "Activo" : "Inactivo"
            });
        }

        table.Summary.Add(("Proveedores", data.Count.ToString("N0", Es)));
        table.Summary.Add(("Activos", data.Count(s => s.Activo).ToString("N0", Es)));

        return table;
    }

    private async Task<ReportTable> BuildUsersAsync(ReportRequest f, CancellationToken ct)
    {
        var query = _db.Usuarios.AsNoTracking().Include(u => u.Rol).AsQueryable();
        if (!f.IncludeInactive) query = query.Where(u => u.Activo);

        var data = await query.OrderBy(u => u.NombreUsuario).ToListAsync(ct);

        var table = new ReportTable
        {
            Title = "Reporte de usuarios",
            Subtitle = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            FileBaseName = "usuarios",
            Columns = new List<string> { "Usuario", "Nombre completo", "Correo", "Rol", "Estado", "Creado" }
        }.WithWidths(1.2, 2, 2, 1, 0.9, 1.2);

        foreach (var u in data)
        {
            table.Rows.Add(new List<string>
            {
                u.NombreUsuario,
                u.NombreCompleto,
                u.Correo ?? "—",
                u.Rol.Nombre,
                u.Activo ? "Activo" : "Inactivo",
                u.FechaCreacion.ToLocalTime().ToString("dd/MM/yyyy")
            });
        }

        table.Summary.Add(("Usuarios", data.Count.ToString("N0", Es)));
        table.Summary.Add(("Activos", data.Count(u => u.Activo).ToString("N0", Es)));

        return table;
    }

    // Escala del estado: sin stock (0), crítico hasta la mitad del mínimo (mínimo 1) y bajo hasta el mínimo
    private static string StatusText(int quantity, int minStock) => quantity switch
    {
        <= 0 => "Sin stock",
        _ when quantity <= Math.Max(1, minStock / 2) => "Crítico",
        _ when quantity <= minStock => "Stock bajo",
        _ => "OK"
    };

    // Resume en el subtítulo los filtros aplicados para que el documento se entienda sin ver la petición
    private static string Describe(ReportRequest f)
    {
        var parts = new List<string>();

        if (f.WarehouseId.HasValue) parts.Add("Almacén filtrado");
        if (f.CategoryId.HasValue) parts.Add("Categoría filtrada");
        if (f.CriticalOnly) parts.Add("Solo críticos");
        if (f.From.HasValue) parts.Add($"Desde {f.From.Value:dd/MM/yyyy}");
        if (f.To.HasValue) parts.Add($"Hasta {f.To.Value:dd/MM/yyyy}");
        if (f.Type.HasValue) parts.Add($"Tipo: {f.Type.Value}");
        if (!string.IsNullOrWhiteSpace(f.Search)) parts.Add($"Búsqueda: \"{f.Search}\"");

        return parts.Count == 0 ? "Sin filtros aplicados" : string.Join(" · ", parts);
    }
}

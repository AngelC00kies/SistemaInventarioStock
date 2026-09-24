using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Enums;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Core.Interfaces;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Reporting;

public class ReportService : IReportService
{
    private static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-CL");
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db) => _db = db;

    public async Task<ReportFile> ExportAsync(string report, string format, ReportRequest request, CancellationToken ct = default)
    {
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
        var query = _db.StockLevels.AsNoTracking()
            .Include(s => s.Product).ThenInclude(p => p.Category)
            .Include(s => s.Product).ThenInclude(p => p.Supplier)
            .Include(s => s.Warehouse)
            .AsQueryable();

        if (!f.IncludeInactive) query = query.Where(s => s.Product.IsActive && s.Warehouse.IsActive);
        if (f.WarehouseId.HasValue) query = query.Where(s => s.WarehouseId == f.WarehouseId.Value);
        if (f.CategoryId.HasValue) query = query.Where(s => s.Product.CategoryId == f.CategoryId.Value);
        if (f.SupplierId.HasValue) query = query.Where(s => s.Product.SupplierId == f.SupplierId.Value);
        if (criticalOnly || f.CriticalOnly) query = query.Where(s => s.Quantity <= s.Product.MinStock);

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var term = f.Search.Trim().ToLower();
            query = query.Where(s =>
                s.Product.Name.ToLower().Contains(term) ||
                s.Product.Code.ToLower().Contains(term));
        }

        var data = await query
            .OrderBy(s => s.Product.Code)
            .ThenBy(s => s.Warehouse.Code)
            .Select(s => new
            {
                s.Product.Code,
                s.Product.Name,
                Category = s.Product.Category.Name,
                Supplier = s.Product.Supplier != null ? s.Product.Supplier.Name : "—",
                s.Product.Unit,
                Warehouse = s.Warehouse.Name,
                s.Quantity,
                s.Product.MinStock,
                s.Product.SalePrice
            })
            .ToListAsync(ct);

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

        foreach (var row in data)
        {
            var value = row.Quantity * row.SalePrice;
            totalValue += value;
            totalUnits += row.Quantity;

            table.Rows.Add(new List<string>
            {
                row.Code,
                row.Name,
                row.Category,
                row.Supplier,
                row.Warehouse,
                row.Unit,
                row.Quantity.ToString("N0", Es),
                row.MinStock.ToString("N0", Es),
                row.SalePrice.ToString("N2", Es),
                value.ToString("N0", Es),
                StatusText(row.Quantity, row.MinStock)
            });
        }

        table.Summary.Add(("SKU con stock", data.Select(d => d.Code).Distinct().Count().ToString("N0", Es)));
        table.Summary.Add(("Unidades totales", totalUnits.ToString("N0", Es)));
        table.Summary.Add(("Valor de stock", "$" + totalValue.ToString("N0", Es)));

        return table;
    }

    private async Task<ReportTable> BuildMovementsAsync(ReportRequest f, CancellationToken ct)
    {
        var query = _db.Movements.AsNoTracking()
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .Include(m => m.User)
            .AsQueryable();

        if (f.ProductId.HasValue) query = query.Where(m => m.ProductId == f.ProductId.Value);
        if (f.WarehouseId.HasValue) query = query.Where(m => m.WarehouseId == f.WarehouseId.Value);
        if (f.UserId.HasValue) query = query.Where(m => m.UserId == f.UserId.Value);
        if (f.Type.HasValue) query = query.Where(m => m.Type == f.Type.Value);
        if (f.From.HasValue) query = query.Where(m => m.Date >= f.From.Value.ToUniversalTime());
        if (f.To.HasValue) query = query.Where(m => m.Date <= f.To.Value.ToUniversalTime().AddDays(1).AddTicks(-1));

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var term = f.Search.Trim().ToLower();
            query = query.Where(m =>
                m.Product.Name.ToLower().Contains(term) ||
                m.Product.Code.ToLower().Contains(term) ||
                m.Reason.ToLower().Contains(term) ||
                (m.DocumentReference != null && m.DocumentReference.ToLower().Contains(term)));
        }

        var data = await query.OrderByDescending(m => m.Date)
            .Select(m => new
            {
                m.Date,
                Type = m.Type.ToString(),
                m.Product.Code,
                Product = m.Product.Name,
                Warehouse = m.Warehouse.Name,
                User = m.User.FullName,
                m.Quantity,
                m.UnitPrice,
                m.StockAfter,
                m.Reason,
                m.DocumentReference
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

        foreach (var row in data)
        {
            var total = row.Quantity * row.UnitPrice;
            if (row.Type == nameof(MovementType.Entrada))
            {
                entries += row.Quantity;
                entriesValue += total;
            }
            else
            {
                exits += row.Quantity;
                exitsValue += total;
            }

            table.Rows.Add(new List<string>
            {
                row.Date.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                row.Type,
                row.Code,
                row.Product,
                row.Warehouse,
                row.User,
                row.Quantity.ToString("N0", Es),
                row.UnitPrice.ToString("N2", Es),
                total.ToString("N0", Es),
                row.StockAfter.ToString("N0", Es),
                row.Reason,
                row.DocumentReference ?? "—"
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
        var query = _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.StockLevels)
            .AsQueryable();

        if (!f.IncludeInactive) query = query.Where(p => p.IsActive);
        if (f.CategoryId.HasValue) query = query.Where(p => p.CategoryId == f.CategoryId.Value);
        if (f.SupplierId.HasValue) query = query.Where(p => p.SupplierId == f.SupplierId.Value);

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var term = f.Search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) || p.Code.ToLower().Contains(term));
        }

        var data = await query.OrderBy(p => p.Code).ToListAsync(ct);

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

        foreach (var p in data)
        {
            var stock = p.StockLevels.Sum(s => s.Quantity);
            table.Rows.Add(new List<string>
            {
                p.Code,
                p.Name,
                p.Category.Name,
                p.Supplier?.Name ?? "—",
                p.Unit,
                stock.ToString("N0", Es),
                p.MinStock.ToString("N0", Es),
                p.PurchasePrice.ToString("N2", Es),
                p.SalePrice.ToString("N2", Es),
                p.IsActive ? Mapping.ProductStatus(stock, p.MinStock) switch
                {
                    "ok" => "Activo / OK",
                    "low" => "Stock bajo",
                    "critical" => "Crítico",
                    _ => "Sin stock"
                } : "Inactivo"
            });
        }

        table.Summary.Add(("Productos", data.Count.ToString("N0", Es)));
        table.Summary.Add(("Activos", data.Count(p => p.IsActive).ToString("N0", Es)));
        table.Summary.Add(("Con stock bajo", data.Count(p => p.StockLevels.Sum(s => s.Quantity) <= p.MinStock).ToString("N0", Es)));

        return table;
    }

    private async Task<ReportTable> BuildCategoriesAsync(ReportRequest f, CancellationToken ct)
    {
        var query = _db.Categories.AsNoTracking().Include(c => c.Products).AsQueryable();
        if (!f.IncludeInactive) query = query.Where(c => c.IsActive);
        var data = await query.OrderBy(c => c.Name).ToListAsync(ct);

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
                c.Name,
                c.Description ?? "—",
                c.Products.Count(p => p.IsActive).ToString("N0", Es),
                c.Products.Count.ToString("N0", Es),
                c.IsActive ? "Activa" : "Inactiva"
            });
        }

        table.Summary.Add(("Categorías", data.Count.ToString("N0", Es)));
        table.Summary.Add(("Activas", data.Count(c => c.IsActive).ToString("N0", Es)));

        return table;
    }

    private async Task<ReportTable> BuildSuppliersAsync(ReportRequest f, CancellationToken ct)
    {
        var query = _db.Suppliers.AsNoTracking().Include(s => s.Products).AsQueryable();
        if (!f.IncludeInactive) query = query.Where(s => s.IsActive);

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var term = f.Search.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(term));
        }

        var data = await query.OrderBy(s => s.Name).ToListAsync(ct);

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
                s.Name,
                s.ContactName ?? "—",
                s.Phone ?? "—",
                s.Email ?? "—",
                s.Address ?? "—",
                s.Products.Count(p => p.IsActive).ToString("N0", Es),
                s.IsActive ? "Activo" : "Inactivo"
            });
        }

        table.Summary.Add(("Proveedores", data.Count.ToString("N0", Es)));
        table.Summary.Add(("Activos", data.Count(s => s.IsActive).ToString("N0", Es)));

        return table;
    }

    private async Task<ReportTable> BuildUsersAsync(ReportRequest f, CancellationToken ct)
    {
        var query = _db.Users.AsNoTracking().Include(u => u.Role).AsQueryable();
        if (!f.IncludeInactive) query = query.Where(u => u.IsActive);

        var data = await query.OrderBy(u => u.Username).ToListAsync(ct);

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
                u.Username,
                u.FullName,
                u.Email ?? "—",
                u.Role.Name,
                u.IsActive ? "Activo" : "Inactivo",
                u.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy")
            });
        }

        table.Summary.Add(("Usuarios", data.Count.ToString("N0", Es)));
        table.Summary.Add(("Activos", data.Count(u => u.IsActive).ToString("N0", Es)));

        return table;
    }

    private static string StatusText(int quantity, int minStock) => quantity switch
    {
        <= 0 => "Sin stock",
        _ when quantity <= Math.Max(1, minStock / 2) => "Crítico",
        _ when quantity <= minStock => "Stock bajo",
        _ => "OK"
    };

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

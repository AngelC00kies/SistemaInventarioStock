using Microsoft.EntityFrameworkCore;
using SistemaInventario.Core.Common;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Entities;
using SistemaInventario.Core.Enums;
using SistemaInventario.Core.Exceptions;
using SistemaInventario.Core.Interfaces;
using SistemaInventario.Infrastructure.Data;

namespace SistemaInventario.Infrastructure.Services;

public class MovementService : IMovementService
{
    private readonly AppDbContext _db;

    public MovementService(AppDbContext db) => _db = db;

    public async Task<MovementDto> CreateAsync(CreateMovementRequest request, int userId, CancellationToken ct = default)
    {
        if (request.Quantity <= 0)
            throw new AppException("La cantidad debe ser mayor a cero.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new AppException("El motivo del movimiento es obligatorio.");
        if (request.Type != MovementType.Entrada && request.Type != MovementType.Salida)
            throw new AppException("Tipo de movimiento no válido.");

        var product = await _db.Productos.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new NotFoundException("Producto no encontrado.");
        if (!product.Activo)
            throw new AppException("No se pueden registrar movimientos de un producto inactivo.");

        var warehouse = await _db.Almacenes.FirstOrDefaultAsync(w => w.Id == request.WarehouseId, ct)
            ?? throw new NotFoundException("Almacén no encontrado.");
        if (!warehouse.Activo)
            throw new AppException("No se pueden registrar movimientos en un almacén inactivo.");

        var stock = await _db.NivelesStock
            .FirstOrDefaultAsync(s => s.ProductoId == request.ProductId && s.AlmacenId == request.WarehouseId, ct);

        if (request.Type == MovementType.Salida)
        {
            var available = stock?.Cantidad ?? 0;
            if (request.Quantity > available)
                throw new AppException(
                    $"Stock insuficiente. Disponible: {available} {product.Unidad.ToLower()} en {warehouse.Nombre}.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var isNewStock = stock is null;
        stock ??= new NivelStock
        {
            ProductoId = request.ProductId,
            AlmacenId = request.WarehouseId,
            Cantidad = 0
        };
        if (isNewStock) _db.NivelesStock.Add(stock);

        stock.Cantidad += request.Type == MovementType.Entrada ? request.Quantity : -request.Quantity;

        var unitPrice = request.UnitPrice ??
                        (request.Type == MovementType.Entrada ? product.PrecioCompra : product.PrecioVenta);

        var movement = new Movimiento
        {
            Fecha = request.Date ?? DateTime.UtcNow,
            Tipo = request.Type,
            Motivo = request.Reason.Trim(),
            Cantidad = request.Quantity,
            ProductoId = request.ProductId,
            AlmacenId = request.WarehouseId,
            UsuarioId = userId,
            DocumentoReferencia = string.IsNullOrWhiteSpace(request.DocumentReference)
                ? null
                : request.DocumentReference.Trim(),
            StockResultante = stock.Cantidad,
            PrecioUnitario = unitPrice
        };

        _db.Movimientos.Add(movement);

        await CheckLowStockAsync(product, warehouse.Id, stock.Cantidad, ct);
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        var created = await _db.Movimientos.AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.Almacen)
            .Include(m => m.Usuario)
            .FirstAsync(m => m.Id == movement.Id, ct);

        return Mapping.ToDto(created);
    }

    public async Task<PagedResult<MovementDto>> GetAsync(MovementFilter filter, CancellationToken ct = default)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);

        var query = _db.Movimientos.AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.Almacen)
            .Include(m => m.Usuario)
            .AsQueryable();

        if (filter.ProductId.HasValue) query = query.Where(m => m.ProductoId == filter.ProductId.Value);
        if (filter.WarehouseId.HasValue) query = query.Where(m => m.AlmacenId == filter.WarehouseId.Value);
        if (filter.UserId.HasValue) query = query.Where(m => m.UsuarioId == filter.UserId.Value);
        if (filter.Type.HasValue) query = query.Where(m => m.Tipo == filter.Type.Value);
        if (filter.From.HasValue) query = query.Where(m => m.Fecha >= filter.From.Value.ToUniversalTime());
        if (filter.To.HasValue)
        {
            var to = filter.To.Value.ToUniversalTime().AddDays(1).AddTicks(-1);
            query = query.Where(m => m.Fecha <= to);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(m =>
                m.Producto.Nombre.ToLower().Contains(term) ||
                m.Producto.Codigo.ToLower().Contains(term) ||
                m.Motivo.ToLower().Contains(term) ||
                (m.DocumentoReferencia != null && m.DocumentoReferencia.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(m => m.Fecha)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => Mapping.ToDto(m))
            .ToListAsync(ct);

        return new PagedResult<MovementDto> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<MovementDto> GetAsync(int id, CancellationToken ct = default)
    {
        var movement = await _db.Movimientos.AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.Almacen)
            .Include(m => m.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException("Movimiento no encontrado.");

        return Mapping.ToDto(movement);
    }

    private async Task CheckLowStockAsync(Producto product, int warehouseId, int quantity, CancellationToken ct)
    {
        var pending = await _db.Notificaciones.AnyAsync(n =>
            n.ProductoId == product.Id &&
            n.AlmacenId == warehouseId &&
            !n.Leida, ct);

        if (quantity > product.StockMinimo)
        {
            if (pending)
            {
                var toClose = await _db.Notificaciones
                    .Where(n => n.ProductoId == product.Id && n.AlmacenId == warehouseId && !n.Leida)
                    .ToListAsync(ct);
                toClose.ForEach(n => n.Leida = true);
            }
            return;
        }

        if (pending) return;

        _db.Notificaciones.Add(new Notificacion
        {
            ProductoId = product.Id,
            AlmacenId = warehouseId,
            Nivel = quantity <= Math.Max(1, product.StockMinimo / 2) ? NotificationLevel.Critical : NotificationLevel.Warning,
            Mensaje = $"Stock bajo: \"{product.Nombre}\" tiene {quantity} {product.Unidad.ToLower()} " +
                      $"(mínimo {product.StockMinimo})."
        });
    }
}

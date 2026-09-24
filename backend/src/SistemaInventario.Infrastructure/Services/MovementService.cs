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

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new NotFoundException("Producto no encontrado.");
        if (!product.IsActive)
            throw new AppException("No se pueden registrar movimientos de un producto inactivo.");

        var warehouse = await _db.Warehouses.FirstOrDefaultAsync(w => w.Id == request.WarehouseId, ct)
            ?? throw new NotFoundException("Almacén no encontrado.");
        if (!warehouse.IsActive)
            throw new AppException("No se pueden registrar movimientos en un almacén inactivo.");

        var stock = await _db.StockLevels
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId && s.WarehouseId == request.WarehouseId, ct);

        if (request.Type == MovementType.Salida)
        {
            var available = stock?.Quantity ?? 0;
            if (request.Quantity > available)
                throw new AppException(
                    $"Stock insuficiente. Disponible: {available} {product.Unit.ToLower()} en {warehouse.Name}.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var isNewStock = stock is null;
        stock ??= new StockLevel
        {
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            Quantity = 0
        };
        if (isNewStock) _db.StockLevels.Add(stock);

        stock.Quantity += request.Type == MovementType.Entrada ? request.Quantity : -request.Quantity;

        var unitPrice = request.UnitPrice ??
                        (request.Type == MovementType.Entrada ? product.PurchasePrice : product.SalePrice);

        var movement = new Movement
        {
            Date = request.Date ?? DateTime.UtcNow,
            Type = request.Type,
            Reason = request.Reason.Trim(),
            Quantity = request.Quantity,
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            UserId = userId,
            DocumentReference = string.IsNullOrWhiteSpace(request.DocumentReference)
                ? null
                : request.DocumentReference.Trim(),
            StockAfter = stock.Quantity,
            UnitPrice = unitPrice
        };

        _db.Movements.Add(movement);

        await CheckLowStockAsync(product, warehouse.Id, stock.Quantity, ct);
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        var created = await _db.Movements.AsNoTracking()
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .Include(m => m.User)
            .FirstAsync(m => m.Id == movement.Id, ct);

        return Mapping.ToDto(created);
    }

    public async Task<PagedResult<MovementDto>> GetAsync(MovementFilter filter, CancellationToken ct = default)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);

        var query = _db.Movements.AsNoTracking()
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .Include(m => m.User)
            .AsQueryable();

        if (filter.ProductId.HasValue) query = query.Where(m => m.ProductId == filter.ProductId.Value);
        if (filter.WarehouseId.HasValue) query = query.Where(m => m.WarehouseId == filter.WarehouseId.Value);
        if (filter.UserId.HasValue) query = query.Where(m => m.UserId == filter.UserId.Value);
        if (filter.Type.HasValue) query = query.Where(m => m.Type == filter.Type.Value);
        if (filter.From.HasValue) query = query.Where(m => m.Date >= filter.From.Value.ToUniversalTime());
        if (filter.To.HasValue)
        {
            var to = filter.To.Value.ToUniversalTime().AddDays(1).AddTicks(-1);
            query = query.Where(m => m.Date <= to);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(m =>
                m.Product.Name.ToLower().Contains(term) ||
                m.Product.Code.ToLower().Contains(term) ||
                m.Reason.ToLower().Contains(term) ||
                (m.DocumentReference != null && m.DocumentReference.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(m => m.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => Mapping.ToDto(m))
            .ToListAsync(ct);

        return new PagedResult<MovementDto> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<MovementDto> GetAsync(int id, CancellationToken ct = default)
    {
        var movement = await _db.Movements.AsNoTracking()
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException("Movimiento no encontrado.");

        return Mapping.ToDto(movement);
    }

    private async Task CheckLowStockAsync(Product product, int warehouseId, int quantity, CancellationToken ct)
    {
        var pending = await _db.Notifications.AnyAsync(n =>
            n.ProductId == product.Id &&
            n.WarehouseId == warehouseId &&
            !n.IsRead, ct);

        if (quantity > product.MinStock)
        {
            if (pending)
            {
                var toClose = await _db.Notifications
                    .Where(n => n.ProductId == product.Id && n.WarehouseId == warehouseId && !n.IsRead)
                    .ToListAsync(ct);
                toClose.ForEach(n => n.IsRead = true);
            }
            return;
        }

        if (pending) return;

        _db.Notifications.Add(new AppNotification
        {
            ProductId = product.Id,
            WarehouseId = warehouseId,
            Level = quantity <= Math.Max(1, product.MinStock / 2) ? NotificationLevel.Critical : NotificationLevel.Warning,
            Message = $"Stock bajo: \"{product.Name}\" tiene {quantity} {product.Unit.ToLower()} " +
                      $"(mínimo {product.MinStock})."
        });
    }
}

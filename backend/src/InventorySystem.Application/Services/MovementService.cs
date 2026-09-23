using InventorySystem.Application.Common;
using InventorySystem.Application.Dtos;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using InventorySystem.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Application.Services;

/// <summary>
/// Registro de movimientos con actualización atómica del stock y
/// generación automática de alertas de stock bajo.
/// </summary>
public class MovementService : IMovementService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public MovementService(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<MovementDto> CreateAsync(MovementRequest request, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
            throw new ForbiddenException("Debe iniciar sesión para registrar movimientos.");

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new NotFoundException($"Producto {request.ProductId} no encontrado.");

        if (!product.IsActive)
            throw new AppException("No se pueden registrar movimientos de un producto inactivo.");

        var warehouse = await _db.Warehouses.FirstOrDefaultAsync(w => w.Id == request.WarehouseId, ct)
            ?? throw new NotFoundException($"Almacén {request.WarehouseId} no encontrado.");

        if (!warehouse.IsActive)
            throw new AppException("No se pueden registrar movimientos en un almacén inactivo.");

        var quantity = request.Quantity;
        var stockAfter = request.Type == MovementType.Entrada
            ? await ApplyEntryAsync(request, ct)
            : await ApplyExitAsync(request, ct);

        var movement = new Movement
        {
            Date = DateTime.UtcNow,
            Type = request.Type,
            Reason = request.Reason.Trim(),
            Quantity = quantity,
            StockAfter = stockAfter,
            DocumentReference = string.IsNullOrWhiteSpace(request.DocumentReference)
                ? null
                : request.DocumentReference.Trim(),
            ProductId = product.Id,
            WarehouseId = warehouse.Id,
            UserId = _currentUser.UserId!,
            UserName = _currentUser.UserName ?? "desconocido"
        };

        _db.Movements.Add(movement);
        await UpdateLowStockStateAsync(product, warehouse.Id, stockAfter, ct);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(movement.Id, ct);
    }

    /// <summary>Suma la cantidad al stock (crea el registro si no existe) y devuelve el nuevo total.</summary>
    private async Task<int> ApplyEntryAsync(MovementRequest request, CancellationToken ct)
    {
        var stock = await _db.Stocks
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId && s.WarehouseId == request.WarehouseId, ct);

        if (stock is null)
        {
            stock = new Stock
            {
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                Quantity = request.Quantity,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Stocks.Add(stock);
        }
        else
        {
            stock.Quantity += request.Quantity;
            stock.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return stock.Quantity;
    }

    /// <summary>
    /// Resta la cantidad con una condición atómica: nunca deja stock negativo.
    /// Devuelve el stock resultante o lanza excepción si no hay suficiente.
    /// </summary>
    private async Task<int> ApplyExitAsync(MovementRequest request, CancellationToken ct)
    {
        // UPDATE condicional: solo descuenta si hay stock suficiente (evita condiciones de carrera).
        var affected = await _db.Stocks
            .Where(s => s.ProductId == request.ProductId &&
                        s.WarehouseId == request.WarehouseId &&
                        s.Quantity >= request.Quantity)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.Quantity, s => s.Quantity - request.Quantity)
                .SetProperty(s => s.UpdatedAt, _ => DateTime.UtcNow), ct);

        if (affected == 0)
        {
            var exists = await _db.Stocks.AnyAsync(
                s => s.ProductId == request.ProductId && s.WarehouseId == request.WarehouseId, ct);

            if (!exists)
                throw new AppException("El producto no tiene stock registrado en el almacén seleccionado.");

            var current = await _db.Stocks
                .Where(s => s.ProductId == request.ProductId && s.WarehouseId == request.WarehouseId)
                .Select(s => s.Quantity)
                .FirstAsync(ct);

            throw new AppException(
                $"Stock insuficiente: hay {current} unidades disponibles y se intentaron retirar {request.Quantity}.");
        }

        var stockAfter = await _db.Stocks
            .Where(s => s.ProductId == request.ProductId && s.WarehouseId == request.WarehouseId)
            .Select(s => s.Quantity)
            .FirstAsync(ct);

        return stockAfter;
    }

    /// <summary>Crea/resuelve alertas de stock bajo tras cada movimiento.</summary>
    private async Task UpdateLowStockStateAsync(Product product, int warehouseId, int stockAfter, CancellationToken ct)
    {
        var pending = await _db.LowStockNotifications
            .Where(n => n.ProductId == product.Id && n.WarehouseId == warehouseId && !n.IsResolved)
            .FirstOrDefaultAsync(ct);

        if (product.MinimumStock > 0 && stockAfter <= product.MinimumStock)
        {
            if (pending is null)
            {
                _db.LowStockNotifications.Add(new LowStockNotification
                {
                    ProductId = product.Id,
                    WarehouseId = warehouseId,
                    QuantityAtDetection = stockAfter,
                    MinimumStock = product.MinimumStock,
                    DetectedAt = DateTime.UtcNow
                });
            }
            else
            {
                pending.QuantityAtDetection = stockAfter;
                pending.MinimumStock = product.MinimumStock;
                pending.DetectedAt = DateTime.UtcNow;
            }
        }
        else if (pending is not null)
        {
            pending.IsResolved = true;
            pending.ResolvedAt = DateTime.UtcNow;
        }
    }

    public async Task<PagedResult<MovementDto>> GetAsync(MovementQuery query, CancellationToken ct = default)
    {
        var q = _db.Movements.AsNoTracking();

        if (query.ProductId.HasValue)
            q = q.Where(m => m.ProductId == query.ProductId);

        if (query.WarehouseId.HasValue)
            q = q.Where(m => m.WarehouseId == query.WarehouseId);

        if (query.Type.HasValue)
            q = q.Where(m => m.Type == query.Type);

        if (!string.IsNullOrWhiteSpace(query.UserId))
            q = q.Where(m => m.UserId == query.UserId);

        if (query.From.HasValue)
            q = q.Where(m => m.Date >= query.From.Value.ToUniversalTime());

        if (query.To.HasValue)
        {
            var to = query.To.Value.ToUniversalTime().Date.AddDays(1);
            q = q.Where(m => m.Date < to);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(m => m.Product.Name.Contains(term) ||
                             m.Product.Code.Contains(term) ||
                             m.Reason.Contains(term) ||
                             (m.DocumentReference != null && m.DocumentReference.Contains(term)));
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(m => m.Date)
            .ThenByDescending(m => m.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(m => new MovementDto
            {
                Id = m.Id,
                Date = m.Date,
                Type = m.Type,
                Reason = m.Reason,
                Quantity = m.Quantity,
                StockAfter = m.StockAfter,
                DocumentReference = m.DocumentReference,
                ProductId = m.ProductId,
                ProductCode = m.Product.Code,
                ProductName = m.Product.Name,
                WarehouseId = m.WarehouseId,
                WarehouseName = m.Warehouse.Name,
                UserId = m.UserId,
                UserName = m.UserName
            })
            .ToListAsync(ct);

        return new PagedResult<MovementDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<MovementDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var dto = await _db.Movements.AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MovementDto
            {
                Id = m.Id,
                Date = m.Date,
                Type = m.Type,
                Reason = m.Reason,
                Quantity = m.Quantity,
                StockAfter = m.StockAfter,
                DocumentReference = m.DocumentReference,
                ProductId = m.ProductId,
                ProductCode = m.Product.Code,
                ProductName = m.Product.Name,
                WarehouseId = m.WarehouseId,
                WarehouseName = m.Warehouse.Name,
                UserId = m.UserId,
                UserName = m.UserName
            })
            .FirstOrDefaultAsync(ct);

        return dto ?? throw new NotFoundException($"Movimiento {id} no encontrado.");
    }
}

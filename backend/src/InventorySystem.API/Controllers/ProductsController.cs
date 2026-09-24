using InventorySystem.Application.Common;
using InventorySystem.Application.Dtos;
using InventorySystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _products;
    private readonly IMovementService _movements;

    public ProductsController(IProductService products, IMovementService movements)
    {
        _products = products;
        _movements = movements;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> Get([FromQuery] ProductQuery query, CancellationToken ct)
        => Ok(await _products.GetAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken ct)
        => Ok(await _products.GetByIdAsync(id, ct));

    [HttpGet("{id:int}/stock")]
    public async Task<ActionResult<IReadOnlyList<StockEntryDto>>> GetStock(int id, CancellationToken ct)
        => Ok(await _products.GetStockAsync(id, ct));

    [HttpGet("{id:int}/movements")]
    public async Task<ActionResult<PagedResult<MovementDto>>> GetMovements(
        int id, [FromQuery] MovementQuery query, CancellationToken ct)
    {
        query.ProductId = id;
        return Ok(await _movements.GetAsync(query, ct));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Usuario")]
    public async Task<ActionResult<ProductDto>> Create([FromBody] ProductRequest request, CancellationToken ct)
    {
        var created = await _products.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Usuario")]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] ProductRequest request, CancellationToken ct)
        => Ok(await _products.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _products.DeleteAsync(id, ct);
        return NoContent();
    }
}

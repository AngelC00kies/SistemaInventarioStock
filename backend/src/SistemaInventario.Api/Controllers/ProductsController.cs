using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.Core.Common;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Interfaces;

namespace SistemaInventario.Api.Controllers;

/// <summary>Productos con búsqueda, filtros y paginación, más su alta, edición y baja lógica.</summary>
[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _products;

    public ProductsController(IProductService products) => _products = products;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll([FromQuery] ProductFilter filter, CancellationToken ct)
        => Ok(await _products.GetAsync(filter, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken ct)
        => Ok(await _products.GetAsync(id, ct));

    [HttpPost]
    [Authorize(Policy = "ReadWrite")]
    public async Task<ActionResult<ProductDto>> Create([FromBody] ProductRequest request, CancellationToken ct)
    {
        var created = await _products.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "ReadWrite")]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] ProductRequest request, CancellationToken ct)
        => Ok(await _products.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ReadWrite")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _products.DeleteAsync(id, ct);
        return NoContent();
    }
}

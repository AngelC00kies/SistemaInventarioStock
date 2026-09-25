using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Interfaces;

namespace SistemaInventario.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categories;

    public CategoriesController(ICategoryService categories) => _categories = categories;

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] bool includeInactive = false, CancellationToken ct = default)
        => Ok(await _categories.GetAsync(search, includeInactive, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken ct)
        => Ok(await _categories.GetAsync(id, ct));

    [HttpPost]
    [Authorize(Policy = "ReadWrite")]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryRequest request, CancellationToken ct)
    {
        var created = await _categories.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "ReadWrite")]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] CategoryRequest request, CancellationToken ct)
        => Ok(await _categories.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ReadWrite")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _categories.DeleteAsync(id, ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _suppliers;

    public SuppliersController(ISupplierService suppliers) => _suppliers = suppliers;

    [HttpGet]
    public async Task<ActionResult<List<SupplierDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] bool includeInactive = false, CancellationToken ct = default)
        => Ok(await _suppliers.GetAsync(search, includeInactive, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupplierDto>> GetById(int id, CancellationToken ct)
        => Ok(await _suppliers.GetAsync(id, ct));

    [HttpPost]
    [Authorize(Policy = "ReadWrite")]
    public async Task<ActionResult<SupplierDto>> Create([FromBody] SupplierRequest request, CancellationToken ct)
    {
        var created = await _suppliers.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "ReadWrite")]
    public async Task<ActionResult<SupplierDto>> Update(int id, [FromBody] SupplierRequest request, CancellationToken ct)
        => Ok(await _suppliers.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ReadWrite")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _suppliers.DeleteAsync(id, ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/warehouses")]
[Authorize]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouses;

    public WarehousesController(IWarehouseService warehouses) => _warehouses = warehouses;

    [HttpGet]
    public async Task<ActionResult<List<WarehouseDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] bool includeInactive = false, CancellationToken ct = default)
        => Ok(await _warehouses.GetAsync(search, includeInactive, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WarehouseDto>> GetById(int id, CancellationToken ct)
        => Ok(await _warehouses.GetAsync(id, ct));

    [HttpPost]
    [Authorize(Policy = "ReadWrite")]
    public async Task<ActionResult<WarehouseDto>> Create([FromBody] WarehouseRequest request, CancellationToken ct)
    {
        var created = await _warehouses.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "ReadWrite")]
    public async Task<ActionResult<WarehouseDto>> Update(int id, [FromBody] WarehouseRequest request, CancellationToken ct)
        => Ok(await _warehouses.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ReadWrite")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _warehouses.DeleteAsync(id, ct);
        return NoContent();
    }
}

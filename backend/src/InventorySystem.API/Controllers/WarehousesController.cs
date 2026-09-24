using InventorySystem.Application.Dtos;
using InventorySystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouses;

    public WarehousesController(IWarehouseService warehouses) => _warehouses = warehouses;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WarehouseDto>>> Get(
        [FromQuery] bool includeInactive = false, CancellationToken ct = default)
        => Ok(await _warehouses.GetAsync(includeInactive, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WarehouseDto>> GetById(int id, CancellationToken ct)
        => Ok(await _warehouses.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = "Admin,Usuario")]
    public async Task<ActionResult<WarehouseDto>> Create([FromBody] WarehouseRequest request, CancellationToken ct)
    {
        var created = await _warehouses.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Usuario")]
    public async Task<ActionResult<WarehouseDto>> Update(int id, [FromBody] WarehouseRequest request, CancellationToken ct)
        => Ok(await _warehouses.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _warehouses.DeleteAsync(id, ct);
        return NoContent();
    }
}

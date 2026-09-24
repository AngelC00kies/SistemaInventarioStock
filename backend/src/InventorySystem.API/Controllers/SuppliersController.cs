using InventorySystem.Application.Common;
using InventorySystem.Application.Dtos;
using InventorySystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _suppliers;

    public SuppliersController(ISupplierService suppliers) => _suppliers = suppliers;

    [HttpGet]
    public async Task<ActionResult<PagedResult<SupplierDto>>> Get([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await _suppliers.GetAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupplierDto>> GetById(int id, CancellationToken ct)
        => Ok(await _suppliers.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = "Admin,Usuario")]
    public async Task<ActionResult<SupplierDto>> Create([FromBody] SupplierRequest request, CancellationToken ct)
    {
        var created = await _suppliers.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Usuario")]
    public async Task<ActionResult<SupplierDto>> Update(int id, [FromBody] SupplierRequest request, CancellationToken ct)
        => Ok(await _suppliers.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _suppliers.DeleteAsync(id, ct);
        return NoContent();
    }
}

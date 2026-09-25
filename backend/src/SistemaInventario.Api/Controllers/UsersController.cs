using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.Core.Common;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Interfaces;

namespace SistemaInventario.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users) => _users = users;

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 15,
        CancellationToken ct = default)
        => Ok(await _users.GetAsync(search, page, pageSize, ct));

    [HttpGet("roles")]
    public async Task<ActionResult<List<RoleDto>>> GetRoles(CancellationToken ct)
        => Ok(await _users.GetRolesAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id, CancellationToken ct)
        => Ok(await _users.GetAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var created = await _users.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserRequest request, CancellationToken ct)
        => Ok(await _users.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _users.DeleteAsync(id, ct);
        return NoContent();
    }
}

using InventorySystem.Application.Common;
using InventorySystem.Application.Dtos;
using InventorySystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Controllers;

[ApiController]
[Route("api/movements")]
[Authorize]
public class MovementsController : ControllerBase
{
    private readonly IMovementService _movements;

    public MovementsController(IMovementService movements) => _movements = movements;

    [HttpGet]
    public async Task<ActionResult<PagedResult<MovementDto>>> Get([FromQuery] MovementQuery query, CancellationToken ct)
        => Ok(await _movements.GetAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovementDto>> GetById(int id, CancellationToken ct)
        => Ok(await _movements.GetByIdAsync(id, ct));

    /// <summary>Registra una entrada o salida y actualiza el stock automáticamente.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Usuario")]
    public async Task<ActionResult<MovementDto>> Create([FromBody] MovementRequest request, CancellationToken ct)
    {
        var created = await _movements.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications) => _notifications = notifications;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> Get(
        [FromQuery] bool pendingOnly = true, CancellationToken ct = default)
        => Ok(await _notifications.GetAsync(pendingOnly, ct));

    [HttpGet("pending-count")]
    public async Task<ActionResult<int>> GetPendingCount(CancellationToken ct)
        => Ok(await _notifications.GetPendingCountAsync(ct));

    [HttpPost("{id:int}/ack")]
    [Authorize(Roles = "Admin,Usuario")]
    public async Task<IActionResult> Acknowledge(int id, CancellationToken ct)
    {
        await _notifications.AcknowledgeAsync(id, ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken ct)
        => Ok(await _dashboard.GetAsync(ct));
}

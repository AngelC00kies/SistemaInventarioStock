using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.Core.Common;
using SistemaInventario.Core.Dtos;
using SistemaInventario.Core.Interfaces;

namespace SistemaInventario.Api.Controllers;

[ApiController]
[Route("api/movements")]
[Authorize]
public class MovementsController : ControllerBase
{
    private readonly IMovementService _movements;

    public MovementsController(IMovementService movements) => _movements = movements;

    [HttpGet]
    public async Task<ActionResult<PagedResult<MovementDto>>> GetAll([FromQuery] MovementFilter filter, CancellationToken ct)
        => Ok(await _movements.GetAsync(filter, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovementDto>> GetById(int id, CancellationToken ct)
        => Ok(await _movements.GetAsync(id, ct));

    [HttpPost]
    [Authorize(Policy = "ReadWrite")]
    public async Task<ActionResult<MovementDto>> Create([FromBody] CreateMovementRequest request, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var created = await _movements.CreateAsync(request, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications) => _notifications = notifications;

    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetAll(
        [FromQuery] int limit = 30, [FromQuery] bool onlyUnread = false, CancellationToken ct = default)
        => Ok(await _notifications.GetAsync(limit, onlyUnread, ct));

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> UnreadCount(CancellationToken ct)
        => Ok(await _notifications.GetUnreadCountAsync(ct));

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken ct)
    {
        await _notifications.MarkAsReadAsync(id, ct);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        await _notifications.MarkAllAsReadAsync(ct);
        return NoContent();
    }
}

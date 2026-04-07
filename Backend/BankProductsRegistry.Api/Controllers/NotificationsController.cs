using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Controllers;

[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(BankProductsDbContext dbContext) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<NotificationResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var notifications = await dbContext.SystemNotifications
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        var response = notifications.Select(n => new NotificationResponse(
            n.Id, n.Title, n.Message, n.Type, n.CreatedAt, n.IsRead
        )).ToList();

        return Ok(response);
    }

    [HttpPost("{id:int}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsReadAsync(int id, CancellationToken cancellationToken)
    {
        var notification = await dbContext.SystemNotifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

        if (notification is null) return NotFound();

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Ok();
    }
}
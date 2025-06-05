using Be.DTOs.Notification;
using Be.Services.NotificationService;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("user")]
    public async Task<IActionResult> SendToUser([FromBody] CreateNotificationDto dto)
    {
        try
        {
            await _notificationService.SendToUserAsync(dto);
            return Ok(new { message = "Notification sent to user successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to send notification.", details = ex.Message });
        }
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> SendToAll([FromBody] BulkNotificationDto dto)
    {
        try
        {
            await _notificationService.SendToAllUsersAsync(dto);
            return Ok(new { message = "Bulk email notifications sent successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to send bulk email notifications.", details = ex.Message });
        }
    }

    [HttpGet("account/{accountId}")]
    public async Task<IActionResult> GetByAccount(int accountId)
    {
        try
        {
            var result = await _notificationService.GetByAccountIdAsync(accountId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to retrieve user notifications.", details = ex.Message });
        }
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(new { message = "Notification marked as read." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to update notification.", details = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            // Basic validation for page number and page size
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Set a reasonable max page size

            var (notifications, totalCount) = await _notificationService.GetPaginatedNotificationsAsync(pageNumber, pageSize);

            return Ok(new
            {
                notifications = notifications,
                totalCount = totalCount,
                pageNumber = pageNumber,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to retrieve paginated notifications.", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _notificationService.DeleteNotificationAsync(id);
            if (success)
            {
                return Ok(new { message = "Notification deleted successfully." });
            }
            else
            {
                return NotFound(new { message = "Notification not found." });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to delete notification.", details = ex.Message });
        }
    }
}

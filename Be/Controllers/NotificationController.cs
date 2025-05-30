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
}

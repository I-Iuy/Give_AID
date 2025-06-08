using Be.DTOs.Notification;
using Be.Services.NotificationService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
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
            _logger.LogError(ex, "Error sending notification to user.");
            return StatusCode(500, new { message = "Failed to send notification.", details = ex.Message });
        }
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> SendToAll([FromBody] BulkNotificationDto dto)
    {
        _logger.LogInformation("SendToAll: Received bulk notification request.");
        
        if (dto == null)
        {
            _logger.LogWarning("SendToAll: BulkNotificationDto is null");
            return BadRequest(new { message = "Invalid request data" });
        }

        _logger.LogInformation("SendToAll: BulkNotificationDto received - Title: '{Title}', Message: '{Message}', CampaignId: {CampaignId}", 
            dto.Title, dto.Message, dto.CampaignId);

        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Message))
        {
            _logger.LogWarning("SendToAll: Title or Message is empty");
            return BadRequest(new { message = "Title and Message are required" });
        }

        if (dto.Title.Length < 3 || dto.Title.Length > 100)
        {
            _logger.LogWarning("SendToAll: Invalid Title length");
            return BadRequest(new { message = "Title must be between 3 and 100 characters" });
        }

        if (dto.Message.Length < 3 || dto.Message.Length > 500)
        {
            _logger.LogWarning("SendToAll: Invalid Message length");
            return BadRequest(new { message = "Message must be between 3 and 500 characters" });
        }

        try
        {
            await _notificationService.SendToAllUsersAsync(dto);
            _logger.LogInformation("SendToAll: Bulk email notifications sent successfully");
            return Ok(new { message = "Bulk email notifications sent successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendToAll: Error sending bulk email notifications");
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
            _logger.LogError(ex, "Error retrieving user notifications for account {AccountId}.", accountId);
            return StatusCode(500, new { message = "Failed to retrieve user notifications.", details = ex.Message });
        }
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            var notification = await _notificationService.GetByIdAsync(id);
            if (notification == null)
            {
                return NotFound(new { message = "Notification not found." });
            }

            notification.IsRead = true;
            await _notificationService.UpdateAsync(notification);
            return Ok(new { message = "Notification marked as read successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read.", id);
            return StatusCode(500, new { message = "Failed to mark notification as read.", details = ex.Message });
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
            _logger.LogError(ex, "Error retrieving paginated notifications.");
            return StatusCode(500, new { message = "Failed to retrieve paginated notifications.", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            _logger.LogInformation("Backend: Attempting to delete notification {NotificationId}", id);
            
            var success = await _notificationService.DeleteNotificationAsync(id);
            _logger.LogInformation("Backend: Delete notification result: {Success}", success);

            if (success)
            {
                _logger.LogInformation("Backend: Successfully deleted notification {NotificationId}", id);
                return Ok(new { message = "Notification deleted successfully." });
            }
            else
            {
                _logger.LogWarning("Backend: Notification {NotificationId} not found", id);
                return NotFound(new { message = "Notification not found." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backend: Error deleting notification {NotificationId}", id);
            return StatusCode(500, new { message = "Failed to delete notification.", details = ex.Message });
        }
    }
}

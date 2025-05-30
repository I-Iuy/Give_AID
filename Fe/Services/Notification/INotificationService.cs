using Fe.DTOs.Notification;

namespace Fe.Services.Notification
{
    public interface INotificationService
    {
        Task<IEnumerable<UserNotificationDto>> GetByAccountIdAsync(int accountId);
        Task<bool> SendToUserAsync(CreateNotificationDto dto);
        Task<bool> SendBulkAsync(BulkNotificationDto dto);
        Task<bool> MarkAsReadAsync(int notificationId);
    }
}

using Be.DTOs.Notification;

namespace Be.Services.NotificationService
{
    public interface INotificationService
    {
        Task SendToUserAsync(CreateNotificationDto dto);
        Task SendToAllUsersAsync(BulkNotificationDto dto);
        Task<IEnumerable<UserNotificationDto>> GetByAccountIdAsync(int accountId);
        Task MarkAsReadAsync(int notificationId);
    }
}

using Be.DTOs.Notification;

namespace Be.Services.NotificationService
{
    public interface INotificationService
    {
        Task SendToUserAsync(CreateNotificationDto dto);
        Task SendToAllUsersAsync(BulkNotificationDto dto);
        Task<IEnumerable<UserNotificationDto>> GetByAccountIdAsync(int accountId);
        Task MarkAsReadAsync(int notificationId);
        Task<bool> DeleteNotificationAsync(int notificationId);
        Task<(IEnumerable<UserNotificationDto> notifications, int totalCount)> GetPaginatedNotificationsAsync(int pageNumber, int pageSize);
        Task<UserNotificationDto?> GetByIdAsync(int id);
        Task UpdateAsync(UserNotificationDto notification);
    }
}

using Fe.DTOs.Notification;
using Fe.DTOs.Campaigns;

namespace Fe.Services.Notification
{
    public interface INotificationService
    {
        Task<IEnumerable<UserNotificationDto>> GetByAccountIdAsync(int accountId);
        Task<bool> SendToUserAsync(CreateNotificationDto notificationDto);
        Task<bool> SendBulkAsync(BulkNotificationDto notificationDto);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<List<UserNotificationDto>> GetHistoryAsync();
        Task<List<UserNotificationDto>> GetByCampaignAsync(int campaignId);
        Task<(IEnumerable<UserNotificationDto> notifications, int totalCount)> GetPaginatedNotificationsAsync(int pageNumber, int pageSize);
        Task<Fe.DTOs.Campaigns.CampaignDto?> GetLatestCampaignFromApi();
    }
}

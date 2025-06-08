using Be.DTOs.Notification;
using Be.Models;
using Be.Repositories.NotificationRepo;
using Be.Services.EmailService;
using Microsoft.Extensions.Logging;

namespace Be.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IEmailService _emailService;
        private const int MAX_NOTIFICATIONS_PER_USER = 100;//giới hạn 100 thông báo/người dùng
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            IEmailService emailService,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _emailService = emailService;
            _logger = logger;
        }
//Tự động xóa thông báo cũ khi vượt quá giới hạn
        private async Task CleanupOldNotifications(int accountId)
        {
            var notifications = await _notificationRepository.GetByAccountIdAsync(accountId);
            if (notifications.Count() > MAX_NOTIFICATIONS_PER_USER)
            {
                var oldNotifications = notifications
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip(MAX_NOTIFICATIONS_PER_USER);

                foreach (var notification in oldNotifications)
                {
                    _notificationRepository.Delete(notification);
                }
                await _notificationRepository.SaveChangesAsync();
            }
        }

        public async Task SendToUserAsync(CreateNotificationDto dto)
        {
            await CleanupOldNotifications(dto.AccountId);

            var notification = new UserNotification
            {
                AccountId = dto.AccountId,
                Title = dto.Title,
                Message = dto.Message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task SendToAllUsersAsync(BulkNotificationDto dto)
        {
            _logger.LogInformation("Starting to send bulk notification: {Title}", dto.Title);

            try
            {
                // Get all notifications to find subscribed users
                var notifications = await _notificationRepository.GetAllAsync();
                var subscribedUserIds = notifications
                    .Where(n => n.Title == "Subscription Confirmed")
                    .Select(n => n.AccountId)
                    .Distinct()
                    .ToList();

                _logger.LogInformation("Found {Count} subscribed users", subscribedUserIds.Count);

                if (!subscribedUserIds.Any())
                {
                    _logger.LogWarning("No subscribed users found");
                    throw new Exception("No subscribed users found");
                }

                // Get emails of only subscribed users
                var emails = await _notificationRepository.GetUserEmailsByIdsAsync(subscribedUserIds);
                _logger.LogInformation("Retrieved {Count} email addresses", emails.Count);

                // Create notifications for each user
                foreach (var userId in subscribedUserIds)
                {
                    var notification = new UserNotification
                    {
                        AccountId = userId,
                        Title = dto.Title,
                        Message = dto.Message,
                        CampaignId = dto.CampaignId,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };

                    await _notificationRepository.AddAsync(notification);
                }

                await _notificationRepository.SaveChangesAsync();
                _logger.LogInformation("Created notifications for all subscribed users");

                // Send emails
                await _emailService.SendBulkEmailsAsync(emails, dto.Title, dto.Message, dto.CampaignId);
                _logger.LogInformation("Sent emails to all subscribed users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk notification");
                throw;
            }
        }

        public async Task<IEnumerable<UserNotificationDto>> GetByAccountIdAsync(int accountId)
        {
            await CleanupOldNotifications(accountId);

            var notifications = await _notificationRepository.GetByAccountIdAsync(accountId);

            return notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(MAX_NOTIFICATIONS_PER_USER)
                .Select(n => new UserNotificationDto
                {
                    NotificationId = n.NotificationId,
                    AccountId = n.AccountId,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    CampaignId = n.CampaignId,
                    CampaignTitle = n.Campaign?.Title
                });
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            await _notificationRepository.MarkAsReadAsync(notificationId);
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            _logger.LogInformation("Backend Service: Attempting to delete notification {NotificationId}", notificationId);
            
            var notificationToDelete = await _notificationRepository.GetByIdAsync(notificationId);
            _logger.LogInformation("Backend Service: Found notification: {Found}", notificationToDelete != null);

            if (notificationToDelete == null)
            {
                _logger.LogWarning("Backend Service: Notification {NotificationId} not found", notificationId);
                return false;
            }

            _logger.LogInformation("Backend Service: Deleting notification {NotificationId}", notificationId);
            _notificationRepository.Delete(notificationToDelete);
            
            _logger.LogInformation("Backend Service: Saving changes for notification {NotificationId}", notificationId);
            await _notificationRepository.SaveChangesAsync();
            
            _logger.LogInformation("Backend Service: Successfully deleted notification {NotificationId}", notificationId);
            return true;
        }

        public async Task<(IEnumerable<UserNotificationDto> notifications, int totalCount)> GetPaginatedNotificationsAsync(int pageNumber, int pageSize)
        {
            var (notifications, totalCount) = await _notificationRepository.GetPaginatedAsync(pageNumber, pageSize);

            var notificationDtos = notifications.Select(n => new UserNotificationDto
            {
                NotificationId = n.NotificationId,
                AccountId = n.AccountId,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                CampaignId = n.CampaignId,
                CampaignTitle = n.Campaign?.Title
            });

            return (notificationDtos, totalCount);
        }

        public async Task<UserNotificationDto?> GetByIdAsync(int id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
            {
                return null;
            }

            return new UserNotificationDto
            {
                NotificationId = notification.NotificationId,
                AccountId = notification.AccountId,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                CampaignId = notification.CampaignId,
                CampaignTitle = notification.Campaign?.Title
            };
        }

        public async Task UpdateAsync(UserNotificationDto notificationDto)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationDto.NotificationId);
            if (notification != null)
            {
                notification.IsRead = notificationDto.IsRead;
                await _notificationRepository.SaveChangesAsync();
            }
        }
    }
}

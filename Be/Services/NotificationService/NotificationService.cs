using Be.DTOs.Notification;
using Be.Models;
using Be.Repositories.NotificationRepo;
using Be.Services.EmailService;

namespace Be.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IEmailService _emailService;
        private const int MAX_NOTIFICATIONS_PER_USER = 100;

        public NotificationService(
            INotificationRepository notificationRepository,
            IEmailService emailService)
        {
            _notificationRepository = notificationRepository;
            _emailService = emailService;
        }

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
            // Kiểm tra và xóa thông báo cũ nếu cần
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
            var emails = await _notificationRepository.GetAllUserEmailsAsync();
            foreach (var email in emails)
            {
                await _emailService.SendAsync(email, dto.Title, dto.Message);
            }
        }

        public async Task<IEnumerable<UserNotificationDto>> GetByAccountIdAsync(int accountId)
        {
            // Kiểm tra và xóa thông báo cũ nếu cần
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
                    CreatedAt = n.CreatedAt
                });
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            await _notificationRepository.MarkAsReadAsync(notificationId);
            await _notificationRepository.SaveChangesAsync();
        }
    }
}

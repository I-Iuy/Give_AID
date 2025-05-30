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

        public NotificationService(
            INotificationRepository notificationRepository,
            IEmailService emailService)
        {
            _notificationRepository = notificationRepository;
            _emailService = emailService;
        }

        public async Task SendToUserAsync(CreateNotificationDto dto)
        {
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
            var notifications = await _notificationRepository.GetByAccountIdAsync(accountId);

            return notifications.Select(n => new UserNotificationDto
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

using Be.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Be.Repositories.NotificationRepo
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<NotificationRepository> _logger;

        public NotificationRepository(DatabaseContext context, ILogger<NotificationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<UserNotification>> GetByAccountIdAsync(int accountId)
        {
            return await _context.UserNotifications
                .Where(n => n.AccountId == accountId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserNotification?> GetByIdAsync(int id)
        {
            return await _context.UserNotifications.FindAsync(id);
        }

        public async Task AddAsync(UserNotification entity)
        {
            await _context.UserNotifications.AddAsync(entity);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var notification = await _context.UserNotifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<string>> GetAllUserEmailsAsync()
        {
            var subscribers = await _context.Accounts
                .Where(a => !string.IsNullOrEmpty(a.Email))
                .Where(a => _context.UserNotifications
                    .Any(n => n.AccountId == a.AccountId && n.Title == "Subscription Confirmed"))
                .Select(a => a.Email)
                .ToListAsync();

            return subscribers;
        }

        public void Delete(UserNotification notification)
        {
            _context.UserNotifications.Remove(notification);
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            _logger.LogInformation("Backend Repository: Attempting to find notification {NotificationId}", notificationId);
            var notification = await _context.UserNotifications.FindAsync(notificationId);
            _logger.LogInformation("Backend Repository: Found notification: {Found}", notification != null);

            if (notification != null)
            {
                _logger.LogInformation("Backend Repository: Removing notification {NotificationId}", notificationId);
                _context.UserNotifications.Remove(notification);
                
                _logger.LogInformation("Backend Repository: Saving changes for notification {NotificationId}", notificationId);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Backend Repository: Successfully deleted notification {NotificationId}", notificationId);
                return true;
            }
            
            _logger.LogWarning("Backend Repository: Notification {NotificationId} not found", notificationId);
            return false;
        }

        public async Task<(IEnumerable<UserNotification> notifications, int totalCount)> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            var totalCount = await _context.UserNotifications.CountAsync();
            var notifications = await _context.UserNotifications
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (notifications, totalCount);
        }

        public async Task<IEnumerable<UserNotification>> GetAllAsync()
        {
            return await _context.UserNotifications.ToListAsync();
        }

        public async Task<List<string>> GetUserEmailsByIdsAsync(List<int> accountIds)
        {
            return await _context.Accounts
                .Where(a => accountIds.Contains(a.AccountId) && !string.IsNullOrEmpty(a.Email))
                .Select(a => a.Email!)
                .ToListAsync();
        }
    }
}

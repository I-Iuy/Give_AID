using Be.Models;
using Microsoft.EntityFrameworkCore;

namespace Be.Repositories.NotificationRepo
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly DatabaseContext _context;

        public NotificationRepository(DatabaseContext context)
        {
            _context = context;
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
            return await _context.Accounts
                .Where(a => !string.IsNullOrEmpty(a.Email))
                .Select(a => a.Email!)
                .ToListAsync();
        }

        public void Delete(UserNotification notification)
        {
            _context.UserNotifications.Remove(notification);
        }
    }
}

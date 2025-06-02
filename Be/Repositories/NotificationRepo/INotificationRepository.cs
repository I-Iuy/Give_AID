using Be.Models;

namespace Be.Repositories.NotificationRepo
{
    public interface INotificationRepository
    {
        Task<IEnumerable<UserNotification>> GetByAccountIdAsync(int accountId);
        Task<UserNotification?> GetByIdAsync(int id);
        Task AddAsync(UserNotification entity);
        Task MarkAsReadAsync(int id);
        Task SaveChangesAsync();
        Task<List<string>> GetAllUserEmailsAsync();
        void Delete(UserNotification notification);
    }
}

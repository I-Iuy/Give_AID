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
        Task<bool> DeleteNotificationAsync(int notificationId);
        Task<(IEnumerable<UserNotification> notifications, int totalCount)> GetPaginatedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<UserNotification>> GetAllAsync();
        Task<List<string>> GetUserEmailsByIdsAsync(List<int> accountIds);
    }
}

using Be.Models;

namespace Be.Repositories.ShareRepo
{
    public interface IShareRepository
    {
        Task<IEnumerable<Share>> GetAllAsync();
        Task<Share?> GetByIdAsync(int id);
        Task AddAsync(Share entity);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
        Task<IEnumerable<Share>> GetRecentSharesAsync(int? accountId, string? guestName);
    }
}

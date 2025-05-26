using Be.Models;

namespace Be.Repositories.Campaigns
{
    public interface ICampaignRepository
    {
        Task<IEnumerable<Campaign>> GetAllAsync();
        Task<Campaign> GetByIdAsync(int id);
        Task AddAsync(Campaign campaign);
        Task EditAsync(Campaign campaign);
        Task DeleteAsync(int id);
    }
}
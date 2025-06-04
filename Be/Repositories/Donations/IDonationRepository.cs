using Be.Models;

namespace Be.Repositories.Donations
{
    public interface IDonationRepository
    {
        Task<IEnumerable<Donation>> GetAllAsync();
        Task<Donation> GetByIdAsync(int id);
        Task AddAsync(Donation donation);
    }
}

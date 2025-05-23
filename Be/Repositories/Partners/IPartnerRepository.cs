using Be.Models;

namespace Be.Repositories.Partners
{
    public interface IPartnerRepository
    {
        Task<IEnumerable<Partner>> GetAllAsync();
        Task<Partner> GetByIdAsync(int id);
        Task AddAsync(Partner partner);
        Task EditAsync(Partner partner);
        Task DeleteAsync(int id);
    }
}

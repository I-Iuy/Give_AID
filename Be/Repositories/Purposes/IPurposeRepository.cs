using Be.Models;

namespace Be.Repositories.Purposes
{
    public interface IPurposeRepository
    {
        Task<IEnumerable<Purpose>> GetAllAsync();
        Task<Purpose> GetByIdAsync(int id);
        Task AddAsync(Purpose purpose);
        Task EditAsync(Purpose purpose);
        Task DeleteAsync(int id);
    }
}

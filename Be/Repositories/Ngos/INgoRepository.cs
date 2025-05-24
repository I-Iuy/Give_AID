using Be.Models;

namespace Be.Repositories.Ngos
{
    public interface INgoRepository
    {
       Task<IEnumerable<Ngo>> GetAllAsync();
        Task<Ngo> GetByIdAsync(int id);
        Task AddAsync(Ngo ngo);
        Task EditAsync(Ngo ngo);
        Task DeleteAsync(int id);
    }
}

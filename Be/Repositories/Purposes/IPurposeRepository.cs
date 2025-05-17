using Be.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Be.Repositories.Purposes
{
    public interface IPurposeRepository
    {
        Task<IEnumerable<Purpose>> GetAllAsync();
        Task AddAsync(Purpose purpose);
        Task DeleteAsync(int id);
    }
}

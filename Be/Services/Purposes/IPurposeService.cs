using Be.DTOs.Purposes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Be.Services.Purposes
{
    public interface IPurposeService
    {
        Task<IEnumerable<PurposeDto>> GetAllAsync();
        Task AddAsync(CreatePurposeDto dto);
        Task DeleteAsync(int id);
    }
}

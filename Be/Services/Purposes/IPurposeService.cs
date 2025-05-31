using Be.DTOs.Purposes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Be.Services.Purposes
{
    public interface IPurposeService
    {
        Task<IEnumerable<PurposeDto>> GetAllAsync();
        Task<PurposeDto> GetByIdAsync(int id);
        Task AddAsync(CreatePurposeDto dto);
        Task EditAsync(UpdatePurposeDto dto);
        Task DeleteAsync(int id);
    }
}

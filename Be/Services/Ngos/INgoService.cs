using Be.Dtos.Ngos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Be.Services.Ngos
{
    public interface INgoService
    {
        Task<IEnumerable<NgoDto>> GetAllAsync();
        Task<NgoDto> GetByIdAsync(int id);
        Task AddAsync(CreateNgoDto dto);
        Task EditAsync(UpdateNgoDto dto);
        Task DeleteAsync(int id);
    }
}

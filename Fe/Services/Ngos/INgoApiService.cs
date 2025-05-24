using Fe.Dtos.Ngos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fe.Services.Ngos
{
    public interface INgoApiService
    {
        Task<IEnumerable<NgoDto>> GetAllAsync();
        Task<NgoDto> GetByIdAsync(int id);
        FileStream GetLogoFileStream(string logoUrl);
        Task AddAsync(CreateNgoDto dto, IFormFile logo);
        Task EditAsync(UpdateNgoDto dto, IFormFile logo);
        Task DeleteAsync(int id);
    }
}

using Fe.DTOs.Purposes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fe.Services.Purposes
{
    public interface IPurposeApiService
    {
        Task<IEnumerable<PurposeDto>> GetAllAsync();
        Task CreateAsync(CreatePurposeDto dto);
        Task DeleteAsync(int id);
    }
}

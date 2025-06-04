using Fe.DTOs.Purposes;

namespace Fe.Services.Purposes
{
    public interface IPurposeApiService
    {
        Task<IEnumerable<PurposeDto>> GetAllAsync();
        Task<PurposeDto> GetByIdAsync(int id);
        Task CreateAsync(CreatePurposeDto dto);
        Task<bool> CheckInUseAsync(int id);
        Task EditAsync(UpdatePurposeDto dto);
        Task DeleteAsync(int id);
    }
}

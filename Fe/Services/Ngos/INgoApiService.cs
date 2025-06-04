using Fe.Dtos.Ngos;

namespace Fe.Services.Ngos
{
    public interface INgoApiService
    {
        Task<IEnumerable<NgoDto>> GetAllAsync();
        Task<NgoDto> GetByIdAsync(int id);
        FileStream GetLogoFileStream(string logoUrl);
        Task AddAsync(CreateNgoDto dto, IFormFile logo);
        Task<bool> CheckInUseAsync(int id);
        Task EditAsync(UpdateNgoDto dto, IFormFile logo);
        Task DeleteAsync(int id);
    }
}

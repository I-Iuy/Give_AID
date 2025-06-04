using Be.DTOs.Share;

namespace Be.Services.ShareService
{
    public interface IShareService
    {
        Task<ShareDto> ShareAsync(CreateShareDto dto, string baseUrl);
        Task<IEnumerable<ShareDto>> GetAllAsync();
        Task<ShareDto?> GetByIdAsync(int id);
    }
}

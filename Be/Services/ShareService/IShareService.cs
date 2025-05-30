using Be.DTOs.Share;

namespace Be.Services.ShareService
{
    public interface IShareService
    {
        Task<ShareDto> ShareAsync(CreateShareDto dto);
        Task<IEnumerable<ShareDto>> GetAllAsync();
        Task<ShareDto?> GetByIdAsync(int id);
    }
}

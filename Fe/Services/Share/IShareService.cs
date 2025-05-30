using Fe.DTOs.Share;

namespace Fe.Services.Share
{
    public interface IShareService
    {
        Task<IEnumerable<ShareDto>> GetAllAsync();
        Task<ShareDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(CreateShareDto dto);
    }
}

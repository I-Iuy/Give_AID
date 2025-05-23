using Be.DTOs.Partners;

namespace Be.Services.Partners
{
    public interface IPartnerService
    {
        Task<IEnumerable<PartnerDto>> GetAllAsync();
        Task<PartnerDto> GetByIdAsync(int id);
        Task AddAsync(CreatePartnerDto dto);
        Task EditAsync(UpdatePartnerDto dto);
        Task DeleteAsync(int id);

    }
}

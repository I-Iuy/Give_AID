using Be.DTOs.Donations;

namespace Be.Services.Donations
{
    public interface IDonationService
    {
        Task<IEnumerable<DonationDto>> GetAllAsync();
        Task<DonationDto> GetByIdAsync(int id);
        Task AddAsync(CreateDonationDto dto);
    }
}

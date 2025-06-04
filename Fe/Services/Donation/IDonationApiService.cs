using Fe.DTOs.Donations;

namespace Fe.Services.Donations
{
    public interface IDonationApiService
    {
        Task<List<DonationDto>> GetAllAsync();
        Task<DonationDto?> GetByIdAsync(int id);
        Task Add(CreateDonationDto dto);
    }
}

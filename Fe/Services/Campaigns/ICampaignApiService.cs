using Fe.DTOs.Campaigns;

namespace Fe.Services.Campaigns
{
    public interface ICampaignApiService
    {
        Task<IEnumerable<CampaignDto>> GetAllAsync();
        Task<CampaignDto> GetByIdAsync(int id);
        Task AddAsync(CreateCampaignDto dto);
        Task EditAsync(UpdateCampaignDto dto);
        Task DeleteAsync(int id);
    }
}

using Be.DTOs.Campaigns;

namespace Be.Services.Campaigns
{
    public interface ICampaignService
    {
        Task<IEnumerable<CampaignDto>> GetAllAsync();
        Task<CampaignDto> GetByIdAsync(int id);
        Task AddAsync(CreateCampaignDto dto);
        Task EditAsync(UpdateCampaignDto dto);
        Task DeleteAsync(int id);
    }
}

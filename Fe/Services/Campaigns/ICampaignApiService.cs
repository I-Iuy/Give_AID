using Fe.Dtos.Ngos;
using Fe.DTOs.Campaigns;
using Fe.DTOs.Partners;
using Fe.DTOs.Purposes;

namespace Fe.Services.Campaigns
{
    public interface ICampaignApiService
    {
        Task<IEnumerable<CampaignDto>> GetAllAsync();
        Task<CampaignDto> GetByIdAsync(int id);
        Task AddAsync(CreateCampaignDto dto);
        //Task EditAsync(UpdateCampaignDto dto);
        //Task DeleteAsync(int id);
    }
}

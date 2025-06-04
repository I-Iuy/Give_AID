using Fe.Dtos.Ngos;
using Fe.DTOs.Campaigns;
using Fe.DTOs.Partners;
using Fe.DTOs.Purposes;

namespace Fe.Services.Getdata
{
    public interface IGetdataApiService
    {
        Task<List<PartnerDto>> GetAllPartnersAsync();
        Task<List<NgoDto>> GetAllNgosAsync();
        Task<List<PurposeDto>> GetAllPurposesAsync();
        Task<List<CampaignDto>> GetAllCampaignsAsync();
    }
}

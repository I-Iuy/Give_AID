using Be.Repositories.CampaignsUsage;

namespace Be.Services.CampaignUsage
{
    public class CampaignUsageService : ICampaignUsageService
    {
        private readonly ICampaignUsageRepository _repo;

        public CampaignUsageService(ICampaignUsageRepository repo)
        {
            _repo = repo;
        }
        // Check if Purpose, Partner, or NGO is used in any Campaign
        public Task<bool> IsPurposeUsedAsync(int purposeId)
        {
            return _repo.IsPurposeUsedAsync(purposeId);
        }
        public Task<bool> IsPartnerUsedAsync(int partnerId)
        {
            return _repo.IsPartnerUsedAsync(partnerId);
        }
        public Task<bool> IsNgoUsedAsync(int ngoId)
        {
            return _repo.IsNgoUsedAsync(ngoId);
        }
    }
}


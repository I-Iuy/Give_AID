using Be.Models;
using Microsoft.EntityFrameworkCore;

namespace Be.Repositories.CampaignsUsage
{
    public class CampaignUsageRepository : ICampaignUsageRepository
    {
        private readonly DatabaseContext _context;

        public CampaignUsageRepository(DatabaseContext context)
        {
            _context = context;
        }

        // Check if Purpose, Partner, or Ngo is used in any Campaign
        public async Task<bool> IsPurposeUsedAsync(int purposeId)
        {
            return await _context.Campaigns.AnyAsync(c => c.PurposeId == purposeId);
        }
        public async Task<bool> IsPartnerUsedAsync(int partnerId)
        {
            return await _context.CampaignPartners.AnyAsync(cp => cp.PartnerId == partnerId);
        }
        public async Task<bool> IsNgoUsedAsync(int ngoId)
        {
            return await _context.CampaignNgos.AnyAsync(cn => cn.NgoId == ngoId);
        }
    }

}

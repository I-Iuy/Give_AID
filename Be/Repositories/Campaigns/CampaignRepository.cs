using Be.Models;
using Microsoft.EntityFrameworkCore;

namespace Be.Repositories.Campaigns
{
    public class CampaignRepository : ICampaignRepository
    {
        private readonly DatabaseContext _context;

        public CampaignRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Campaign>> GetAllAsync()
        {
            return await _context.Campaigns.ToListAsync();
        }

        public async Task<Campaign> GetByIdAsync(int id)
        {
            return await _context.Campaigns.FindAsync(id);
        }

        public async Task AddAsync(Campaign campaign)
        {
            await _context.Campaigns.AddAsync(campaign);
            await _context.SaveChangesAsync();
        }

        public async Task EditAsync(Campaign campaign)
        {
            _context.Campaigns.Update(campaign);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign != null)
            {
                _context.Campaigns.Remove(campaign);
                await _context.SaveChangesAsync();
            }
        }
    }
}

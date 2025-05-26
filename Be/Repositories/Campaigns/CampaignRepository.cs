using Be.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Be.Repositories.Campaigns
{
    public class CampaignRepository : ICampaignRepository
    {
        private readonly DatabaseContext _context;

        public CampaignRepository(DatabaseContext context)
        {
            _context = context;
        }

        // Lấy danh sách tất cả Campaign
        public async Task<IEnumerable<Campaign>> GetAllAsync()
        {
            return await _context.Campaigns.ToListAsync();
        }

        // Lấy Campaign theo ID
        public async Task<Campaign> GetByIdAsync(int id)
        {
            return await _context.Campaigns.FindAsync(id);
        }

        // Thêm mới Campaign
        public async Task AddAsync(Campaign campaign)
        {
            await _context.Campaigns.AddAsync(campaign);
            await _context.SaveChangesAsync();
        }

        // Cập nhật Campaign
        public async Task EditAsync(Campaign campaign)
        {
            _context.Campaigns.Update(campaign);
            await _context.SaveChangesAsync();
        }

        // Xoá Campaign theo ID
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

using Be.Models;
using Microsoft.EntityFrameworkCore;

namespace Be.Repositories.ShareRepo
{
    //=========================
    // SHARE REPOSITORY - HANDLE SHARE DATA OPERATIONS
    public class ShareRepository : IShareRepository
    {
        private readonly DatabaseContext _context;

        public ShareRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Share>> GetAllAsync()
        {
            return await _context.Shares
                .Include(s => s.Account)
                .Include(s => s.Campaign)
                .ToListAsync();
        }

        public async Task<Share?> GetByIdAsync(int id)
        {
            return await _context.Shares
                .Include(s => s.Account)
                .Include(s => s.Campaign)
                .FirstOrDefaultAsync(s => s.ShareId == id);
        }

        public async Task AddAsync(Share entity)
        {
            await _context.Shares.AddAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Shares.FindAsync(id);
            if (entity != null)
            {
                _context.Shares.Remove(entity);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Share>> GetRecentSharesAsync(int? accountId, string? guestName)
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);

            return await _context.Shares
                .Where(s =>
                    (accountId.HasValue && s.AccountId == accountId) ||
                    (!string.IsNullOrEmpty(guestName) && s.GuestName == guestName))
                .Where(s => s.SharedAt >= oneHourAgo)
                .ToListAsync();
        }
    }
    //=========================
}
using Be.Models;
using Microsoft.EntityFrameworkCore;

namespace Be.Repositories.Donations
{
    public class DonationRepository : IDonationRepository
    {
        private readonly DatabaseContext _context;

        public DonationRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Donation>> GetAllAsync()
        {
            return await _context.Donations.ToListAsync();
        }

        public async Task<Donation> GetByIdAsync(int id)
        {
            return await _context.Donations.FindAsync(id);
        }

        public async Task AddAsync(Donation donation)
        {
            await _context.Donations.AddAsync(donation);
            await _context.SaveChangesAsync();
        }
    }
}

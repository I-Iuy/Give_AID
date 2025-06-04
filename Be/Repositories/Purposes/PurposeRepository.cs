using Be.Models;
using Microsoft.EntityFrameworkCore;

namespace Be.Repositories.Purposes
{
    public class PurposeRepository : IPurposeRepository
    {
        private readonly DatabaseContext _context;

        public PurposeRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Purpose>> GetAllAsync()
        {
            return await _context.Purposes.ToListAsync();
        }

        public async Task<Purpose> GetByIdAsync(int id)
        {
            return await _context.Purposes.FindAsync(id);
        }

        public async Task AddAsync(Purpose purpose)
        {
            await _context.Purposes.AddAsync(purpose);
            await _context.SaveChangesAsync();
        }

        public async Task EditAsync(Purpose purpose)
        {
            _context.Purposes.Update(purpose);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var purpose = await _context.Purposes.FindAsync(id);
            if (purpose != null)
            {
                _context.Purposes.Remove(purpose);
                await _context.SaveChangesAsync();
            }
        }
    }
}

using Be.Models;
using Microsoft.EntityFrameworkCore;

namespace Be.Repositories.Ngos
{
    public class NgoRepository : INgoRepository
    {
        private readonly DatabaseContext _context;

        public NgoRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ngo>> GetAllAsync()
        {
            return await _context.Ngos.ToListAsync();
        }

        public async Task<Ngo> GetByIdAsync(int id)
        {
            return await _context.Ngos.FindAsync(id);
        }

        public async Task AddAsync(Ngo ngo)
        {
            await _context.Ngos.AddAsync(ngo);
            await _context.SaveChangesAsync();
        }

        public async Task EditAsync(Ngo ngo)
        {
            _context.Ngos.Update(ngo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var ngo = await _context.Ngos.FindAsync(id);
            if (ngo != null)
            {
                _context.Ngos.Remove(ngo);
                await _context.SaveChangesAsync();
            }
        }
    }
}

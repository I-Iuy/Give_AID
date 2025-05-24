using Be.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Be.Repositories.Ngos
{
    public class NgoRepository : INgoRepository
    {
        private readonly DatabaseContext _context;

        public NgoRepository(DatabaseContext context)
        {
            _context = context;
        }

        // Lấy danh sách tất cả NGO
        public async Task<IEnumerable<Ngo>> GetAllAsync()
        {
            return await _context.Ngos.ToListAsync();
        }

        // Lấy NGO theo ID
        public async Task<Ngo> GetByIdAsync(int id)
        {
            return await _context.Ngos.FindAsync(id);
        }

        // Thêm mới NGO
        public async Task AddAsync(Ngo ngo)
        {
            await _context.Ngos.AddAsync(ngo);
            await _context.SaveChangesAsync();
        }

        // Cập nhật NGO
        public async Task EditAsync(Ngo ngo)
        {
            _context.Ngos.Update(ngo);
            await _context.SaveChangesAsync();
        }

        // Xoá NGO theo ID
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

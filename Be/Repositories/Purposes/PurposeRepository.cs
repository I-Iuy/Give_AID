using Be.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Be.Repositories.Purposes
{
    public class PurposeRepository : IPurposeRepository
    {
        private readonly DatabaseContext _context;

        public PurposeRepository(DatabaseContext context)
        {
            _context = context;
        }

        // Lấy danh sách tất cả Purpose
        public async Task<IEnumerable<Purpose>> GetAllAsync()
        {
            return await _context.Purposes.ToListAsync();
        }

        // Thêm mới Purpose
        public async Task AddAsync(Purpose purpose)
        {
            await _context.Purposes.AddAsync(purpose);
            await _context.SaveChangesAsync();
        }

        // Xoá Purpose theo ID
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

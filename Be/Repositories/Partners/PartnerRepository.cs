using Be.Models;
using Microsoft.EntityFrameworkCore;

namespace Be.Repositories.Partners
{
    public class PartnerRepository : IPartnerRepository
    {
        private readonly DatabaseContext _context;

        public PartnerRepository(DatabaseContext context)
        {
            _context = context;
        }

        // Lấy danh sách tất cả Partner
        public async Task<IEnumerable<Partner>> GetAllAsync()
        {
            return await _context.Partners.ToListAsync();
        }

        // Lấy Partner theo ID
        public async Task<Partner> GetByIdAsync(int id)
        {
            return await _context.Partners.FindAsync(id);
        }

        // Thêm mới Partner
        public async Task AddAsync(Partner partner)
        {
            await _context.Partners.AddAsync(partner);
            await _context.SaveChangesAsync();
        }

        // Cập nhật Partner
        public async Task EditAsync(Partner partner)
        {
            _context.Partners.Update(partner);
            await _context.SaveChangesAsync();
        }

        // Xoá Partner theo ID
        public async Task DeleteAsync(int id)
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner != null)
            {
                _context.Partners.Remove(partner);
                await _context.SaveChangesAsync();
            }
        }
    }
}

using Be.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Be.Repositories.ContentPageRepositories // ✅ Đảm bảo khớp với interface
{
    public class ContentPageRepository : IContentPageRepository
    {
        private readonly DatabaseContext _context;

        public ContentPageRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<ContentPage>> GetAllAsync()
        {
            return await _context.ContentPages
                .Include(cp => cp.Account) // Include Account nếu cần email người tạo
                .ToListAsync();
        }

        public async Task<ContentPage?> GetByIdAsync(int id)
        {
            return await _context.ContentPages
                .Include(cp => cp.Account)
                .FirstOrDefaultAsync(cp => cp.PageId == id);
        }

        public async Task<ContentPage> CreateAsync(ContentPage contentPage)
        {
            _context.ContentPages.Add(contentPage);
            await _context.SaveChangesAsync();
            return contentPage;
        }

        public async Task<ContentPage?> UpdateAsync(ContentPage contentPage)
        {
            var existing = await _context.ContentPages.FindAsync(contentPage.PageId);
            if (existing == null)
                return null;

            _context.Entry(existing).CurrentValues.SetValues(contentPage);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var contentPage = await _context.ContentPages.FindAsync(id);
            if (contentPage == null)
                return false;

            _context.ContentPages.Remove(contentPage);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

using Be.Models;
using Be.Repositories.ContentPages;
using Microsoft.EntityFrameworkCore;

namespace Be.Repositories.ContentPages
{
    public class ContentPageRepository : IContentPageRepository
    {
        private readonly DatabaseContext _context;

        // Constructor: injects the EF database context
        public ContentPageRepository(DatabaseContext context)
        {
            _context = context;
        }

        // Retrieves all content pages from the database
        public async Task<List<ContentPage>> GetAllAsync()
        {
            return await _context.ContentPages.ToListAsync();
        }

        // Retrieves a single content page by its ID
        public async Task<ContentPage?> GetByIdAsync(int id)
        {
            return await _context.ContentPages.FindAsync(id);
        }

        // Retrieves a content page by its slug (used for frontend routing)
        public async Task<ContentPage?> GetBySlugAsync(string slug)
        {
            return await _context.ContentPages.FirstOrDefaultAsync(x => x.Slug == slug);
        }

        // Adds a new content page to the database
        public async Task AddAsync(ContentPage page)
        {
            _context.ContentPages.Add(page);
            await _context.SaveChangesAsync();
        }

        // Updates an existing content page
        public async Task UpdateAsync(ContentPage page)
        {
            _context.ContentPages.Update(page);
            await _context.SaveChangesAsync();
        }

        // Deletes a content page by its ID
        public async Task DeleteAsync(int id)
        {
            var page = await GetByIdAsync(id);
            if (page != null)
            {
                _context.ContentPages.Remove(page);
                await _context.SaveChangesAsync();
            }
        }
    }
}

using Be.Models;

namespace Be.Repositories.ContentPages
{
    public interface IContentPageRepository
    {
        // Get all content pages from the database
        Task<List<ContentPage>> GetAllAsync();

        // Get a content page by its ID (primary key)
        Task<ContentPage?> GetByIdAsync(int id);

        // Get a content page by its URL-friendly slug
        Task<ContentPage?> GetBySlugAsync(string slug);

        // Add a new content page
        Task AddAsync(ContentPage page);

        // Update an existing content page
        Task UpdateAsync(ContentPage page);

        // Delete a content page by its ID
        Task DeleteAsync(int id);
    }
}

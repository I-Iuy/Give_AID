using Be.DTOs.ContentPages;

namespace Be.Services.ContentPages
{
    public interface IContentPageService
    {
        // Get all content pages as DTO list
        Task<List<ContentPageDto>> GetAllAsync();

        // Get a single content page by its ID
        Task<ContentPageDto?> GetByIdAsync(int id);

        // Get a single content page by its slug (for URL-based lookup)
        Task<ContentPageDto?> GetBySlugAsync(string slug);

        // Add a new content page
        Task AddAsync(ContentPageDto dto);

        // Update an existing content page
        Task UpdateAsync(ContentPageDto dto);

        // Delete a content page by ID
        Task DeleteAsync(int id);
    }
}

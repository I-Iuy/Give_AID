namespace Fe.Services.ContentPages
{
    using Fe.DTOs.ContentPages;

    // Defines the contract for interacting with ContentPage API
    public interface IContentPageApiService
    {
        // Retrieves all content pages
        Task<List<ContentPageDto>> GetAllAsync();

        // Retrieves a content page by its ID
        Task<ContentPageDto?> GetByIdAsync(int id);

        // Retrieves a content page by its slug (URL-friendly identifier)
        Task<ContentPageDto?> GetBySlugAsync(string slug);

        // Creates a new content page
        Task AddAsync(ContentPageDto dto);

        // Updates an existing content page
        Task UpdateAsync(ContentPageDto dto);

        // Deletes a content page by its ID
        Task DeleteAsync(int id);
    }
}

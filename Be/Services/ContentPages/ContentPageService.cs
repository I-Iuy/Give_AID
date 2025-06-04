using Be.DTOs.ContentPages;
using Be.Models;
using Be.Repositories.ContentPages;

namespace Be.Services.ContentPages
{
    public class ContentPageService : IContentPageService
    {
        private readonly IContentPageRepository _repo;

        // Constructor: injects repository dependency
        public ContentPageService(IContentPageRepository repo)
        {
            _repo = repo;
        }

        // Get all content pages and map to DTOs
        public async Task<List<ContentPageDto>> GetAllAsync()
        {
            var pages = await _repo.GetAllAsync();
            return pages.Select(p => new ContentPageDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Content = p.Content,
                Author = p.Author,
                UpdatedAt = p.UpdatedAt
            }).ToList();
        }

        // Get a content page by its ID and map to DTO
        public async Task<ContentPageDto?> GetByIdAsync(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            if (p == null) return null;

            return new ContentPageDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Content = p.Content,
                Author = p.Author,
                UpdatedAt = p.UpdatedAt
            };
        }

        // Get a content page by its slug and map to DTO
        public async Task<ContentPageDto?> GetBySlugAsync(string slug)
        {
            var p = await _repo.GetBySlugAsync(slug);
            if (p == null) return null;

            return new ContentPageDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Content = p.Content,
                Author = p.Author,
                UpdatedAt = p.UpdatedAt
            };
        }

        // Add a new content page to the database
        public async Task AddAsync(ContentPageDto dto)
        {
            var page = new ContentPage
            {
                Title = dto.Title,
                Slug = dto.Slug,
                Content = dto.Content,
                Author = dto.Author,
                UpdatedAt = dto.UpdatedAt
            };

            await _repo.AddAsync(page);
        }

        // Update an existing content page
        public async Task UpdateAsync(ContentPageDto dto)
        {
            var page = await _repo.GetByIdAsync(dto.Id);
            if (page == null) return;

            page.Title = dto.Title;
            page.Slug = dto.Slug;
            page.Content = dto.Content;
            page.Author = dto.Author;
            page.UpdatedAt = dto.UpdatedAt;

            await _repo.UpdateAsync(page);
        }

        // Delete a content page by its ID
        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}

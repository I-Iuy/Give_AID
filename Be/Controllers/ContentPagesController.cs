using Be.DTOs.ContentPage;
using Be.Models;
using Be.Repositories.ContentPageRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Be.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentPagesController : ControllerBase
    {
        private readonly IContentPageRepository _repository;

        public ContentPagesController(IContentPageRepository repository)
        {
            _repository = repository;
        }

        // GET: api/contentpages
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var pages = await _repository.GetAllAsync();
            var result = pages.Select(p => new ContentPageGetDto
            {
                PageId = p.PageId,
                Title = p.Title,
                Content = p.Content,
                AuthorEmail = p.Account?.Email ?? ""
            });
            return Ok(result);
        }

        // GET: api/contentpages/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var page = await _repository.GetByIdAsync(id);
            if (page == null) return NotFound();

            var dto = new ContentPageGetDto
            {
                PageId = page.PageId,
                Title = page.Title,
                Content = page.Content,
                AuthorEmail = page.Account?.Email ?? ""
            };

            return Ok(dto);
        }

        // POST: api/contentpages
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ContentPageCreateDto dto)
        {
            var contentPage = new ContentPage
            {
                Title = dto.Title,
                Content = dto.Content,
                AccountId = dto.AccountId
            };

            var result = await _repository.CreateAsync(contentPage);
            return Ok(new { message = "Content page created successfully.", result.PageId });
        }

        // PUT: api/contentpages/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ContentPageCreateDto dto)
        {
            var contentPage = new ContentPage
            {
                PageId = id,
                Title = dto.Title,
                Content = dto.Content,
                AccountId = dto.AccountId
            };

            var updated = await _repository.UpdateAsync(contentPage);
            if (updated == null) return NotFound();

            return Ok(new { message = "Content page updated successfully." });
        }

        // DELETE: api/contentpages/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted) return NotFound();

            return Ok(new { message = "Content page deleted successfully." });
        }
    }
}

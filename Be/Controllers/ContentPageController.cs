using Be.DTOs.ContentPages;
using Be.Services.ContentPages;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class ContentPageController : ControllerBase
{
    private readonly IContentPageService _service;

    // Injecting the content page service via constructor
    public ContentPageController(IContentPageService service)
    {
        _service = service;
    }

    // GET: api/ContentPage
    // Retrieves all content pages
    [HttpGet]
    public async Task<ActionResult<List<ContentPageDto>>> GetAll()
    {
        return await _service.GetAllAsync();
    }

    // GET: api/ContentPage/5
    // Retrieves a specific content page by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<ContentPageDto>> GetById(int id)
    {
        var page = await _service.GetByIdAsync(id);
        return page == null ? NotFound() : Ok(page);
    }

    // GET: api/ContentPage/slug/about-us
    // Retrieves a content page by its slug (used for frontend routing)
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ContentPageDto>> GetBySlug(string slug)
    {
        var page = await _service.GetBySlugAsync(slug);
        return page == null ? NotFound() : Ok(page);
    }

    // POST: api/ContentPage
    // Creates a new content page
    [HttpPost]
    public async Task<ActionResult> Create(ContentPageDto dto)
    {
        await _service.AddAsync(dto);
        return Ok("Page added");
    }

    // PUT: api/ContentPage/5
    // Updates an existing content page (ID must match DTO.Id)
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, ContentPageDto dto)
    {
        if (id != dto.Id) return BadRequest(); // Prevent mismatch between route and body
        await _service.UpdateAsync(dto);
        return Ok("Page updated");
    }

    // DELETE: api/ContentPage/5
    // Deletes a content page by ID
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok("Page deleted");
    }
}

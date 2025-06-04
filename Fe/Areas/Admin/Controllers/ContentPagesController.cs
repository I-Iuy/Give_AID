using Fe.DTOs.ContentPages;
using Fe.Services.ContentPages;
using Microsoft.AspNetCore.Mvc;

namespace Fe.Areas.Admin.Controllers;

// Controller in the Admin area for managing static content pages (e.g., About, Mission, etc.)
[Area("Admin")]
public class ContentPagesController : Controller
{
    // Injected service for API calls related to content pages
    private readonly IContentPageApiService _contentPageService;

    // Constructor with dependency injection
    public ContentPagesController(IContentPageApiService contentPageService)
    {
        _contentPageService = contentPageService;
    }

    // GET: Admin/ContentPages/List
    // Display all existing content pages in a list view
    public async Task<IActionResult> List()
    {
        var pages = await _contentPageService.GetAllAsync();
        return View(pages);
    }

    // GET: Admin/ContentPages/Add
    // Render form to add a new content page
    public IActionResult Add()
    {
        return View();
    }

    // POST: Admin/ContentPages/Add
    // Submit and save new content page to API
    [HttpPost]
    public async Task<IActionResult> Add(ContentPageDto dto)
    {
        // Set default Author (can later be replaced with logged-in user)
        dto.Author = "Admin";

        // Set the current time as the last updated timestamp
        dto.UpdatedAt = DateTime.Now;

        // Generate slug from title (simple hyphenation)
        dto.Slug = dto.Title.ToLower().Replace(" ", "-");

        await _contentPageService.AddAsync(dto);
        return RedirectToAction("List");
    }

    // GET: Admin/ContentPages/Edit/{id}
    // Load a content page for editing by ID
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _contentPageService.GetByIdAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    // POST: Admin/ContentPages/Edit/{id}
    // Submit updates to a content page
    [HttpPost]
    public async Task<IActionResult> Edit(ContentPageDto dto)
    {
        // Update the timestamp to the current time
        dto.UpdatedAt = DateTime.Now;

        await _contentPageService.UpdateAsync(dto);
        return RedirectToAction("List");
    }
}

using Fe.Dtos.Ngos;
using Fe.Services.Ngos;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NGOsController : Controller
    {
        private readonly INgoApiService _ngoApiService;
        public NGOsController(INgoApiService ngoApiService)
        {
            _ngoApiService = ngoApiService;
        }
        [HttpGet("Admin/NGOs/Logo")]
        public IActionResult GetLogo(string logoUrl)
        {
            try
            {
                var stream = _ngoApiService.GetLogoFileStream(logoUrl);
                var contentType = Path.GetExtension(logoUrl).ToLower() switch
                {
                    ".png" => "image/png",
                    ".svg" => "image/svg+xml",
                    _ => "application/octet-stream"
                };
                return File(stream, contentType);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
        // GET: Admin/NGOs
        public async Task<IActionResult> List()
        {
            var ngos = await _ngoApiService.GetAllAsync(); // Lấy danh sách partner từ API
            return View(ngos);
        }
        // GET: /Admin/Ngos/Add
        public IActionResult Add()
        {
            return View();
        }
        // POST: /Admin/Ngos/Add
        [HttpPost]
        public async Task<IActionResult> Add(CreateNgoDto dto, IFormFile logo)
        {
            ModelState.Remove("LogoUrl");
            // Kiểm tra logo
            if (logo == null || logo.Length == 0)
            {
                ModelState.AddModelError(nameof(dto.LogoUrl), "File field is required.");
            }
            else
            {
                var ext = Path.GetExtension(logo.FileName).ToLower();
                if (ext != ".png" && ext != ".svg")
                    ModelState.AddModelError(nameof(dto.LogoUrl), "Logo must be a .png or .svg file.");

                if (logo.Length > 500 * 1024)
                    ModelState.AddModelError(nameof(dto.LogoUrl), "Logo must be smaller than 500KB.");
            }
            if (!ModelState.IsValid)
                return View(dto);
            try
            {
                // Truyền IFormFile trực tiếp vào service
                await _ngoApiService.AddAsync(dto, logo);
                return RedirectToAction("List");
            }
            catch (HttpRequestException ex)
            {
                var errorMessage = ex.Message;

                if (errorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    if (errorMessage.Contains("WebsiteUrl", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.WebsiteUrl), "This website URL already exists.");
                    else
                        ModelState.AddModelError(nameof(dto.Name), "This name already exists.");
                }
                else if (errorMessage.Contains("must not be empty", StringComparison.OrdinalIgnoreCase))
                {
                    if (errorMessage.Contains("WebsiteUrl", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.WebsiteUrl), "Website URL is required.");
                    else if (errorMessage.Contains("Name", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.Name), "Name is required.");
                }
                else if (errorMessage.Contains("unreachable", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(dto.WebsiteUrl), "The website URL is unreachable or invalid.");
                }
                else if (errorMessage.Contains("valid URL format", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(dto.WebsiteUrl), "Website URL must be a valid format.");
                }
                else
                    ModelState.AddModelError(string.Empty, errorMessage);

                return View(dto);
            }
        }
        // GET: /Admin/Partners/CheckInUse/{id}
        public async Task<IActionResult> CheckInUse(int id)
        {
            try
            {
                bool isUsed = await _ngoApiService.CheckInUseAsync(id);
                return Json(new { isUsed });
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // GET: /Admin/Ngos/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var ngo = await _ngoApiService.GetByIdAsync(id);

            if (ngo == null)
                return NotFound();

            var dto = new UpdateNgoDto
            {
                NgoId = ngo.NgoId,
                Name = ngo.Name,
                LogoUrl = ngo.LogoUrl,
                WebsiteUrl = ngo.WebsiteUrl,
                AccountId = ngo.AccountId
            };

            return View(dto);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateNgoDto dto, IFormFile logo)
        {
            ModelState.Remove("LogoUrl");

            bool hasFileErrors = false;
            if (logo != null && logo.Length > 0)
            {
                var ext = Path.GetExtension(logo.FileName).ToLower();
                if (ext != ".png" && ext != ".svg")
                {
                    ModelState.AddModelError(nameof(dto.LogoUrl), "Logo must be a .png or .svg file.");
                    hasFileErrors = true;
                }
                if (logo.Length > 500 * 1024)
                {
                    ModelState.AddModelError(nameof(dto.LogoUrl), "Logo must be smaller than 500KB.");
                    hasFileErrors = true;
                }
            }
            // Nếu có lỗi file, trả về view ngay
            if (hasFileErrors)
                return View(dto);
            try
            {
                await _ngoApiService.EditAsync(dto, logo);
                return RedirectToAction("List");
            }
            catch (HttpRequestException ex)
            {
                var errorMessage = ex.Message;

                if (errorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    if (errorMessage.Contains("WebsiteUrl", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.WebsiteUrl), "This website URL already exists.");
                    else
                        ModelState.AddModelError(nameof(dto.Name), "This name already exists.");
                }
                else if (errorMessage.Contains("must not be empty", StringComparison.OrdinalIgnoreCase))
                {
                    if (errorMessage.Contains("WebsiteUrl", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.WebsiteUrl), "Website URL is required.");
                    else if (errorMessage.Contains("Name", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.Name), "Name is required.");
                }
                else if (errorMessage.Contains("unreachable", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(dto.WebsiteUrl), "The website URL is unreachable or invalid.");
                }
                else if (errorMessage.Contains("valid URL format", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(dto.WebsiteUrl), "Website URL must be a valid format.");
                }
                else
                    ModelState.AddModelError(string.Empty, errorMessage);

                return View(dto);
            }

        }
        // GET: /Admin/Ngos/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _ngoApiService.DeleteAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Warning"] = ex.Message;
            }

            return RedirectToAction("List");
        }
    }
}

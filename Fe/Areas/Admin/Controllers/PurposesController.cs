using Fe.DTOs.Purposes;
using Fe.Services.Purposes;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PurposesController : Controller
    {
        private readonly IPurposeApiService _purposeService;

        public PurposesController(IPurposeApiService purposeService)
        {
            _purposeService = purposeService;
        }

        // GET: /Admin/Purposes
        public async Task<IActionResult> List()
        {
            var purposes = await _purposeService.GetAllAsync();
            return View(purposes); 
        }


        // GET: /Admin/Purposes/Add
        public IActionResult Add()
        {
            return View();
        }

        // POST: /Admin/Purposes/Add
        [HttpPost]
        public async Task<IActionResult> Add(CreatePurposeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var success = await _purposeService.CreateAsync(dto);
                if (success)
                {
                    return RedirectToAction("List");
                }

                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
            }
            catch (HttpRequestException ex)
            {
                var errorMessage = ex.Message;

                // Phân loại lỗi để đưa về đúng field
                if (errorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(dto.Title), "This title already exists.");
                }
                else if (errorMessage.Contains("must not be empty", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(dto.Title), "Title is required.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, errorMessage);
                }
            }

            return View(dto);
        }


        // GET: /Admin/Purposes/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _purposeService.DeleteAsync(id);
            return RedirectToAction("List");
        }
    }
}

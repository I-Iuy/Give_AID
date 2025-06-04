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
                await _purposeService.CreateAsync(dto);
                return RedirectToAction("List");
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

                return View(dto);
            }
        }
        // GET: /Admin/Purposes/CheckInUse/{id}
        public async Task<IActionResult> CheckInUse(int id)
        {
            try
            {
                bool isUsed = await _purposeService.CheckInUseAsync(id);
                return Json(new { isUsed }); 
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(ex.Message); 
            }
        }
        // GET: /Admin/Purposes/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var purpose = await _purposeService.GetByIdAsync(id);

            if (purpose == null)
                return NotFound();

            var dto = new UpdatePurposeDto
            {
                PurposeId = purpose.PurposeId,
                Title = purpose.Title,
                AccountId = purpose.AccountId
            };

            return View(dto);
        }
        [HttpPost]
        // POST: /Admin/Purposes/Edit/{id}
        public async Task<IActionResult> Edit(UpdatePurposeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _purposeService.EditAsync(dto);
                return RedirectToAction("List");
            }
            catch (HttpRequestException ex)
            {
                var errorMessage = ex.Message;

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

                return View(dto);
            }
        }

        // GET: /Admin/Purposes/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _purposeService.DeleteAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Warning"] = ex.Message; 
            }

            return RedirectToAction("List");
        }
    }
}

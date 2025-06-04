using Fe.DTOs.Partners;
using Fe.Services.Partners;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PartnersController : Controller
    {
        private readonly IPartnerApiService _partnerService;

        public PartnersController(IPartnerApiService partnerService)
        {
            _partnerService = partnerService;
        }

        private void RemoveFileFieldsFromModelState()
        {
            ModelState.Remove("LogoUrl");
            ModelState.Remove("ContractFile");
        }

        [HttpGet("Admin/Partners/Logo")]
        public IActionResult GetLogo(string logoUrl)
        {
            try
            {
                var stream = _partnerService.GetLogoFileStream(logoUrl); // gọi hàm từ service
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

        [HttpGet("Admin/Partners/Contract")]
        public IActionResult GetContract(string fileUrl)
        {
            try
            {
                var stream = _partnerService.GetContractFileStream(fileUrl);
                var contentType = Path.GetExtension(fileUrl).ToLower() switch
                {
                    ".pdf" => "application/pdf",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    _ => "application/octet-stream"
                };

                return File(stream, contentType);
            }
            catch (FileNotFoundException)
            {
                return NotFound("File not found.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: /Admin/Partners
        public async Task<IActionResult> List()
        {
            var partners = await _partnerService.GetAllAsync(); // Lấy danh sách partner từ API
            return View(partners);
        }

        // GET: /Admin/Partners/Add
        public IActionResult Add()
        {
            return View();
        }

        // POST: /Admin/Partners/Add
        [HttpPost]
        public async Task<IActionResult> Add(CreatePartnerDto dto, IFormFile logo, IFormFile contract)
        {
            RemoveFileFieldsFromModelState();

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

            // Kiểm tra file hợp đồng
            if (contract == null || contract.Length == 0)
            {
                ModelState.AddModelError(nameof(dto.ContractFile), "File field is required.");
            }
            else
            {
                var ext = Path.GetExtension(contract.FileName).ToLower();
                if (ext != ".pdf" && ext != ".docx")
                    ModelState.AddModelError(nameof(dto.ContractFile), "Contract must be a .pdf or .docx file.");

                if (contract.Length > 5 * 1024 * 1024)
                    ModelState.AddModelError(nameof(dto.ContractFile), "Contract must be smaller than 5MB.");
            }

            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                // Truyền IFormFile trực tiếp vào service
                await _partnerService.AddAsync(dto, logo, contract);
                return RedirectToAction("List");
            }
            catch (HttpRequestException ex)
            {
                var errorMessage = ex.Message;

                if (errorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    ModelState.AddModelError(nameof(dto.Name), "This name already exists.");
                else if (errorMessage.Contains("must not be empty", StringComparison.OrdinalIgnoreCase))
                    ModelState.AddModelError(nameof(dto.Name), "Name is required.");
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
                bool isUsed = await _partnerService.CheckInUseAsync(id);
                return Json(new { isUsed });
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: /Admin/Partners/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var partner = await _partnerService.GetByIdAsync(id); 

            if (partner == null)
                return NotFound();

            var dto = new UpdatePartnerDto
            {
                PartnerId = partner.PartnerId,
                Name = partner.Name,
                LogoUrl = partner.LogoUrl,
                ContractFile = partner.ContractFile,
                AccountId = partner.AccountId
            };

            return View(dto); 
        }
        [HttpPost]
        public async Task<IActionResult> Edit(UpdatePartnerDto dto, IFormFile logo, IFormFile contract)
        {
            RemoveFileFieldsFromModelState();

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

            if (contract != null && contract.Length > 0)
            {
                var ext = Path.GetExtension(contract.FileName).ToLower();
                if (ext != ".pdf" && ext != ".docx")
                {
                    ModelState.AddModelError(nameof(dto.ContractFile), "Contract must be a .pdf or .docx file.");
                    hasFileErrors = true;
                }
                if (contract.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(nameof(dto.ContractFile), "Contract must be smaller than 5MB.");
                    hasFileErrors = true;
                }
            }

            // Nếu có lỗi file, trả về view ngay
            if (hasFileErrors)
                return View(dto);
       
            try
            {
                await _partnerService.EditAsync(dto, logo, contract);
                return RedirectToAction("List");
            }
            catch (HttpRequestException ex)
            {
                var errorMessage = ex.Message;

                if (errorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    ModelState.AddModelError(nameof(dto.Name), "This name already exists.");
                else if (errorMessage.Contains("must not be empty", StringComparison.OrdinalIgnoreCase))
                    ModelState.AddModelError(nameof(dto.Name), "Name is required.");
                else
                    ModelState.AddModelError(string.Empty, errorMessage);

                return View(dto);
            }
        }


        // GET: /Admin/Partners/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _partnerService.DeleteAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Warning"] = ex.Message;
            }

            return RedirectToAction("List");
        }
    }
}

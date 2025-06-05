using Fe.DTOs.Donations;
using Fe.Services.Donations;
using Fe.Services.Getdata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fe.Areas.Web.Controllers
{
    [Area("Web")]
    public class DonationController : Controller
    {
        private readonly IDonationApiService _donationService;
        private readonly IGetdataApiService _getdataService;

        public DonationController(IDonationApiService donationService, IGetdataApiService getdataService)
        {
            _donationService = donationService;
            _getdataService = getdataService;
        }
        [HttpGet]
        private async Task ReloadDonationView()
        {
            var purposes = await _getdataService.GetAllPurposesAsync();
            ViewBag.Purposes = purposes.Select(p => new SelectListItem
            {
                Value = p.PurposeId.ToString(),
                Text = p.Title
            }).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> AddFromIndex()
        {
                await ReloadDonationView();
                return View("FilterPartial", new CreateDonationDto());
        }
        [HttpGet]
        public async Task<IActionResult> AddFromPost(int purposeId, int? campaignId = null)
        {
            var purposes = await _getdataService.GetAllPurposesAsync();

            ViewBag.Purposes = purposes
                .Select(p => new SelectListItem
                {
                    Value = p.PurposeId.ToString(),
                    Text = p.Title,
                    Selected = p.PurposeId == purposeId
                }).ToList();

            ViewBag.CampaignId = campaignId;

            return PartialView("FilterPartial", new CreateDonationDto
            {
                PurposeId = purposeId 
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CreateDonationDto dto)
        {
            if (!ModelState.IsValid)
            {
                await ReloadDonationView();
                return View("FilterPartial", dto);
            }

            try
            {
                await _donationService.Add(dto);
                return Json(new { status = "Success" });
            }
            catch (HttpRequestException ex)
            {
                var errorMessage = ex.Message;

                // Xử lý lỗi cụ thể được ném từ BE
                if (errorMessage.Contains("must not be empty", StringComparison.OrdinalIgnoreCase))
                {
                    if (errorMessage.Contains("Full Name", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.FullName), "Full Name is required.");
                    if (errorMessage.Contains("Email", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.Email), "Email is required.");
                    if (errorMessage.Contains("Address", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.Address), "Address is required.");
                    if (errorMessage.Contains("Status", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.Status), "Status is required.");
                }
                else if (errorMessage.Contains("Invalid purpose ID", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(dto.PurposeId), "Invalid purpose selected.");
                }
                else if (errorMessage.Contains("Invalid campaign ID", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(dto.CampaignId), "Invalid campaign selected.");
                }
                else if (errorMessage.Contains("must be greater than 0", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(dto.Amount), "Amount must be greater than 0.");
                }
                else if (errorMessage.Contains("Status must be", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(dto.Status), "Status must be either 'Success' or 'Failed'.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, errorMessage); 
                }
                await ReloadDonationView();
                return View("FilterPartial", dto);
            }
        }


    }
}

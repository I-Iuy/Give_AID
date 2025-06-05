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
            return PartialView("FilterPartial", new CreateDonationDto());
        }
        [HttpGet]
        public async Task<IActionResult> AddFromPost(int purposeId, int? campaignId = null, string campaignTitle = "", string purposeTitle = "")
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
            ViewBag.CampaignTitle = campaignTitle;
            ViewBag.PurposeTitle = purposeTitle;

            return PartialView("FilterPartial", new CreateDonationDto
            {
                PurposeId = purposeId
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateDonationDto dto)
        {
            if (!ModelState.IsValid)
            {
                await ReloadDonationView();
                return PartialView("FilterPartial", dto);
            }

            try
            {
                await _donationService.Add(dto);
                return Json(new { status = "Success" });
            }
            catch (HttpRequestException ex)
            {
                var errorMessage = ex.Message;

                if (errorMessage.Contains("must not be empty", StringComparison.OrdinalIgnoreCase))
                {
                    if (errorMessage.Contains("Full Name", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.FullName), "Full Name is required.");
                    if (errorMessage.Contains("Email", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.Email), "Email is required.");
                    if (errorMessage.Contains("Address", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.Address), "Address is required.");
                }
                else if (errorMessage.Contains("must not exceed", StringComparison.OrdinalIgnoreCase))
                {
                    if (errorMessage.Contains("Full Name", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.FullName), "Full Name must not exceed 100 characters.");
                    if (errorMessage.Contains("Email", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.Email), "Email must not exceed 200 characters.");
                    if (errorMessage.Contains("Address", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.Address), "Address must not exceed 250 characters.");
                }
                else if (errorMessage.Contains("Invalid purpose ID", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(dto.PurposeId), "Invalid purpose selected.");
                }
                else if (errorMessage.Contains("must be greater than 0", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(dto.Amount), "Amount must be greater than 0.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, errorMessage); 
                }
                await ReloadDonationView();
                ViewBag.CampaignId = dto.CampaignId;
                ViewBag.CampaignTitle = Request.Form["campaignTitle"];
                ViewBag.PurposeTitle = Request.Form["purposeTitle"];
                return PartialView("FilterPartial", dto);

            }
        }


    }
}

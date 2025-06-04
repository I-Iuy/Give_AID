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
        private async Task<IActionResult> ReloadDonationView(object dto, string viewName)
        {
            ViewBag.Purposes = (await _getdataService.GetAllPurposesAsync())
                .Select(p => new SelectListItem
                {
                    Value = p.PurposeId.ToString(),
                    Text = p.Title
                }).ToList();

            ViewBag.Campaigns = (await _getdataService.GetAllCampaignsAsync())
                .Select(c => new SelectListItem
                {
                    Value = c.CampaignId.ToString(),
                    Text = c.Title
                }).ToList();

            return View(viewName, dto);
        }
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            return await ReloadDonationView(new CreateDonationDto(), "FilterPartial");
        }
     
    }
}

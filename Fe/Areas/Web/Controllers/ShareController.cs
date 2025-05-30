using Fe.DTOs.Share;
using Fe.Services.Share;
using Microsoft.AspNetCore.Mvc;

namespace Fe.Areas.Web.Controllers
{
    [Area("Web")]
    [Route("Web/[controller]/[action]")]
    public class ShareController : Controller
    {
        private readonly IShareService _shareService;

        public ShareController(IShareService shareService)
        {
            _shareService = shareService;
        }

        // GET: Share/Create?campaignId=123
        [HttpGet]
        public IActionResult Create(int campaignId)
        {
            var model = new CreateShareDto
            {
                CampaignId = campaignId,
                Platform = "Email"
            };
            return View(model);
        }

        // POST: Share/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateShareDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var success = await _shareService.CreateAsync(dto);

            if (success)
                return RedirectToAction("Success");

            ModelState.AddModelError("", "Failed to share the campaign. Please try again.");
            return View(dto);
        }

        // GET: Share/Success
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}

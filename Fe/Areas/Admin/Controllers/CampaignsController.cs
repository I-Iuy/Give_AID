using Fe.DTOs.Campaigns;
using Fe.Services.Campaigns;
using Fe.Services.Getdata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CampaignsController : Controller
    {
       private readonly ICampaignApiService _campaignApiService;
       private readonly IGetdataApiService _getdataApiService;
        public CampaignsController(ICampaignApiService campaignApiService, IGetdataApiService getdataApiService)
        {
            _campaignApiService = campaignApiService;
            _getdataApiService = getdataApiService;
        }
        // GET: Admin/Campaigns
        public async Task<IActionResult> List()
        {
            var campaigns = await _campaignApiService.GetAllAsync();
            return View(campaigns);
        }
        public async Task<IActionResult> Add()
        {
            ViewBag.Purposes = (await _getdataApiService.GetAllPurposesAsync())
                .Select(p => new SelectListItem
                {
                    Value = p.PurposeId.ToString(),
                    Text = p.Title
                }).ToList();

            ViewBag.Partners = (await _getdataApiService.GetAllPartnersAsync())
                .Select(p => new SelectListItem
                {
                    Value = p.PartnerId.ToString(),
                    Text = p.Name
                }).ToList();

            ViewBag.NGOs = (await _getdataApiService.GetAllNgosAsync())
                .Select(n => new SelectListItem
                {
                    Value = n.NgoId.ToString(),
                    Text = n.Name
                }).ToList();

            return View();
        }
        private async Task<IActionResult> ReloadAddView(CreateCampaignDto dto)
        {
            ViewBag.Purposes = (await _getdataApiService.GetAllPurposesAsync())
                .Select(p => new SelectListItem
                {
                    Value = p.PurposeId.ToString(),
                    Text = p.Title
                }).ToList();

            ViewBag.Partners = (await _getdataApiService.GetAllPartnersAsync())
                .Select(p => new SelectListItem
                {
                    Value = p.PartnerId.ToString(),
                    Text = p.Name
                }).ToList();

            ViewBag.NGOs = (await _getdataApiService.GetAllNgosAsync())
                .Select(n => new SelectListItem
                {
                    Value = n.NgoId.ToString(),
                    Text = n.Name
                }).ToList();

            return View("Add", dto);
        }
        [HttpPost]
        public async Task<IActionResult> Add(CreateCampaignDto dto)
        {
            //if (!Uri.TryCreate(dto.VideoUrl, UriKind.Absolute, out var uri) ||
            //    !(uri.Host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) && uri.Query.Contains("v=")) &&
            //    !(uri.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase) && uri.AbsolutePath.Length > 1))
            //{
            //    ModelState.AddModelError(nameof(dto.VideoUrl), "The video URL must be a valid YouTube link.");
            //    return await ReloadAddView(dto);
            //}

            //if (!ModelState.IsValid)
            //{
            //    return View(dto);
            //}

            //try
            //{
                await _campaignApiService.AddAsync(dto);
                return RedirectToAction("List");
            //}
            //catch (HttpRequestException ex)
            //{
            //    var errorMessage = ex.Message;

            //    if (errorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            //    {
            //        if (errorMessage.Contains("VideoUrl", StringComparison.OrdinalIgnoreCase))
            //            ModelState.AddModelError(nameof(dto.VideoUrl), "This video URL already exists.");
            //        else if (errorMessage.Contains("Title", StringComparison.OrdinalIgnoreCase))
            //            ModelState.AddModelError(nameof(dto.Title), "A campaign with this title already exists.");
            //    }
            //    else if (errorMessage.Contains("must not be empty", StringComparison.OrdinalIgnoreCase))
            //    {
            //        if (errorMessage.Contains("Title", StringComparison.OrdinalIgnoreCase))
            //            ModelState.AddModelError(nameof(dto.Title), "Title is required.");
            //        else if (errorMessage.Contains("VideoUrl", StringComparison.OrdinalIgnoreCase))
            //            ModelState.AddModelError(nameof(dto.VideoUrl), "Video URL is required.");
            //        else if (errorMessage.Contains("Content", StringComparison.OrdinalIgnoreCase))
            //            ModelState.AddModelError(nameof(dto.Content), "Content is required.");
            //    }
            //    else if (errorMessage.Contains("unreachable", StringComparison.OrdinalIgnoreCase)
            //          || errorMessage.Contains("valid URL format", StringComparison.OrdinalIgnoreCase))
            //    {
            //        ModelState.AddModelError(nameof(dto.VideoUrl), "The video URL is unreachable or invalid.");
            //    }
            //    else if (errorMessage.Contains("Purpose ID", StringComparison.OrdinalIgnoreCase)
            //          || errorMessage.Contains("Partner ID", StringComparison.OrdinalIgnoreCase)
            //          || errorMessage.Contains("NGO", StringComparison.OrdinalIgnoreCase))
            //    {
            //        ModelState.AddModelError(string.Empty, "One or more selected options are invalid.");
            //    }
            //    else
            //    {
            //        ModelState.AddModelError(string.Empty, errorMessage);
            //    }

            //    return await ReloadAddView(dto);
            //}
        }

    }
}


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

        [HttpGet]
        private async Task<IActionResult> ReloadCampaignView(object dto, string viewName)
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

            return View(viewName, dto);
        }
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var campaigns = await _campaignApiService.GetAllAsync();
            return View(campaigns);
        }
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            return await ReloadCampaignView(new CreateCampaignDto(), "Add");
        }
        [HttpPost]
        public async Task<IActionResult> Add(CreateCampaignDto dto)
        {
            if (dto.PurposeId <= 0)
                ModelState.AddModelError(nameof(dto.PurposeId), "Please select a purpose.");

            if ((dto.PartnerIds == null || !dto.PartnerIds.Any()) &&
                (dto.NgoIds == null || !dto.NgoIds.Any()))
            {
                ModelState.AddModelError("PartnersAndNgos", "Please select at least one Partner and one NGO.");
            }
            else if (dto.PartnerIds == null || !dto.PartnerIds.Any())
            {
                ModelState.AddModelError("PartnersAndNgos", "Please select at least one Partner.");
            }
            else if (dto.NgoIds == null || !dto.NgoIds.Any())
            {
                ModelState.AddModelError("PartnersAndNgos", "Please select at least one NGO.");
            }

            if (!Uri.TryCreate(dto.VideoUrl, UriKind.Absolute, out var uri) ||
                !(uri.Host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) && uri.Query.Contains("v=")) &&
                !(uri.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase) && uri.AbsolutePath.Length > 1))
            {
                ModelState.AddModelError(nameof(dto.VideoUrl), "The video URL must be a valid YouTube link.");
                return await ReloadCampaignView(dto, "Add");
            }

            if (!ModelState.IsValid)
            {
                return await ReloadCampaignView(dto, "Add");
            }

            try
            {
                await _campaignApiService.AddAsync(dto);
                return RedirectToAction("List");
            }
            catch (HttpRequestException ex)
            {
                var errorMessage = ex.Message;

                if (errorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    if (errorMessage.Contains("VideoUrl", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.VideoUrl), "This video URL already exists.");
                    else if (errorMessage.Contains("Title", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.Title), "A campaign with this title already exists.");
                }
                else if (errorMessage.Contains("must not be empty", StringComparison.OrdinalIgnoreCase))
                {
                    if (errorMessage.Contains("Title", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.Title), "Title is required.");
                    else if (errorMessage.Contains("VideoUrl", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.VideoUrl), "Video URL is required.");
                    else if (errorMessage.Contains("Content", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.Content), "Content is required.");
                }
                else if (errorMessage.Contains("unreachable", StringComparison.OrdinalIgnoreCase)
                      || errorMessage.Contains("valid URL format", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(dto.VideoUrl), "The video URL is unreachable or invalid.");
                }
                else if (errorMessage.Contains("Purpose ID", StringComparison.OrdinalIgnoreCase)
                      || errorMessage.Contains("Partner ID", StringComparison.OrdinalIgnoreCase)
                      || errorMessage.Contains("NGO", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty, "One or more selected options are invalid.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, errorMessage);
                }
                return await ReloadCampaignView(dto, "Add");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var campaign = await _campaignApiService.GetByIdAsync(id);
            var allPurposes = await _getdataApiService.GetAllPurposesAsync();
            var allPartners = await _getdataApiService.GetAllPartnersAsync();
            var allNgos = await _getdataApiService.GetAllNgosAsync();

            var selectedPartnerIds = allPartners
                .Where(p => campaign.PartnerNames.Contains(p.Name))
                .Select(p => p.PartnerId)
                .ToList();

            var selectedNgoIds = allNgos
                .Where(n => campaign.NgoNames.Contains(n.Name))
                .Select(n => n.NgoId)
                .ToList();

            var selectedPurposeId = allPurposes
                .FirstOrDefault(p => p.Title == campaign.PurposeTitle)?.PurposeId ?? 0;

            var dto = new UpdateCampaignDto
            {
                CampaignId = campaign.CampaignId,
                Title = campaign.Title,
                Content = campaign.Content,
                VideoUrl = campaign.VideoUrl,
                EventDate = campaign.EventDate,
                AccountId = campaign.AccountId,
                PurposeId = selectedPurposeId,
                NgoIds = selectedNgoIds,
                PartnerIds = selectedPartnerIds
            };

            ViewBag.Purposes = allPurposes.Select(p => new SelectListItem
            {
                Value = p.PurposeId.ToString(),
                Text = p.Title
            }).ToList();

            ViewBag.Partners = allPartners.Select(p => new SelectListItem
            {
                Value = p.PartnerId.ToString(),
                Text = p.Name
            }).ToList();

            ViewBag.NGOs = allNgos.Select(n => new SelectListItem
            {
                Value = n.NgoId.ToString(),
                Text = n.Name
            }).ToList();

            return View(dto);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateCampaignDto dto)
        {
            if (dto.PurposeId <= 0)
                ModelState.AddModelError(nameof(dto.PurposeId), "Please select a purpose.");

            if ((dto.PartnerIds == null || !dto.PartnerIds.Any()) &&
                (dto.NgoIds == null || !dto.NgoIds.Any()))
            {
                ModelState.AddModelError("PartnersAndNgos", "Please select at least one Partner and one NGO.");
            }
            else if (dto.PartnerIds == null || !dto.PartnerIds.Any())
            {
                ModelState.AddModelError("PartnersAndNgos", "Please select at least one Partner.");
            }
            else if (dto.NgoIds == null || !dto.NgoIds.Any())
            {
                ModelState.AddModelError("PartnersAndNgos", "Please select at least one NGO.");
            }

            if (!Uri.TryCreate(dto.VideoUrl, UriKind.Absolute, out var uri) ||
                !(uri.Host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) && uri.Query.Contains("v=")) &&
                !(uri.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase) && uri.AbsolutePath.Length > 1))
            {
                ModelState.AddModelError(nameof(dto.VideoUrl), "The video URL must be a valid YouTube link.");
                return await ReloadCampaignView(dto, "Edit");
            }

            if (!ModelState.IsValid)
            {
                return await ReloadCampaignView(dto, "Edit");
            }

            try
            {
                await _campaignApiService.EditAsync(dto);
                return RedirectToAction("List");
            }
            catch (HttpRequestException ex)
            {
                var errorMessage = ex.Message;

                if (errorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    if (errorMessage.Contains("VideoUrl", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.VideoUrl), "This video URL already exists.");
                    else if (errorMessage.Contains("Title", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.Title), "A campaign with this title already exists.");
                }
                else if (errorMessage.Contains("must not be empty", StringComparison.OrdinalIgnoreCase))
                {
                    if (errorMessage.Contains("Title", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.Title), "Title is required.");
                    else if (errorMessage.Contains("VideoUrl", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.VideoUrl), "Video URL is required.");
                    else if (errorMessage.Contains("Content", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError(nameof(dto.Content), "Content is required.");
                }
                else if (errorMessage.Contains("unreachable", StringComparison.OrdinalIgnoreCase)
                      || errorMessage.Contains("valid URL format", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(dto.VideoUrl), "The video URL is unreachable or invalid.");
                }
                else if (errorMessage.Contains("Purpose ID", StringComparison.OrdinalIgnoreCase)
                      || errorMessage.Contains("Partner ID", StringComparison.OrdinalIgnoreCase)
                      || errorMessage.Contains("NGO", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty, "One or more selected options are invalid.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, errorMessage);
                }
                return await ReloadCampaignView(dto, "Edit");
            }
         
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            await _campaignApiService.DeleteAsync(id);
            return RedirectToAction("List");
        }
    }
}


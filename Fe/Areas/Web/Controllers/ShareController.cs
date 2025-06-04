using Fe.DTOs.Share;
using Fe.Services;
using Fe.Services.Share;
using Fe.Services.Campaigns;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Fe.Areas.Web.Controllers
{
    [Area("Web")]
    [Route("Web/[controller]/[action]")]
    public class ShareController : Controller
    {
        private readonly IShareService _shareService;
        private readonly ICampaignApiService _campaignService;
        private readonly ILogger<ShareController> _logger;
        private readonly IConfiguration _configuration;

        public ShareController(
            IShareService shareService,
            ICampaignApiService campaignService,
            ILogger<ShareController> logger,
            IConfiguration configuration)
        {
            _shareService = shareService;
            _campaignService = campaignService;
            _logger = logger;
            _configuration = configuration;
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
            try
            {
                // Log the received data for debugging purposes  
                _logger.LogInformation("ShareController POST Create received. CampaignId: {CampaignId}, Email: {Email}, Name: {GuestName}, Platform: {Platform}",
                    dto.CampaignId, dto.ReceiverEmail, dto.GuestName, dto.Platform);

                // Validate the model state to ensure all required fields are correctly populated  
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState is invalid.");
                    return View(dto);
                }

                // Check if CampaignId is null or invalid before proceeding  
                if (!dto.CampaignId.HasValue || dto.CampaignId.Value <= 0)
                {
                    ModelState.AddModelError("", "Invalid campaign ID.");
                    _logger.LogWarning("Invalid or missing CampaignId: {CampaignId}", dto.CampaignId);
                    return View(dto);
                }

                // Log the start of the sharing process for the specified campaign  
                _logger.LogInformation("Starting share process for campaign {CampaignId}", dto.CampaignId);

                // Retrieve campaign details using the CampaignId (use .Value since null has been checked)  
                var campaign = await _campaignService.GetByIdAsync(dto.CampaignId.Value);
                if (campaign == null)
                {
                    ModelState.AddModelError("", "Campaign not found.");
                    _logger.LogWarning("Campaign {CampaignId} not found.", dto.CampaignId);
                    return View(dto);
                }

                // Calculate the ShareUrl based on the frontend base URL and assign it to the DTO  
                var frontendBaseUrl = _configuration["AppSettings:FrontendBaseUrl"] ?? "https://localhost:3000";
                var shareUrl = $"{frontendBaseUrl}/Web/Home/Post/{dto.CampaignId.Value}"; // Use the correct path to the Post page  
                dto.ShareUrl = shareUrl;

                // Check if the sharing platform is email  
                if (dto.Platform.ToLower() == "email")
                {
                    // Validate the recipient's email address  
                    if (string.IsNullOrEmpty(dto.ReceiverEmail))
                    {
                        ModelState.AddModelError("ReceiverEmail", "Please enter recipient's email address");
                        _logger.LogWarning("Receiver email is empty for Email platform.");
                        return View(dto);
                    }

                    try
                    {
                        // Get the base URL from the current request  
                        var baseUrl = $"{Request.Scheme}://{Request.Host}";

                        // Call the ShareService to handle email sending  
                        var result = await _shareService.ShareCampaignAsync(dto, baseUrl);
                        if (!result)
                        {
                            ModelState.AddModelError("", "Failed to send email. Please try again later.");
                            _logger.LogError("ShareService.ShareAsync returned false.");
                            return View(dto);
                        }
                        _logger.LogInformation("Email sent successfully to {Email} for campaign {CampaignId}",
                            dto.ReceiverEmail, dto.CampaignId);

                        // Redirect to the Success page after successfully sending the email  
                        TempData["SuccessMessage"] = "Campaign shared successfully!";
                        return RedirectToAction("Success");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email to {Email}", dto.ReceiverEmail);
                        ModelState.AddModelError("", $"Failed to send email: {ex.Message}");
                        return View(dto);
                    }
                }

                // Handle other platforms if needed  
                // Use the calculated ShareUrl  
                if (dto.Platform.ToLower() == "facebook")
                {
                    var fbUrl = $"https://www.facebook.com/sharer/sharer.php?u={Uri.EscapeDataString(dto.ShareUrl)}";
                    return Redirect(fbUrl);
                }

                if (dto.Platform.ToLower() == "whatsapp")
                {
                    var waUrl = $"https://wa.me/?text={Uri.EscapeDataString(dto.ShareUrl)}";
                    return Redirect(waUrl);
                }

                // Default behavior: redirect to the campaign detail page if the platform is not email, Facebook, or WhatsApp  
                TempData["SuccessMessage"] = "Campaign shared successfully!";
                return RedirectToAction("Post", "Home", new { id = dto.CampaignId.Value });
            }
            catch (Exception ex)
            {
                // Log the error and display a generic error message to the user  
                _logger.LogError(ex, "Error in ShareController.Create");
                ModelState.AddModelError("", "An error occurred while sharing the campaign. Please try again later.");
                return View(dto);
            }
        }

        // GET: Share/Success
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}

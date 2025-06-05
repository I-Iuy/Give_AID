// Import services and dependencies for campaign, content, NGO, partner, and purpose management
using Fe.Services.Campaigns;
using Fe.Services.ContentPages;
using Fe.Services.Ngos;
using Fe.Services.Partners;
using Fe.Services.Purposes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fe.Areas.Web.Controllers
{
    // Assign this controller to the "Web" area
    [Area("Web")]
    public class HomeController : Controller
    {
        // Injected service interfaces for business logic access
        private readonly IPurposeApiService _purposeService;
        private readonly IPartnerApiService _partnerService;
        private readonly INgoApiService _ngoService;
        private readonly ICampaignApiService _campaignService;
        private readonly IContentPageApiService _contentPageService;

        // Constructor: DI setup for all services used by this controller
        public HomeController(
            IPurposeApiService purposeService,
            IPartnerApiService partnerService,
            INgoApiService ngoService,
            ICampaignApiService campaignService,
            IContentPageApiService contentPageService)
        {
            _purposeService = purposeService;
            _partnerService = partnerService;
            _ngoService = ngoService;
            _campaignService = campaignService;
            _contentPageService = contentPageService;
        }

        // ====================== View Models ======================

        // Used to represent one campaign's display information
        public class FeatureCard
        {
            public int CampaignId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Link { get; set; }
            public string ImageUrl { get; set; }
            public string Cause { get; set; }
            public string VideoUrl { get; set; }

            // List of supporting organizations
            public List<OrgInfo> Partners { get; set; } = new();
            public List<OrgInfo> NGOs { get; set; } = new();

            // Mini-model for organization logo and name
            public class OrgInfo
            {
                public string Name { get; set; }
                public string LogoUrl { get; set; }
            }
        }
        // ====================== Helper Methods ======================

        // Extract the first image URL from raw HTML content using regex
        private string ExtractFirstImage(string html)
        {
            var match = Regex.Match(html ?? "", "<img[^>]*src=[\"'](?<src>[^\"']+)[\"']", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups["src"].Value : null;
        }

        // ====================== UI Routes ======================

        // GET: Web/Home/Index
        // Renders main homepage with paginated campaign list
        public async Task<IActionResult> Index(int page = 1)
        {
            const int PageSize = 6;
            var campaigns = await _campaignService.GetAllAsync();
            var totalItems = campaigns.Count();
            var pagedCampaigns = campaigns.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            var model = pagedCampaigns.Select(c => new FeatureCard
            {
                CampaignId = c.CampaignId,
                Title = c.Title,
                Description = c.Content,
                ImageUrl = ExtractFirstImage(c.Content) ?? "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
                Cause = c.PurposeTitle
            }).ToList();

            // Intro content for homepage hero section
            ViewData["IntroTitle"] = "Together for a Better World";
            ViewData["IntroDescription"] = "This is a platform dedicated to connecting communities and donors to support impactful NGO programs across the country.";
            ViewData["IntroButtonText"] = "Support a Cause";

            // Pagination info
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.CurrentPage = page;

            return View(model);
        }

        // GET: Web/Home/CampaignListPartial?page=1
        // Loads partial view of campaign list (used for AJAX pagination)
        [HttpGet]
        public async Task<IActionResult> CampaignListPartial(int page = 1)
        {
            const int PageSize = 6;
            var campaigns = await _campaignService.GetAllAsync();
            var totalItems = campaigns.Count();
            var pagedCampaigns = campaigns.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            var model = pagedCampaigns.Select(c => new FeatureCard
            {
                CampaignId = c.CampaignId,
                Title = c.Title,
                Description = c.Content,
                ImageUrl = ExtractFirstImage(c.Content) ?? "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
                Cause = c.PurposeTitle
            }).ToList();

            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.CurrentPage = page;

            return PartialView("_CampaignListPartial", model);
        }

        // GET: Web/Home/Post/5
        // Renders details of a specific campaign post
        public async Task<IActionResult> Post(int id, bool returnToAll = false)
        {
            var campaigns = await _campaignService.GetAllAsync();
            var campaign = campaigns.FirstOrDefault(c => c.CampaignId == id);
            if (campaign == null)
                return NotFound();

            // Prepare data model for the selected campaign
            var selectedPost = new FeatureCard
            {
                CampaignId = campaign.CampaignId,
                Title = campaign.Title,
                Description = campaign.Content,
                ImageUrl = ExtractFirstImage(campaign.Content) ?? "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
                Cause = campaign.PurposeTitle,
                VideoUrl = campaign.VideoUrl
            };

            ViewData["ProgramTitle"] = selectedPost.Title;
            ViewBag.SelectedCause = selectedPost.Cause;
            ViewBag.ReturnToAll = returnToAll;

            // Load supporting purposes for UI filters
            var purposes = await _purposeService.GetAllAsync();
            ViewBag.Purposes = purposes;

            // Load partner and NGO collaborators of this campaign
            var allPartners = await _partnerService.GetAllAsync();
            var allNgos = await _ngoService.GetAllAsync();

            selectedPost.Partners = allPartners
                .Where(p => campaign.PartnerIds != null && campaign.PartnerIds.Contains(p.PartnerId))
                .Select(p => new FeatureCard.OrgInfo
                {
                    Name = p.Name ?? "",
                    LogoUrl = p.LogoUrl ?? ""
                }).ToList();

            selectedPost.NGOs = allNgos
                .Where(n => campaign.NgoIds != null && campaign.NgoIds.Contains(n.NgoId))
                .Select(n => new FeatureCard.OrgInfo
                {
                    Name = n.Name ?? "",
                    LogoUrl = n.LogoUrl ?? ""
                }).ToList();

            // Show top 3 urgent campaigns for sidebar
            var urgentCampaigns = campaigns
                .OrderByDescending(c => c.EventDate)
                .Take(3)
                .ToList();
            ViewBag.UrgentCampaigns = urgentCampaigns;

            return View("Post", selectedPost);
        }

        // GET: Web/Home/AllCampaigns
        // Full campaign browsing with filter + search
        public async Task<IActionResult> AllCampaigns(string search, string category, int page = 1)
        {
            const int PageSize = 6;
            var allCards = await _campaignService.GetAllAsync();
            var query = allCards.AsQueryable();

            // Apply search filter if exists
            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.Title.Contains(search, StringComparison.OrdinalIgnoreCase) || c.Content.Contains(search));

            // Apply category filter if selected
            if (!string.IsNullOrEmpty(category))
                query = query.Where(c => c.PurposeTitle.Equals(category, StringComparison.OrdinalIgnoreCase));

            var totalItems = query.Count();
            var pagedCards = query.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            var model = pagedCards.Select(c => new FeatureCard
            {
                CampaignId = c.CampaignId,
                Title = c.Title,
                Description = c.Content,
                ImageUrl = ExtractFirstImage(c.Content) ?? "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
                Cause = c.PurposeTitle
            }).ToList();

            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;
            ViewBag.Category = category;

            var purposes = await _purposeService.GetAllAsync();
            ViewBag.Categories = purposes.Select(p => p.Title).Distinct().ToList();

            return View("AllCampaigns", model);
        }

        // GET: Web/Home/DonationHistory
        // Placeholder for future donation tracking view
        public IActionResult DonationHistory()
        {
            return View();
        }

        // GET: Web/Home/About/{slug}
        // Renders static content pages (e.g., About, Mission) dynamically
        [HttpGet("Web/Home/About/{slug}")]
        public async Task<IActionResult> About(string slug)
        {
            var page = await _contentPageService.GetBySlugAsync(slug);
            if (page == null) return NotFound();

            var purposes = await _purposeService.GetAllAsync();
            ViewBag.Purposes = purposes;

            var campaigns = await _campaignService.GetAllAsync();
            var urgentCampaigns = campaigns.OrderByDescending(c => c.EventDate).Take(3).ToList();
            ViewBag.UrgentCampaigns = urgentCampaigns;

            return View("About", page);
        }
    }
}
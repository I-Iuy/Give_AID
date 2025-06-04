using Fe.Services.Campaigns;
using Fe.Services.Ngos;
using Fe.Services.Partners;
using Fe.Services.Purposes;
using Fe.Services.Comment;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Fe.DTOs.Comment;

namespace Fe.Areas.Web.Controllers
{
    [Area("Web")]
    public class HomeController : Controller
    {
        private readonly IPurposeApiService _purposeService;
        private readonly IPartnerApiService _partnerService;
        private readonly INgoApiService _ngoService;
        private readonly ICampaignApiService _campaignService;
        private readonly ICommentService _commentService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IPurposeApiService purposeService,
            IPartnerApiService partnerService,
            INgoApiService ngoService,
            ICampaignApiService campaignService,
            ICommentService commentService,
            ILogger<HomeController> logger)
        {
            _purposeService = purposeService;
            _partnerService = partnerService;
            _ngoService = ngoService;
            _campaignService = campaignService;
            _commentService = commentService;
            _logger = logger;
        }

        public class FeatureCard
        {
            public int CampaignId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Link { get; set; }
            public string ImageUrl { get; set; }
            public string Cause { get; set; }

            public List<OrgInfo> Partners { get; set; } = new();
            public List<OrgInfo> NGOs { get; set; } = new();

            public class OrgInfo
            {
                public string Name { get; set; }
                public string LogoUrl { get; set; }
            }
        }

        public class DonationFilterModel
        {
            public string SelectedCause { get; set; }
            public List<string> Causes { get; set; } = new List<string>
            {
                "Children", "Disabled", "Education", "Elderly", "Employment",
                "Environment", "Health", "Women", "Youth"
            };
        }

        private string ExtractFirstImage(string html)
        {
            var match = Regex.Match(html ?? "", "<img[^>]*src=[\"'](?<src>[^\"']+)[\"']", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups["src"].Value : null;
        }

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

            ViewData["IntroTitle"] = "Together for a Better World";
            ViewData["IntroDescription"] = "This is a platform dedicated to connecting communities and donors to support impactful NGO programs across the country.";
            ViewData["IntroButtonText"] = "Support a Cause";

            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.CurrentPage = page;

            return View(model);
        }

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

        public async Task<IActionResult> Post(int id, bool returnToAll = false)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid campaign ID.";
                return RedirectToAction("Index");
            }

            var cards = await _campaignService.GetAllAsync();
            var campaign = cards.FirstOrDefault(c => c.CampaignId == id);
            if (campaign == null)
            {
                TempData["ErrorMessage"] = "Campaign not found.";
                return RedirectToAction("Index");
            }

            var selectedPost = new FeatureCard
            {
                CampaignId = campaign.CampaignId,
                Title = campaign.Title,
                Description = campaign.Content,
                ImageUrl = ExtractFirstImage(campaign.Content) ?? "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
                Cause = campaign.PurposeTitle
            };

            ViewData["ProgramTitle"] = selectedPost.Title;
            ViewBag.SelectedCause = selectedPost.Cause;
            ViewBag.ReturnToAll = returnToAll;

            var purposes = await _purposeService.GetAllAsync();
            ViewBag.Purposes = purposes;

            var partners = await _partnerService.GetAllAsync();
            var ngos = await _ngoService.GetAllAsync();

            selectedPost.Partners = partners.Select(p => new FeatureCard.OrgInfo
            {
                Name = p.Name,
                LogoUrl = p.LogoUrl
            }).ToList();

            selectedPost.NGOs = ngos.Select(n => new FeatureCard.OrgInfo
            {
                Name = n.Name,
                LogoUrl = n.LogoUrl
            }).ToList();

            // Load comments for this campaign
            try
            {
                var comments = await _commentService.GetByCampaignAsync(id);
                ViewBag.Comments = comments;
                _logger.LogInformation($"Loaded {comments?.Count ?? 0} comments for campaign {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading comments for campaign {id}");
                ViewBag.Comments = new List<CommentDto>();
            }

            // Lấy 3 bài mới nhất theo ngày
            var urgentCampaigns = cards
                .OrderByDescending(c => c.EventDate)
                .Take(3)
                .ToList();

            ViewBag.UrgentCampaigns = urgentCampaigns;

            return View("Post", selectedPost);
        }

        public IActionResult FilterPartial()
        {
            var model = new DonationFilterModel();
            return PartialView("FilterPartial", model);
        }

        public async Task<IActionResult> AllCampaigns(string search, string category, int page = 1)
        {
            const int PageSize = 6;
            var allCards = await _campaignService.GetAllAsync();
            var query = allCards.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.Title.Contains(search, StringComparison.OrdinalIgnoreCase) || c.Content.Contains(search));

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

        public IActionResult DonationHistory()
        {
            return View();
        }

        public IActionResult WhatWeDo()
        {
            return View();
        }
    }
}

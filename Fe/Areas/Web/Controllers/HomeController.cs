using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Fe.Areas.Web.Controllers
{
    [Area("Web")]
    public class HomeController : Controller
    {
        public class FeatureCard
        {
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
                "Children",
                "Disabled",
                "Education",
                "Elderly",
                "Employment",
                "Environment",
                "Health",
                "Women",
                "Youth"
            };
        }
        private static readonly List<FeatureCard> Cards = new()
{
    new FeatureCard
    {
        Title = "Education for All",
        Description = "Support learning initiatives for children in remote areas with limited resources.",
        Link = "#",
        ImageUrl = "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
        Cause = "Education",
        Partners = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Microsoft", LogoUrl = "/images/img.png" },
            new FeatureCard.OrgInfo { Name = "UNICEF", LogoUrl = "/images/img.png" }
        },
        NGOs = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Room to Read", LogoUrl = "/images/img.png" },
            new FeatureCard.OrgInfo { Name = "Save the Children", LogoUrl = "/images/img.png" }
        }
    },
    new FeatureCard
    {
        Title = "Disaster Relief",
        Description = "Join hands to provide aid during natural disasters and emergencies.",
        Link = "#",
        ImageUrl = "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
        Cause = "Children",
        Partners = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Microsoft", LogoUrl = "/images/microsoft.png" },
            new FeatureCard.OrgInfo { Name = "UNICEF", LogoUrl = "/images/unicef.png" }
        },
        NGOs = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Room to Read", LogoUrl = "/images/roomtoread.png" },
            new FeatureCard.OrgInfo { Name = "Save the Children", LogoUrl = "/images/savethechildren.png" }
        }
    },
    new FeatureCard
    {
        Title = "Clean Water Projects",
        Description = "Contribute to building sustainable water sources for underserved communities.",
        Link = "#",
        ImageUrl = "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
        Cause = "Health",
        Partners = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Microsoft", LogoUrl = "/images/microsoft.png" },
            new FeatureCard.OrgInfo { Name = "UNICEF", LogoUrl = "/images/unicef.png" }
        },
        NGOs = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Room to Read", LogoUrl = "/images/roomtoread.png" },
            new FeatureCard.OrgInfo { Name = "Save the Children", LogoUrl = "/images/savethechildren.png" }
        }
    },
     new FeatureCard
    {
        Title = "Education for All",
        Description = "Support learning initiatives for children in remote areas with limited resources.",
        Link = "#",
        ImageUrl = "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
        Cause = "Education",
        Partners = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Microsoft", LogoUrl = "/images/img.png" },
            new FeatureCard.OrgInfo { Name = "UNICEF", LogoUrl = "/images/img.png" }
        },
        NGOs = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Room to Read", LogoUrl = "/images/img.png" },
            new FeatureCard.OrgInfo { Name = "Save the Children", LogoUrl = "/images/img.png" }
        }
    },
    new FeatureCard
    {
        Title = "Disaster Relief",
        Description = "Join hands to provide aid during natural disasters and emergencies.",
        Link = "#",
        ImageUrl = "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
        Cause = "Children",
        Partners = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Microsoft", LogoUrl = "/images/microsoft.png" },
            new FeatureCard.OrgInfo { Name = "UNICEF", LogoUrl = "/images/unicef.png" }
        },
        NGOs = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Room to Read", LogoUrl = "/images/roomtoread.png" },
            new FeatureCard.OrgInfo { Name = "Save the Children", LogoUrl = "/images/savethechildren.png" }
        }
    },
    new FeatureCard
    {
        Title = "Clean Water Projects",
        Description = "Contribute to building sustainable water sources for underserved communities.",
        Link = "#",
        ImageUrl = "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
        Cause = "Health",
        Partners = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Microsoft", LogoUrl = "/images/microsoft.png" },
            new FeatureCard.OrgInfo { Name = "UNICEF", LogoUrl = "/images/unicef.png" }
        },
        NGOs = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Room to Read", LogoUrl = "/images/roomtoread.png" },
            new FeatureCard.OrgInfo { Name = "Save the Children", LogoUrl = "/images/savethechildren.png" }
        }
    },
     new FeatureCard
    {
        Title = "Education for All",
        Description = "Support learning initiatives for children in remote areas with limited resources.",
        Link = "#",
        ImageUrl = "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
        Cause = "Education",
        Partners = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Microsoft", LogoUrl = "/images/img.png" },
            new FeatureCard.OrgInfo { Name = "UNICEF", LogoUrl = "/images/img.png" }
        },
        NGOs = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Room to Read", LogoUrl = "/images/img.png" },
            new FeatureCard.OrgInfo { Name = "Save the Children", LogoUrl = "/images/img.png" }
        }
    },
    new FeatureCard
    {
        Title = "Disaster Relief",
        Description = "Join hands to provide aid during natural disasters and emergencies.",
        Link = "#",
        ImageUrl = "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
        Cause = "Children",
        Partners = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Microsoft", LogoUrl = "/images/microsoft.png" },
            new FeatureCard.OrgInfo { Name = "UNICEF", LogoUrl = "/images/unicef.png" }
        },
        NGOs = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Room to Read", LogoUrl = "/images/roomtoread.png" },
            new FeatureCard.OrgInfo { Name = "Save the Children", LogoUrl = "/images/savethechildren.png" }
        }
    },
    new FeatureCard
    {
        Title = "Clean Water Projects",
        Description = "Contribute to building sustainable water sources for underserved communities.",
        Link = "#",
        ImageUrl = "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
        Cause = "Health",
        Partners = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Microsoft", LogoUrl = "/images/microsoft.png" },
            new FeatureCard.OrgInfo { Name = "UNICEF", LogoUrl = "/images/unicef.png" }
        },
        NGOs = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Room to Read", LogoUrl = "/images/roomtoread.png" },
            new FeatureCard.OrgInfo { Name = "Save the Children", LogoUrl = "/images/savethechildren.png" }
        }
    },
     new FeatureCard
    {
        Title = "Education for All",
        Description = "Support learning initiatives for children in remote areas with limited resources.",
        Link = "#",
        ImageUrl = "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
        Cause = "Education",
        Partners = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Microsoft", LogoUrl = "/images/img.png" },
            new FeatureCard.OrgInfo { Name = "UNICEF", LogoUrl = "/images/img.png" }
        },
        NGOs = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Room to Read", LogoUrl = "/images/img.png" },
            new FeatureCard.OrgInfo { Name = "Save the Children", LogoUrl = "/images/img.png" }
        }
    },
    new FeatureCard
    {
        Title = "Disaster Relief",
        Description = "Join hands to provide aid during natural disasters and emergencies.",
        Link = "#",
        ImageUrl = "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
        Cause = "Children",
        Partners = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Microsoft", LogoUrl = "/images/microsoft.png" },
            new FeatureCard.OrgInfo { Name = "UNICEF", LogoUrl = "/images/unicef.png" }
        },
        NGOs = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Room to Read", LogoUrl = "/images/roomtoread.png" },
            new FeatureCard.OrgInfo { Name = "Save the Children", LogoUrl = "/images/savethechildren.png" }
        }
    },
    new FeatureCard
    {
        Title = "Clean Water Projects",
        Description = "Contribute to building sustainable water sources for underserved communities.",
        Link = "#",
        ImageUrl = "https://dummyimage.com/600x300/ced4da/6c757d.jpg",
        Cause = "Health",
        Partners = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Microsoft", LogoUrl = "/images/microsoft.png" },
            new FeatureCard.OrgInfo { Name = "UNICEF", LogoUrl = "/images/unicef.png" }
        },
        NGOs = new List<FeatureCard.OrgInfo>
        {
            new FeatureCard.OrgInfo { Name = "Room to Read", LogoUrl = "/images/roomtoread.png" },
            new FeatureCard.OrgInfo { Name = "Save the Children", LogoUrl = "/images/savethechildren.png" }
        }
    }

};

        public IActionResult Index(int page = 1)
        {
            const int PageSize = 9;

            var totalItems = Cards.Count;
            var pagedCards = Cards.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            ViewData["IntroTitle"] = "Together for a Better World";
            ViewData["IntroDescription"] = "This is a platform dedicated to connecting communities and donors to support impactful NGO programs across the country.";
            ViewData["IntroButtonText"] = "Support a Cause";

            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.CurrentPage = page;

            return View(pagedCards);
        }

        public IActionResult Post(int id, bool returnToAll = false)
        {
            if (id < 0 || id >= Cards.Count)
                return NotFound();

            var selectedPost = Cards[id];
            ViewData["ProgramTitle"] = selectedPost.Title;
            ViewBag.SelectedCause = selectedPost.Cause;

            ViewBag.ReturnToAll = returnToAll;

            return View("Post", selectedPost);
        }
        public IActionResult FilterPartial()
        {
            var model = new DonationFilterModel();
            return PartialView("FilterPartial", model);
        }
        public IActionResult AllCampaigns(string search, string category, int page = 1)
        {
            const int PageSize = 9;
            var allCards = Cards.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                allCards = allCards.Where(c => c.Title.Contains(search, StringComparison.OrdinalIgnoreCase) || c.Description.Contains(search));

            if (!string.IsNullOrEmpty(category))
                allCards = allCards.Where(c => c.Cause.Equals(category, StringComparison.OrdinalIgnoreCase));

            var totalItems = allCards.Count();
            var pagedCards = allCards.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Categories = Cards.Select(c => c.Cause).Distinct().ToList();

            return View("AllCampaigns", pagedCards);
        }

        public IActionResult DonationHistory()
        {
            return View(Cards); 
        }

        public IActionResult WhatWeDo()
        {
            return View();
        }
    }
}

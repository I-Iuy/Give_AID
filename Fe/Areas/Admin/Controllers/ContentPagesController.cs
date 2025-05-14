using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ContentPagesController : Controller
    {
        public class ContentPageModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
        }

        private static readonly List<ContentPageModel> SamplePages = new List<ContentPageModel>
        {
            new ContentPageModel { Id = 1, Title = "About Us", Content = "Information about the organization..." },
            new ContentPageModel { Id = 2, Title = "Mission", Content = "Our mission is to support communities in need." },
            new ContentPageModel { Id = 3, Title = "Terms of Service", Content = "These are the terms of our service..." }
        };

        public IActionResult List()
        {
            return View(SamplePages);
        }

        public IActionResult Add()
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            var page = SamplePages.FirstOrDefault(p => p.Id == id);
            if (page == null)
                return NotFound();

            return View(page);
        }
    }
}

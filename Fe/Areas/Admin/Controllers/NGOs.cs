using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NGOsController : Controller
    {
        public class NGOModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string LogoUrl { get; set; }
            public string WebsiteUrl { get; set; }
        }

        private static readonly List<NGOModel> SampleNGOs = new List<NGOModel>
        {
            new NGOModel
            {
                Id = 1,
                Name = "Red Cross",
                LogoUrl =  "/images/img.png",
                WebsiteUrl = "https://www.redcross.org"
            },
            new NGOModel
            {
                Id = 2,
                Name = "UNICEF",
                LogoUrl =  "/images/img.png",
                WebsiteUrl = "https://www.unicef.org"
            }
        };

        public IActionResult List()
        {
            return View(SampleNGOs);
        }

        public IActionResult Add()
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            var ngo = SampleNGOs.FirstOrDefault(p => p.Id == id);
            if (ngo == null)
                return NotFound();

            return View(ngo);
        }
    }
}

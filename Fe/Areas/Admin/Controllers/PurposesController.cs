using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PurposesController : Controller
    {
        public class PurposeModel
        {
            public string Name { get; set; }
            public decimal TotalDonated { get; set; }
        }

        public IActionResult List()
        {
            var purposes = new List<PurposeModel>
            {
                new PurposeModel { Name = "Natural Disaster", TotalDonated = 15000 },
                new PurposeModel { Name = "Epidemic Relief", TotalDonated = 8700 },
                new PurposeModel { Name = "Disability Support", TotalDonated = 5300 },
                new PurposeModel { Name = "Child Education", TotalDonated = 12400 }
            };

            return View(purposes);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(string name)
        {
            return RedirectToAction("List");
        }
    }
}

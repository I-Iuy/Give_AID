using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProgramsController : Controller
    {
        public class ProgramModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Purpose { get; set; }
            public string Content { get; set; }
            public string VideoUrl { get; set; }
            public decimal TotalDonated { get; set; }
        }

        private static readonly List<ProgramModel> SamplePrograms = new List<ProgramModel>
        {
            new ProgramModel
            {
                Id = 1,
                Name = "Flood Relief 2024",
                Purpose = "Natural Disaster",
                Content = "Supporting communities affected by floods.",
                VideoUrl = "https://example.com/flood",
                TotalDonated = 15000
            },
            new ProgramModel
            {
                Id = 2,
                Name = "Covid-19 Support",
                Purpose = "Pandemic",
                Content = "Helping communities affected by Covid-19.",
                VideoUrl = "https://example.com/covid",
                TotalDonated = 22000
            }
        };

        public IActionResult List()
        {
            return View(SamplePrograms);
        }

        public IActionResult Add()
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            var program = SamplePrograms.FirstOrDefault(p => p.Id == id);
            if (program == null)
            {
                return NotFound();
            }

            return View(program);
        }
    }
}

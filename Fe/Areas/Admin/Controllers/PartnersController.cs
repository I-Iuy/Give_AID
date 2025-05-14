using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PartnersController : Controller
    {
        public class PartnerModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string LogoUrl { get; set; }
            public string ContractFile { get; set; }
        }

        private static readonly List<PartnerModel> SamplePartners = new List<PartnerModel>
        {
            new PartnerModel
            {
                Id = 1,
                Name = "Red Cross",
                LogoUrl =  "/images/img.png",
                ContractFile = "contract_redcross.pdf"
            },
            new PartnerModel
            {
                Id = 2,
                Name = "UNICEF",
                LogoUrl =  "/images/img.png",
                ContractFile = "contract_unicef.pdf"
            }
        };

        public IActionResult List()
        {
            return View(SamplePartners);
        }

        public IActionResult Add()
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            var partner = SamplePartners.FirstOrDefault(p => p.Id == id);
            if (partner == null)
                return NotFound();

            return View(partner);
        }
    }
}

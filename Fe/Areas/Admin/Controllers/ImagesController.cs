using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ImagesController : Controller
    {
        public class ImageModel
        {
            public string Title { get; set; }
            public string Url { get; set; }
        }

        public IActionResult List()
        {
            var images = new List<ImageModel>
            {
                new ImageModel { Title = "Flood Relief", Url = "/images/img.png" },
                new ImageModel { Title = "Covid Aid", Url = "/images/img.png" },
                new ImageModel { Title = "Education Support", Url = "/images/img.png" }
            };

            return View(images);
        }
    }
}

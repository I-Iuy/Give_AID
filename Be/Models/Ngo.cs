using System.ComponentModel.DataAnnotations;

namespace Be.Models
{
    public class Ngo
    {
        [Key]
        public int NgoId { get; set; } // Pk, auto-increment
        public string Name { get; set; }
        public string LogoUrl { get; set; } // Upload Img Logo .png, .svg, < 500KB
        public string WebsiteUrl { get; set; } 
        public int AccountId { get; set; } // FK
    }
}

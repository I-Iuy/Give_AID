using System.ComponentModel.DataAnnotations;

namespace Be.Models
{
    public class Ngo
    {
        [Key]
        public int NgoId { get; set; } // tự động tăng
        public string Name { get; set; }
        public string LogoUrl { get; set; } // Upload hình ảnh logo .png, .svg, < 500KB
        public string WebsiteUrl { get; set; } // Đường dẫn đến website của tổ chức
        public int AccountId { get; set; } // khóa ngoại
    }
}

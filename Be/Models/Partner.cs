using System.ComponentModel.DataAnnotations;

namespace Be.Models
{
    public class Partner
    {
        [Key]
        public int PartnerId { get; set; } // tự động tăng
        public string Name { get; set; }
        public string LogoUrl { get; set; } // Upload hình ảnh logo .png, .svg, < 500KB
        public string ContractFile { get; set; } // Upload file .docx, .pdf, < 5MB
        public int AccountId { get; set; } // khóa ngoại

    }
}

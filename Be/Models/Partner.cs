using System.ComponentModel.DataAnnotations;

namespace Be.Models
{
    public class Partner
    {
        [Key]
        public int PartnerId { get; set; } // PK, auto-increment
        public string Name { get; set; }
        public string LogoUrl { get; set; } // Upload Img Logo .png, .svg, < 500KB
        public string ContractFile { get; set; } // Upload File .docx, .pdf, < 5MB
        public int AccountId { get; set; } // FK

    }
}

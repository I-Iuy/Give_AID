using System.ComponentModel.DataAnnotations;

namespace Be.Models
{
    public class Share
    {
        [Key]
        public int ShareId { get; set; }

        [Required]
        public int CampaignId { get; set; }

        public int? AccountId { get; set; }           // Người đăng nhập
        public string? GuestName { get; set; }        // Khách chưa đăng nhập

        public string? ReceiverEmail { get; set; }    // Nếu là Email

        [Required]
        public string Platform { get; set; } = "Email"; // Email, Facebook, WhatsApp, etc.

        public DateTime SharedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Campaign Campaign { get; set; } = null!;
        public Account? Account { get; set; }
    }
}

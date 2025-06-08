using System.ComponentModel.DataAnnotations;

namespace Be.Models
{
    public class Share
    {
        [Key]
        public int ShareId { get; set; }

        [Required]
        public int CampaignId { get; set; }

        public int? AccountId { get; set; }           // Logged in user
        public string? GuestName { get; set; }        // Guest user

        public string? ReceiverEmail { get; set; }    // For email sharing

        [Required]
        public string Platform { get; set; } = "Email"; // Email, Facebook, WhatsApp, etc.

        public DateTime SharedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Campaign Campaign { get; set; } = null!;
        public Account? Account { get; set; }
    }
}
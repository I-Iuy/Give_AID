using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Be.Models;

namespace Be.Models
{
    public class UserNotification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int AccountId { get; set; } // Người nhận thông báo

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CampaignId { get; set; }

        // Navigation
        [ForeignKey("AccountId")]
        public Account Account { get; set; } = null!;

        [ForeignKey("CampaignId")]
        public Campaign? Campaign { get; set; }
    }
}

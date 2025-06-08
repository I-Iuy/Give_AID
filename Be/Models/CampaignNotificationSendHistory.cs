using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Be.Models
{
    public class CampaignNotificationSendHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Optional: Link to Campaign if needed, otherwise store title directly
        public int? CampaignId { get; set; }

        [Required]
        public string CampaignTitle { get; set; } = string.Empty; // Store campaign title directly

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string Status { get; set; } = string.Empty; // e.g., "Success", "Failure"

        public string? ErrorMessage { get; set; }

        // Optional: Navigation property if CampaignId is used
        // public Campaign? Campaign { get; set; }
    }
} 
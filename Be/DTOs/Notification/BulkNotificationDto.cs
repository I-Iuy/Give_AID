using System.ComponentModel.DataAnnotations;

namespace Be.DTOs.Notification
{
    public class BulkNotificationDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Message must be between 3 and 500 characters")]
        public string Message { get; set; } = string.Empty;

        public int? CampaignId { get; set; }  // Gắn theo chiến dịch nếu cần
    }
}

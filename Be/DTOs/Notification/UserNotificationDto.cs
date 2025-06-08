namespace Be.DTOs.Notification
{
    public class UserNotificationDto
    {
        public int NotificationId { get; set; }
        public int AccountId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CampaignId { get; set; }
        public string? CampaignTitle { get; set; }
    }
}

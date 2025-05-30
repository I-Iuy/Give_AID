namespace Fe.DTOs.Notification
{
    public class BulkNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? CampaignId { get; set; }
    }
}

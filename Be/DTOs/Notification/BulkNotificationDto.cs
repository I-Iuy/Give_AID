namespace Be.DTOs.Notification
{
    public class BulkNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? CampaignId { get; set; }  // Gắn theo chiến dịch nếu cần
    }
}

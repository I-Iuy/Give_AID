namespace Be.DTOs.Notification
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int AccountId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
} 
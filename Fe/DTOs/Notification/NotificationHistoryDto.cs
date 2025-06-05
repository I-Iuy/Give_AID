namespace Fe.DTOs.Notification
{
    public class NotificationHistoryDto
    {
        public int NotificationId { get; set; }
        public int AccountId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string AccountEmail { get; set; } = string.Empty;
    }
} 
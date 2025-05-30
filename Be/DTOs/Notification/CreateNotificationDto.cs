namespace Be.DTOs.Notification
{
    public class CreateNotificationDto
    {
        public int AccountId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

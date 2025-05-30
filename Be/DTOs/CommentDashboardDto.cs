namespace Be.DTOs
{
    public class CommentDashboardDto
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public int CampaignId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsReplied { get; set; }
    }
}

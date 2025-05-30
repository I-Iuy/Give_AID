namespace Fe.DTOs.Comment
{
    public class CreateCommentDto
    {
        public string Content { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; } = false;
        public int? AccountId { get; set; }
        public string? GuestName { get; set; }
        public int CampaignId { get; set; }
        public int? ParentCommentId { get; set; }
    }
}

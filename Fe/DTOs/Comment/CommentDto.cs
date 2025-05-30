namespace Fe.DTOs.Comment
{
    public class CommentDto
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; }
        public DateTime CommentedAt { get; set; }

        public int? AccountId { get; set; }
        public string? GuestName { get; set; }

        public int CampaignId { get; set; }
        public int? ParentCommentId { get; set; }

        public bool IsReplied { get; set; }
    }
}

namespace Be.DTOs.Comment
{
    public class CreateCommentDto
    {
        public string Content { get; set; }
        public bool IsAnonymous { get; set; } = false;
        public int? AccountId { get; set; }
        public string? GuestName { get; set; }
        public int CampaignId { get; set; } // ✅
        public int? ParentCommentId { get; set; }
    }

}

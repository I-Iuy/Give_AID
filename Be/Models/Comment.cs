using Be.Models;

namespace Be.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; } = false;
        public DateTime CommentedAt { get; set; } = DateTime.UtcNow;

        public int? AccountId { get; set; }
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; } // dùng để admin liên hệ nếu cần

        public int CampaignId { get; set; } 

        public int? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }
        public ICollection<Comment>? Replies { get; set; }

        public Account? Account { get; set; }
        public Campaign Campaign { get; set; } = null!;
    }
}
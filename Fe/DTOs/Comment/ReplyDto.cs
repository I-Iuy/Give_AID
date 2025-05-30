namespace Fe.DTOs.Comment
{
    public class ReplyDto
    {
        public int CommentId { get; set; }
        public string ReplyContent { get; set; } = string.Empty;
    }
}

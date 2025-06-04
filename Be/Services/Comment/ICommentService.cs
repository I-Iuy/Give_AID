using Be.DTOs.Comment;

namespace Be.Services.Comment
{
    public interface ICommentService
    {
        Task<CommentDto> AddCommentAsync(CreateCommentDto dto);
        Task<IEnumerable<CommentDto>> GetCommentsByCampaignAsync(int campaignId);
        Task DeleteCommentAsync(int commentId);
        Task ReplyToCommentAsync(int commentId, string replyContent);
        Task<IEnumerable<CommentDashboardDto>> GetAllForDashboardAsync();
        Task<CommentDto?> GetByIdAsync(int commentId);
    }
}

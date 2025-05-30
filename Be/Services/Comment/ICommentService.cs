using Be.DTOs;

namespace Be.Services.Comment
{
    public interface ICommentService
    {
        Task AddCommentAsync(CreateCommentDto dto);
        Task<IEnumerable<CommentDto>> GetCommentsByCampaignAsync(int campaignId);
        Task<IEnumerable<CommentDashboardDto>> GetAllForDashboardAsync(); // cần cho dashboard
        Task ReplyToCommentAsync(int commentId, string replyContent);     // ✅ thêm dòng này
        Task DeleteCommentAsync(int commentId);                           // ✅ thêm dòng này
    }
}

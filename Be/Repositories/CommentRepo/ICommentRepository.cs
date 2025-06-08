using Be.DTOs.Comment;
using Be.Models;

namespace Be.Repositories.CommentRepo
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<IEnumerable<Comment>> GetCommentsByCampaignAsync(int campaignId);
        Task<Comment?> GetByIdAsync(int commentId);
        Task SaveCommentAsync(Comment comment);
        void Delete(Comment comment);
        Task SaveChangesAsync();
        Task<IEnumerable<Comment>> GetRecentCommentsAsync(int? accountId, string guestName);
        Task<IEnumerable<CommentDashboardDto>> GetAllForDashboardAsync();
    }
}

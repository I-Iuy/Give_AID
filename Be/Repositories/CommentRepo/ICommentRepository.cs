using Be.Models;

namespace Be.Repositories.CommentRepo
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<IEnumerable<Comment>> GetCommentsByCampaignAsync(int campaignId);
        Task<Comment?> GetByIdAsync(int commentId);
        Task AddCommentAsync(Comment comment);
        void Delete(Comment comment);
        Task SaveChangesAsync();
        Task<IEnumerable<Comment>> GetRecentCommentsAsync(int? accountId, string guestName);
    }
}

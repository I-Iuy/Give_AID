using Be.Models;
using Microsoft.EntityFrameworkCore;

namespace Be.Repositories.CommentRepo
{
    public class CommentRepository : ICommentRepository
    {
        private readonly DatabaseContext context;

        public CommentRepository(DatabaseContext context)
        {
            this.context = context;
        }

        // ✅ Thêm comment mới
        public async Task AddCommentAsync(Comment comment)
        {
            await context.Comments.AddAsync(comment);
        }

        // ✅ Xoá comment
        public void Delete(Comment comment)
        {
            context.Comments.Remove(comment);
        }

        // ✅ Lưu thay đổi
        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }

        // ✅ Lấy tất cả comment (cho admin dashboard)
        public async Task<IEnumerable<Comment>> GetAllAsync()
        {
            return await context.Comments
                .Include(c => c.Replies)
                .ToListAsync();
        }

        // ✅ Lấy danh sách comment theo campaign
        public async Task<IEnumerable<Comment>> GetCommentsByCampaignAsync(int campaignId)
        {
            return await context.Comments
                .Include(c => c.Replies)
                .Where(c => c.CampaignId == campaignId)
                .ToListAsync();
        }

        // ✅ Lấy comment theo Id
        public async Task<Comment?> GetByIdAsync(int commentId)
        {
            return await context.Comments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.CommentId == commentId);
        }

        public async Task<IEnumerable<Comment>> GetRecentCommentsAsync(int? accountId, string guestName)
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            
            return await context.Comments
                .Where(c => 
                    (accountId.HasValue && c.AccountId == accountId) || 
                    (!string.IsNullOrEmpty(guestName) && c.GuestName == guestName))
                .Where(c => c.CommentedAt >= oneHourAgo)
                .ToListAsync();
        }
    }
}

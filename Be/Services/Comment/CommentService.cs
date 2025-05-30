using Be.DTOs;
using Be.Models;
using Be.Repositories.CommentRepo;
using CommentModel = Be.Models.Comment;

namespace Be.Services.Comment
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        // ✅ Thêm bình luận mới
        public async Task AddCommentAsync(CreateCommentDto dto)
        {
            var comment = new CommentModel
            {
                Content = dto.Content,
                IsAnonymous = dto.IsAnonymous,
                AccountId = dto.AccountId,
                GuestName = dto.GuestName,
                CampaignId = dto.CampaignId,
                ParentCommentId = dto.ParentCommentId,
                CommentedAt = DateTime.UtcNow
            };

            await _commentRepository.AddCommentAsync(comment);
            await _commentRepository.SaveChangesAsync();
        }

        // ✅ Lấy bình luận theo chiến dịch (chỉ bình luận cha)
        public async Task<IEnumerable<CommentDto>> GetCommentsByCampaignAsync(int campaignId)
        {
            var comments = await _commentRepository.GetCommentsByCampaignAsync(campaignId);

            return comments
                .Where(c => c.ParentCommentId == null)
                .Select(c => new CommentDto
                {
                    CommentId = c.CommentId,
                    Content = c.Content,
                    IsAnonymous = c.IsAnonymous,
                    CommentedAt = c.CommentedAt,
                    AccountId = c.AccountId,
                    GuestName = c.GuestName,
                    CampaignId = c.CampaignId,
                    ParentCommentId = c.ParentCommentId,
                    IsReplied = c.Replies != null && c.Replies.Count > 0
                });
        }

        // ✅ Xoá comment (và xoá luôn các phản hồi nếu có)
        public async Task DeleteCommentAsync(int commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
                throw new Exception("Comment không tồn tại.");

            if (comment.Replies?.Count > 0)
            {
                foreach (var reply in comment.Replies.ToList())
                {
                    _commentRepository.Delete(reply);
                }
            }

            _commentRepository.Delete(comment);
            await _commentRepository.SaveChangesAsync();
        }

        // ✅ Admin phản hồi comment
        public async Task ReplyToCommentAsync(int commentId, string replyContent)
        {
            var parent = await _commentRepository.GetByIdAsync(commentId);
            if (parent == null)
                throw new Exception("Không tìm thấy comment để phản hồi.");

            var reply = new CommentModel
            {
                Content = replyContent,
                IsAnonymous = false,
                AccountId = null,
                GuestName = "Admin",
                CampaignId = parent.CampaignId,
                ParentCommentId = parent.CommentId,
                CommentedAt = DateTime.UtcNow
            };

            await _commentRepository.AddCommentAsync(reply);
            await _commentRepository.SaveChangesAsync();
        }

        // ✅ Lấy bình luận cho Dashboard admin
        public async Task<IEnumerable<CommentDashboardDto>> GetAllForDashboardAsync()
        {
            var comments = await _commentRepository.GetAllAsync();

            return comments
                .Where(c => c.ParentCommentId == null)
                .Select(c => new CommentDashboardDto
                {
                    CommentId = c.CommentId,
                    Content = c.Content,
                    GuestName = c.GuestName ?? "Khách",
                    CampaignId = c.CampaignId,
                    CreatedAt = c.CommentedAt,
                    IsReplied = c.Replies?.Count > 0

                });
        }
    }
}

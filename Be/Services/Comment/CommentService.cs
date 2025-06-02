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

        private void ValidateCommentContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new Exception("Nội dung comment không được để trống");
            }

            if (content.Length > 1000)
            {
                throw new Exception("Nội dung comment không được vượt quá 1000 ký tự");
            }

            // Kiểm tra từ khóa nhạy cảm
            var sensitiveWords = new[] { "spam", "advertisement", "quảng cáo" };
            if (sensitiveWords.Any(word => content.ToLower().Contains(word)))
            {
                throw new Exception("Nội dung comment chứa từ khóa không được phép");
            }
        }

        // ✅ Thêm bình luận mới
        public async Task AddCommentAsync(CreateCommentDto dto)
        {
            // Validate nội dung
            ValidateCommentContent(dto.Content);

            // Kiểm tra rate limiting (tối đa 5 comment trong 1 giờ)
            var recentComments = await _commentRepository.GetRecentCommentsAsync(
                dto.AccountId ?? 0, 
                dto.GuestName ?? ""
            );
            if (recentComments.Count() >= 5)
            {
                throw new Exception("Bạn đã comment quá nhiều trong thời gian ngắn. Vui lòng thử lại sau.");
            }

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
                    IsReplied = c.Replies != null && c.Replies.Count > 0,
                    Replies = c.Replies?
                        .Select(r => new CommentDto
                        {
                            CommentId = r.CommentId,
                            Content = r.Content,
                            IsAnonymous = r.IsAnonymous,
                            CommentedAt = r.CommentedAt,
                            AccountId = r.AccountId,
                            GuestName = r.GuestName,
                            CampaignId = r.CampaignId,
                            ParentCommentId = r.ParentCommentId,
                            IsReplied = false,
                            Replies = null
                        })
                        .ToList()
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
using Be.DTOs.Comment;
using Be.Models;
using Be.Repositories.CommentRepo;
using CommentModel = Be.Models.Comment;
using System.Text.RegularExpressions;

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
                throw new Exception("Comment content cannot be empty");
            }

            // Check if content is only whitespace
            if (content.Trim().Length == 0)
            {
                throw new Exception("Comment content cannot contain only spaces");
            }

            // Check length
            if (content.Length > 1000)
            {
                throw new Exception("Comment content cannot exceed 1000 characters");
            }

            if (content.Length < 3)
            {
                throw new Exception("Comment content must be at least 3 characters long");
            }

            // Check for sensitive keywords in multiple languages
            var sensitiveWords = new Dictionary<string, string[]>
            {
                ["en"] = new[] { "spam", "advertisement", "sex", "porn", "xxx", "18+", "hack", "crack", "virus", "scam", "fraud", "cheat" },
                ["vi"] = new[] {
                    // Basic words
                    "địt", "đụ", "đéo", "đcm", "đkm", "đmm", "đít", "đụ", "đéo", "đcm", "đkm", "đmm",
                    // Common phrases
                    "địt mẹ", "đụ má", "đụ mẹ", "đụ cha", "đụ bố", "đụ ông", "đụ bà", "đụ cô", "đụ chú", "đụ dì",
                    // Variations
                    "quảng cáo", "khiêu dâm", "lừa đảo", "gian lận", "đồi trụy", "dâm ô", "bậy bạ", "thô tục", "tục tĩu",
                    // Additional common words
                    "đụ", "đụ má", "đụ mẹ", "đụ cha", "đụ bố", "đụ ông", "đụ bà", "đụ cô", "đụ chú", "đụ dì",
                    "đụ anh", "đụ chị", "đụ em", "đụ cháu", "đụ con", "đụ vợ", "đụ chồng",
                    "đụ gia đình", "đụ họ hàng", "đụ tổ tiên", "đụ ông bà", "đụ cha mẹ",
                    "đụ anh em", "đụ chị em", "đụ cô chú", "đụ dì dượng", "đụ cậu mợ",
                    "đụ chú thím", "đụ bác mợ", "đụ ông bà nội", "đụ ông bà ngoại",
                    "đụ cha mẹ vợ", "đụ cha mẹ chồng", "đụ vợ chồng", "đụ con cái",
                    "đụ cháu chắt", "đụ họ hàng", "đụ làng xóm", "đụ hàng xóm",
                    "đụ bạn bè", "đụ đồng nghiệp", "đụ đồng đội", "đụ đồng bào",
                    "đụ đồng hương", "đụ đồng môn"
                },
                ["zh"] = new[] { "广告", "色情", "病毒", "诈骗" },
                ["ja"] = new[] { "広告", "ポルノ", "ウイルス", "詐欺" },
                ["ko"] = new[] { "광고", "포르노", "바이러스", "사기" },
                ["th"] = new[] { "โฆษณา", "อนาจาร", "ไวรัส", "การฉ้อโกง" },
                ["id"] = new[] { "iklan", "pornografi", "virus", "penipuan" },
                ["ms"] = new[] { "iklan", "pornografi", "virus", "penipuan" },
                ["tl"] = new[] { "anunsyo", "pornograpiya", "virus", "panloloko" },
                ["hi"] = new[] { "विज्ञापन", "अश्लील", "वायरस", "धोखाधड़ी" }
            };

            // Normalize Vietnamese text by removing diacritics
            string NormalizeVietnamese(string text)
            {
                string normalized = text.Normalize(System.Text.NormalizationForm.FormD);
                var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
                return new string(chars).Normalize(System.Text.NormalizationForm.FormC);
            }

            var contentLower = content.ToLower();
            var normalizedContent = NormalizeVietnamese(contentLower);

            // Check for sensitive words
            foreach (var languageWords in sensitiveWords.Values)
            {
                foreach (var word in languageWords)
                {
                    var wordLower = word.ToLower();
                    var normalizedWord = NormalizeVietnamese(wordLower);

                    // Check both original and normalized versions
                    if (contentLower.Contains(wordLower) || normalizedContent.Contains(normalizedWord))
                    {
                        throw new Exception("Comment contains prohibited keywords");
                    }
                }
            }

            // Check for word combinations
            var words = contentLower.Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '-', '_', '/', '\\', '|', '(', ')', '[', ']', '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                var normalizedWord = NormalizeVietnamese(word);
                if (sensitiveWords.Values.Any(words => words.Any(w => NormalizeVietnamese(w.ToLower()) == normalizedWord)))
                {
                    throw new Exception("Comment contains prohibited keywords");
                }
            }

            // Check for excessive special characters
            var specialCharCount = content.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
            if (specialCharCount > content.Length * 0.3) // More than 30% special characters
            {
                throw new Exception("Comment contains too many special characters");
            }

            // Check for repeated characters
            if (Regex.IsMatch(content, @"(.)\1{4,}")) // 5 or more repeated characters
            {
                throw new Exception("Comment contains too many repeated characters");
            }

            // Check for URLs
            if (Regex.IsMatch(content, @"https?://\S+|www\.\S+"))
            {
                throw new Exception("URLs are not allowed in comments");
            }

            // Check for email addresses
            if (Regex.IsMatch(content, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}"))
            {
                throw new Exception("Email addresses are not allowed in comments");
            }

            // Check for maximum length
            if (content.Length > 500)
            {
                throw new Exception("Comment cannot exceed 500 characters");
            }
        }

        // Add new comment
        public async Task<CommentDto> AddCommentAsync(CreateCommentDto dto)
        {
            Console.WriteLine($"[BE Service] Received AddCommentAsync call.");
            Console.WriteLine($"[BE Service] DTO: CampaignId={dto.CampaignId}, Content='{dto.Content}', IsAnonymous={dto.IsAnonymous}, AccountId={dto.AccountId}, GuestName='{dto.GuestName}'");

            // Validate content
            Console.WriteLine($"[BE Service] Validating content...");
            ValidateCommentContent(dto.Content);
            Console.WriteLine($"[BE Service] Content validation successful.");

            // Check rate limiting (max 5 comments per hour)
            Console.WriteLine($"[BE Service] Checking rate limiting...");
            var recentComments = await _commentRepository.GetRecentCommentsAsync(
                dto.AccountId ?? 0,
                dto.GuestName ?? ""
            );
            if (recentComments.Count() >= 5)
            {
                Console.WriteLine($"[BE Service] Rate limit exceeded.");
                throw new Exception("You have commented too many times in a short period. Please try again later.");
            }
            Console.WriteLine($"[BE Service] Rate limiting check passed.");

            var comment = new CommentModel
            {
                Content = dto.Content,
                IsAnonymous = dto.IsAnonymous,
                AccountId = dto.AccountId,
                GuestName = dto.IsAnonymous ? null : dto.GuestName, // Only set GuestName if not anonymous
                CampaignId = dto.CampaignId,
                ParentCommentId = dto.ParentCommentId,
                CommentedAt = DateTime.Now // Use local time instead of UTC
            };

            Console.WriteLine($"[BE Service] Created comment object: {System.Text.Json.JsonSerializer.Serialize(comment)}");

            try
            {
                await _commentRepository.SaveCommentAsync(comment);
                Console.WriteLine($"[BE Service] Added comment to repository.");
                await _commentRepository.SaveChangesAsync();
                Console.WriteLine($"[BE Service] Saved changes to database.");

                var commentDto = new CommentDto
                {
                    CommentId = comment.CommentId,
                    Content = comment.Content,
                    IsAnonymous = comment.IsAnonymous,
                    CommentedAt = comment.CommentedAt,
                    AccountId = comment.AccountId,
                    GuestName = comment.GuestName,
                    CampaignId = comment.CampaignId,
                    ParentCommentId = comment.ParentCommentId,
                    IsReplied = false
                };

                Console.WriteLine($"[BE Service] Returning new comment: {System.Text.Json.JsonSerializer.Serialize(commentDto)}");
                return commentDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BE Service] Error saving comment: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByCampaignAsync(int campaignId)
        {
            Console.WriteLine($"[BE Service] Getting comments for campaign {campaignId}");
            var comments = await _commentRepository.GetCommentsByCampaignAsync(campaignId);
            Console.WriteLine($"[BE Service] Found {comments.Count()} comments");
            Console.WriteLine($"[BE Service] Raw comments: {System.Text.Json.JsonSerializer.Serialize(comments, new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve })}");

            foreach (var comment in comments)
            {
                Console.WriteLine($"[BE Service] Raw Comment ID: {comment.CommentId}, Content: '{comment.Content}'");
                if (comment.Replies != null)
                {
                    foreach (var reply in comment.Replies)
                    {
                        Console.WriteLine($"[BE Service] Raw Reply ID: {reply.CommentId}, Content: '{reply.Content}'");
                    }
                }
            }

            Console.WriteLine($"[BE Service] GetCommentsByCampaignAsync: Comments count from repository: {comments.Count()}");

            var result = comments
                .Where(c => c.ParentCommentId == null)
                .OrderByDescending(c => c.CommentedAt)
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
                        .OrderByDescending(r => r.CommentedAt)
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
                })
                .ToList();

            Console.WriteLine($"[BE Service] Returning {result.Count} parent comments with their replies");
            Console.WriteLine($"[BE Service] Processed comments: {System.Text.Json.JsonSerializer.Serialize(result)}");

            foreach (var commentDto in result)
            {
                Console.WriteLine($"[BE Service] Processed CommentDto ID: {commentDto.CommentId}, Content: '{commentDto.Content}'");
                if (commentDto.Replies != null)
                {
                    foreach (var replyDto in commentDto.Replies)
                    {
                        Console.WriteLine($"[BE Service] Processed ReplyDto ID: {replyDto.CommentId}, Content: '{replyDto.Content}'");
                    }
                }
            }

            return result;
        }

        // Delete comment (and its replies if any)
        public async Task DeleteCommentAsync(int commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
                throw new Exception("Comment does not exist.");

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

        // Admin reply to comment
        public async Task ReplyToCommentAsync(int commentId, string replyContent)
        {
            var parent = await _commentRepository.GetByIdAsync(commentId);
            if (parent == null)
                throw new Exception("Comment not found for reply.");

            var reply = new CommentModel
            {
                Content = replyContent,
                IsAnonymous = false,
                AccountId = null,
                GuestName = "Admin",
                CampaignId = parent.CampaignId,
                ParentCommentId = parent.CommentId,
                CommentedAt = DateTime.Now
            };

            await _commentRepository.SaveCommentAsync(reply);
            await _commentRepository.SaveChangesAsync();
        }

        // Get comments for admin dashboard
        public async Task<IEnumerable<CommentDashboardDto>> GetAllForDashboardAsync()
        {
            Console.WriteLine($"[BE Service] Getting all comments for dashboard");
            Console.WriteLine($"[BE Service] -> Calling _commentRepository.GetAllForDashboardAsync()");
            var comments = await _commentRepository.GetAllForDashboardAsync();

            Console.WriteLine($"[BE Service] GetAllForDashboardAsync: Comments count from repository: {comments.Count()}");
            Console.WriteLine($"[BE Service] Found {comments.Count()} comments");
            Console.WriteLine($"[BE Service] Returning {comments.Count()} comments for dashboard");
            return comments;
        }

        // Get comment by ID
        public async Task<CommentDto?> GetByIdAsync(int commentId)
        {
            Console.WriteLine($"[BE Service] Getting comment by ID: {commentId}");
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                Console.WriteLine($"[BE Service] Comment with ID {commentId} not found in repository.");
                return null;
            }

            var commentDto = new CommentDto
            {
                CommentId = comment.CommentId,
                Content = comment.Content,
                IsAnonymous = comment.IsAnonymous,
                CommentedAt = comment.CommentedAt,
                AccountId = comment.AccountId,
                GuestName = comment.GuestName,
                CampaignId = comment.CampaignId,
                ParentCommentId = comment.ParentCommentId,
                IsReplied = comment.Replies?.Count > 0,
                Replies = comment.Replies?.Select(r => new CommentDto
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
                }).ToList()
            };

            Console.WriteLine($"[BE Service] Returning comment DTO for ID {commentId}: {System.Text.Json.JsonSerializer.Serialize(commentDto)}");
            return commentDto;
        }
    }
}
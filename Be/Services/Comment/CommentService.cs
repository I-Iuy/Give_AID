using Be.DTOs.Comment;
using Be.Models;
using Be.Repositories.CommentRepo;
using CommentModel = Be.Models.Comment;
using System.Text.RegularExpressions;

namespace Be.Services.Comment
{
    public class BadWordDetector
    {
        private readonly HashSet<string> _badWords;

        public BadWordDetector()
        {
            _badWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddWords(IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                _badWords.Add(NormalizeVietnamese(word.ToLower()));
            }
        }

        public bool ContainsAnyBadWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var normalizedText = NormalizeVietnamese(text.ToLower());

            foreach (var badWord in _badWords)
            {
                // Check if the sensitive word appears anywhere, no need for \b
                var pattern = Regex.Escape(badWord);
                if (Regex.IsMatch(normalizedText, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    return true;
            }
            return false;
        }

        private string NormalizeVietnamese(string text)
        {
            string normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(System.Text.NormalizationForm.FormC);
        }
    }

    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly BadWordDetector _badWordDetector;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
            _badWordDetector = new BadWordDetector();
            
            // Add custom sensitive words
            _badWordDetector.AddWords(new[] {
                // Essential Vietnamese profanity
                "địt", "đụ", "đéo", "đcm", "đkm", "đmm", "đít", "cặc", "lồn",
                "đụ má", "đụ mẹ", "đụ cha", "đụ bố",
                
                // English profanity - basic
                "fuck", "shit", "bitch", "ass", "dick", "cock", "pussy", "cunt",
                "motherfucker", "fucker", "bullshit", "dumbass", "asshole",
                
                // English profanity - variations
                "f*ck", "f**k", "f***", "sh*t", "s*it", "s**t", "b*tch", "b**ch",
                "a**", "a*s", "d*ck", "d**k", "p*ssy", "p**sy", "c*nt", "c**t",
                
                // English sexual content
                "porn", "pornography", "sex", "sexual", "nude", "naked",
                "p*rn", "s*x", "n*de",
                
                // English violence
                "kill", "death", "blood", "violence", "weapon", "gun", "bomb",
                "terrorist", "attack", "beat", "abuse",
                
                // English discrimination
                "racist", "sexist", "homophobic", "transphobic", "prejudice",
                
                // English spam
                "scam", "fraud", "fake", "hack", "crack", "illegal", "unauthorized"
            });
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

            if (content.Length < 2)
            {
                throw new Exception("Comment content must be at least 2 characters long");
            }

            // Check for inappropriate language using BadWordDetector
            if (_badWordDetector.ContainsAnyBadWords(content))
            {
                throw new Exception("Comment contains inappropriate language");
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
        }

        private string? ValidateGuestName(string? guestName, bool isAnonymous, int? accountId)
        {
            // Nếu người dùng đã đăng nhập (có AccountId), không cần validate GuestName
            if (accountId.HasValue)
            {
                return guestName;
            }

            // Nếu chọn ẩn danh, không cần validate GuestName
            if (isAnonymous)
            {
                return guestName;
            }

            // Nếu không chọn ẩn danh và chưa đăng nhập, GuestName là bắt buộc
            if (string.IsNullOrWhiteSpace(guestName))
            {
                return "Guest";
            }

            // Kiểm tra độ dài tối thiểu
            if (guestName.Trim().Length < 2)
            {
                throw new Exception("Name must be at least 2 characters long");
            }

            // Kiểm tra độ dài tối đa
            if (guestName.Length > 100)
            {
                throw new Exception("Name cannot exceed 100 characters");
            }
            return guestName;
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

            // Validate guest name
            Console.WriteLine($"[BE Service] Validating guest name...");
            dto.GuestName = ValidateGuestName(dto.GuestName, dto.IsAnonymous, dto.AccountId);
            Console.WriteLine($"[BE Service] Guest name validation successful.");

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
                GuestName = dto.IsAnonymous ? null : dto.GuestName, // Chỉ set GuestName thành null khi ẩn danh
                CampaignId = dto.CampaignId,
                ParentCommentId = dto.ParentCommentId,
                CommentedAt = DateTime.Now // Sử dụng giờ địa phương thay vì UTC
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
                    FullName = comment.Account?.FullName,
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
                    FullName = c.Account?.FullName,
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
                            FullName = r.Account?.FullName,
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
                FullName = comment.Account?.FullName,
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
                    FullName = r.Account?.FullName,
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
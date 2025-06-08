using Be.DTOs.Comment;
using Be.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Be.Repositories.CommentRepo
{
    public class CommentRepository : ICommentRepository
    {
        private readonly DatabaseContext context;

        public CommentRepository(DatabaseContext context)
        {
            this.context = context;
        }

        // Add new comment
        public async Task SaveCommentAsync(Comment comment)
        {
            try
            {
                Console.WriteLine($"[BE Repository] Starting to save comment: {JsonSerializer.Serialize(comment)}");
                
                // Validate Campaign
                var campaign = await context.Campaigns.FindAsync(comment.CampaignId);
                if (campaign == null)
                {
                    throw new Exception($"Campaign with ID {comment.CampaignId} not found");
                }

                // Validate Account if provided
                if (comment.AccountId.HasValue)
                {
                    var account = await context.Accounts.FindAsync(comment.AccountId.Value);
                    if (account == null)
                    {
                        throw new Exception($"Account with ID {comment.AccountId} not found");
                    }
                }

                // Set timestamp
                if (comment.CommentedAt == default)
                {
                    comment.CommentedAt = DateTime.UtcNow;
                }

                context.Comments.Add(comment);
                context.Entry(comment).State = EntityState.Added;

                Console.WriteLine($"[BE Repository] Comment added to context. State: {context.Entry(comment).State}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BE Repository] Error saving comment: {ex.Message}");
                Console.WriteLine($"[BE Repository] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // Delete comment
        public void Delete(Comment comment)
        {
            context.Comments.Remove(comment);
        }

        // Save changes to database
        public async Task SaveChangesAsync()
        {
            Console.WriteLine($"[BE Repository] Saving changes to database");
            try
            {
                var affectedRows = await context.SaveChangesAsync();
                Console.WriteLine($"[BE Repository] Changes saved. Affected rows: {affectedRows}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BE Repository] Error during SaveChanges: {ex.Message}");
                Console.WriteLine($"[BE Repository] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // Get all comments (for admin dashboard)
        public async Task<IEnumerable<Comment>> GetAllAsync()
        {
            Console.WriteLine($"[BE Repository] Getting all comments");
            var comments = await context.Comments
                .Include(c => c.Replies)
                .Include(c => c.Campaign)
                    .ThenInclude(c => c.Account)
                .Include(c => c.Campaign)
                    .ThenInclude(c => c.Purpose)
                .Include(c => c.Account)
                .OrderByDescending(c => c.CommentedAt)
                .AsNoTracking()
                .ToListAsync();
            
            Console.WriteLine($"[BE Repository] Found {comments.Count} comments");
            return comments;
        }

        // Get comments by campaign
        public async Task<IEnumerable<Comment>> GetCommentsByCampaignAsync(int campaignId)
        {
            Console.WriteLine($"[BE Repository] Getting comments for campaign {campaignId}");
            var comments = await context.Comments
                .Where(c => c.CampaignId == campaignId && c.ParentCommentId == null)
                .Include(c => c.Replies)
                .Include(c => c.Account)
                .OrderByDescending(c => c.CommentedAt)
                .AsNoTracking()
                .ToListAsync();
            
            Console.WriteLine($"[BE Repository] Found {comments.Count} comments for campaign {campaignId}");
            return comments;
        }

        // Get comment by ID
        public async Task<Comment?> GetByIdAsync(int commentId)
        {
            return await context.Comments
                .Include(c => c.Replies)
                .Include(c => c.Campaign)
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.CommentId == commentId);
        }

        // Get recent comments
        public async Task<IEnumerable<Comment>> GetRecentCommentsAsync(int? accountId, string guestName)
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            
            return await context.Comments
                .Include(c => c.Campaign)
                    .ThenInclude(c => c.Account)
                .Include(c => c.Campaign)
                    .ThenInclude(c => c.Purpose)
                .Include(c => c.Account)
                .Where(c => 
                    (accountId.HasValue && c.AccountId == accountId) || 
                    (!string.IsNullOrEmpty(guestName) && c.GuestName == guestName))
                .Where(c => c.CommentedAt >= oneHourAgo)
                .ToListAsync();
        }

        // Get comments for admin dashboard
        public async Task<IEnumerable<CommentDashboardDto>> GetAllForDashboardAsync()
        {
            Console.WriteLine($"[BE Repository] Getting parent comments for dashboard");

            var comments = await context.Comments
                .Where(c => c.ParentCommentId == null)
                .Include(c => c.Replies)
                .Include(c => c.Campaign)
                .OrderByDescending(c => c.CommentedAt)
                .Select(c => new CommentDashboardDto
                {
                    CommentId = c.CommentId,
                    Content = c.Content,
                    GuestName = c.GuestName ?? "GUEST",
                    CampaignId = c.CampaignId,
                    CreatedAt = c.CommentedAt,
                    IsReplied = c.Replies.Any()
                })
                .ToListAsync();

            Console.WriteLine($"[BE Repository] Found {comments.Count} parent comments for dashboard");
            return comments;
        }
    }
}

using Fe.DTOs.Comment;
using Fe.Services.Comment;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Linq;

namespace Fe.Areas.Web.Controllers
{
    [Area("Web")]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentController> _logger;

        public CommentController(ICommentService commentService, ILogger<CommentController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCommentDto dto)
        {
            try
            {
                _logger.LogInformation($"Received comment creation request. DTO: {System.Text.Json.JsonSerializer.Serialize(dto)}");

                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                // Get current user if logged in
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                
                _logger.LogInformation($"Current user: {(userId != null ? userName : "Not logged in")}");

                if (userId != null)
                {
                    try
                    {
                        dto.AccountId = int.Parse(userId);
                        _logger.LogInformation($"Set AccountId to {dto.AccountId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error parsing user ID: {userId}");
                        return Json(new { success = false, message = "Invalid user ID format" });
                    }

                    // If not anonymous, use user's name
                    if (!dto.IsAnonymous)
                    {
                        dto.GuestName = userName;
                        _logger.LogInformation($"Set GuestName to {dto.GuestName} for logged in user");
                    }
                }

                _logger.LogInformation($"Calling comment service with DTO: {System.Text.Json.JsonSerializer.Serialize(dto)}");
                var response = await _commentService.CreateAsync(dto);
                _logger.LogInformation($"Comment service response: {System.Text.Json.JsonSerializer.Serialize(response)}");

                if (response == null)
                {
                    return Json(new { success = false, message = "Failed to create comment" });
                }

                // Generate HTML for the new comment
                var html = $@"
                <div class='d-flex mb-4'>
                    <div class='ms-3'>
                        <div class='fw-bold'>
                            {(response.IsAnonymous ? "Anonymous" : (response.GuestName ?? "Guest"))}
                            <span class='text-muted small'>– {response.CommentedAt:dd/MM/yyyy HH:mm}</span>
                        </div>
                        <p>{response.Content}</p>
                    </div>
                </div>";

                return Json(new { success = true, message = "Comment created successfully", html = html });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating comment for campaign {dto.CampaignId}. DTO: {System.Text.Json.JsonSerializer.Serialize(dto)}");
                return Json(new { success = false, message = ex.Message ?? "An error occurred while creating the comment. Please try again." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(int campaignId)
        {
            try
            {
                _logger.LogInformation($"[DEBUG] GetComments called with campaignId={campaignId}");
                var comments = await _commentService.GetByCampaignAsync(campaignId);
                _logger.LogInformation($"[DEBUG] GetComments: {comments?.Count ?? 0} comments found for campaignId={campaignId}");
                _logger.LogInformation($"[DEBUG] Comments data: {JsonSerializer.Serialize(comments)}");

                if (comments == null || !comments.Any())
                {
                    _logger.LogInformation("[DEBUG] No comments found, returning empty message");
                    return Content("<p class='text-muted'>There are no comments for this campaign yet.</p>", "text/html");
                }

                var html = new System.Text.StringBuilder();
                foreach (var comment in comments)
                {
                    _logger.LogInformation($"[DEBUG] Processing comment: {JsonSerializer.Serialize(comment)}");
                    html.Append($@"
                    <div class='d-flex mb-4'>
                        <div class='ms-3'>
                            <div class='fw-bold'>
                                {(comment.IsAnonymous ? "Anonymous" : (comment.GuestName ?? "Guest"))}
                                <span class='text-muted small'>– {comment.CommentedAt:dd/MM/yyyy HH:mm}</span>
                            </div>
                            <p>{comment.Content}</p>");
                    if (comment.Replies != null && comment.Replies.Any())
                    {
                        foreach (var reply in comment.Replies)
                        {
                            html.Append($@"
                            <div class='mt-2 ms-4 p-2 border rounded bg-light'>
                                <div class='fw-bold text-primary'>{reply.GuestName}:</div>
                                <div class='text-muted small'>{reply.CommentedAt:dd/MM/yyyy HH:mm}</div>
                                <p>{reply.Content}</p>
                            </div>");
                        }
                    }
                    html.Append("</div></div>");
                }

                var result = html.ToString();
                _logger.LogInformation($"[DEBUG] Generated HTML: {result}");
                return Content(result, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[DEBUG] Error getting comments for campaign {campaignId}");
                return BadRequest(new { message = "Error loading comments" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Reply(int commentId, string replyContent)
        {
            try
            {
                _logger.LogInformation($"Received reply request for comment {commentId}");
                
                if (string.IsNullOrWhiteSpace(replyContent))
                {
                    _logger.LogWarning("Empty reply content");
                    return Json(new { success = false, message = "Reply content cannot be empty" });
                }

                await _commentService.ReplyAsync(commentId, replyContent);
                _logger.LogInformation($"Reply created successfully for comment {commentId}");
                
                return Json(new { success = true, message = "Reply created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating reply for comment {commentId}");
                return Json(new { success = false, message = "An error occurred while creating the reply. Please try again." });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int commentId, int campaignId)
        {
            try
            {
                _logger.LogInformation($"Received delete request for comment {commentId}");
                
                await _commentService.DeleteAsync(commentId);
                _logger.LogInformation($"Comment {commentId} deleted successfully");
                
                return Json(new { success = true, message = "Comment deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting comment {commentId}");
                return Json(new { success = false, message = "An error occurred while deleting the comment. Please try again." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadMoreComments(int campaignId, int skip, int take = 5)
        {
            try
            {
                var comments = await _commentService.GetByCampaignAsync(campaignId);
                var paginatedComments = comments
                    .OrderByDescending(c => c.CommentedAt)
                    .Skip(skip)
                    .Take(take)
                    .ToList();

                return Json(new { 
                    success = true, 
                    comments = paginatedComments 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading more comments");
                return Json(new { 
                    success = false, 
                    message = "Failed to load more comments" 
                });
            }
        }
    }
}

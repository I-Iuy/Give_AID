using Fe.DTOs.Comment;
using Fe.Services.Comment;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

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
                _logger.LogInformation($"Received comment creation request for campaign {dto.CampaignId}");
                
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for comment creation");
                    return Json(new { success = false, message = "Invalid comment data" });
                }

                var response = await _commentService.CreateAsync(dto);
                _logger.LogInformation($"Comment created successfully for campaign {dto.CampaignId}");

                // Add logging for the CommentDto received from the service
                _logger.LogInformation($"[FE Controller] Received CommentDto after creation: {System.Text.Json.JsonSerializer.Serialize(response)}");
                _logger.LogInformation($"[FE Controller] Received CommentDto Content: '{response?.Content}'");
                
                if (response == null)
                {
                    _logger.LogWarning("Comment creation returned null response");
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
                _logger.LogError(ex, $"Error creating comment for campaign {dto.CampaignId}");
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
    }
}

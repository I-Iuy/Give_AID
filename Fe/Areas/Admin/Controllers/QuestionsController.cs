using Fe.DTOs.Comment;
using Fe.Services.Comment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")] // Tạm thời comment lại để test
    public class QuestionsController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<QuestionsController> _logger;
        private const int PageSize = 10;

        public QuestionsController(ICommentService commentService, ILogger<QuestionsController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                _logger.LogInformation("Retrieving comments for dashboard, page {Page}", page);
                var comments = await _commentService.GetAllForDashboardAsync();
                var totalPages = (int)Math.Ceiling(comments.Count() / (double)PageSize);
                page = Math.Max(1, Math.Min(page, totalPages));

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.HasPreviousPage = page > 1;
                ViewBag.HasNextPage = page < totalPages;

                var paginatedComments = comments
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize);

                _logger.LogInformation("Retrieved {Count} comments for page {Page}", paginatedComments.Count(), page);
                return View(paginatedComments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for dashboard");
                TempData["Error"] = "An error occurred while retrieving comments. Please try again later.";
                return View(Array.Empty<CommentDashboardDto>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reply(int id, string content)
        {
            try
            {
                _logger.LogInformation("Processing reply for comment {CommentId}", id);
                
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("Empty reply content for comment {CommentId}", id);
                    return Json(new { success = false, message = "Reply content cannot be empty." });
                }

                // Tạm thời bỏ kiểm tra admin vì chưa có authentication
                // var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // if (string.IsNullOrEmpty(adminId))
                // {
                //     _logger.LogWarning("Unauthenticated admin attempting to reply to comment {CommentId}", id);
                //     return Json(new { success = false, message = "Admin not authenticated." });
                // }

                await _commentService.ReplyAsync(id, content);
                _logger.LogInformation("Successfully replied to comment {CommentId}", id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replying to comment {CommentId}", id);
                return Json(new { success = false, message = "An error occurred while submitting the reply. Please try again later." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Processing delete request for comment {CommentId}", id);
                await _commentService.DeleteAsync(id);
                _logger.LogInformation("Successfully deleted comment {CommentId}", id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the comment. Please try again later." });
            }
        }
    }
}

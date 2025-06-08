using Be.DTOs.Comment;
using Be.Services.Comment;
using Microsoft.AspNetCore.Mvc;

namespace Be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // Create new comment (FE Web)
        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto dto)
        {
            try
            {
                var result = await _commentService.AddCommentAsync(dto);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Get comments by campaign (FE Web)
        [HttpGet("campaign/{campaignId}")]
        public async Task<IActionResult> GetCommentsByCampaign(int campaignId)
        {
            var result = await _commentService.GetCommentsByCampaignAsync(campaignId);
            return Ok(result);
        }

        // Get comment by ID
        [HttpGet("{commentId}")]
        public async Task<IActionResult> GetByIdAsync(int commentId)
        {
            Console.WriteLine($"[BE Controller] Received GetByIdAsync request for ID: {commentId}");
            try
            {
                var result = await _commentService.GetByIdAsync(commentId);
                Console.WriteLine($"[BE Controller] GetByIdAsync service returned: {System.Text.Json.JsonSerializer.Serialize(result)}");

                if (result == null)
                {
                    Console.WriteLine($"[BE Controller] Comment with ID {commentId} not found.");
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BE Controller] Error in GetByIdAsync for ID {commentId}: {ex.Message}");
                Console.WriteLine($"[BE Controller] Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "Internal server error." });
            }
        }

        // Delete comment (Admin)
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> Delete(int commentId)
        {
            try
            {
                await _commentService.DeleteCommentAsync(commentId);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        // Get all parent comments for admin dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardComments()
        {
            var result = await _commentService.GetAllForDashboardAsync();
            return Ok(result);
        }

        // Reply to comment (Admin)
        [HttpPost("reply")]
        public async Task<IActionResult> ReplyToComment([FromBody] ReplyDto model)
        {
            if (string.IsNullOrWhiteSpace(model.ReplyContent))
            {
                return BadRequest(new { success = false, message = "Reply content cannot be empty." });
            }

            await _commentService.ReplyToCommentAsync(model.CommentId, model.ReplyContent);
            return Ok(new { success = true });
        }
    }

    // Internal DTO (or move to separate file)
    public class ReplyDto
    {
        public int CommentId { get; set; }
        public string ReplyContent { get; set; } = string.Empty;
    }
}

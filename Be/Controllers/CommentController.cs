using Be.DTOs;
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

        // Tạo bình luận mới (FE Web)
        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto dto)
        {
            await _commentService.AddCommentAsync(dto);
            return Ok(new { success = true });
        }

        // Lấy danh sách bình luận theo chiến dịch (FE Web)
        [HttpGet("campaign/{campaignId}")]
        public async Task<IActionResult> GetCommentsByCampaign(int campaignId)
        {
            var result = await _commentService.GetCommentsByCampaignAsync(campaignId);
            return Ok(result);
        }

        // Xoá comment (Admin)
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

        // Lấy tất cả bình luận cha cho dashboard admin
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardComments()
        {
            var result = await _commentService.GetAllForDashboardAsync();
            return Ok(result);
        }

        // Trả lời bình luận (Admin)
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

    // ✅ Thêm DTO nội bộ (hoặc đưa vào file riêng)
    public class ReplyDto
    {
        public int CommentId { get; set; }
        public string ReplyContent { get; set; } = string.Empty;
    }
}

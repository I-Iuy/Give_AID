using Fe.DTOs.Comment;
using Fe.Services.Comment;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fe.Areas.Web.Controllers
{
    [Area("Web")]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateCommentDto model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ nội dung bình luận.";
                return RedirectToAction("Post", "Home", new { id = model.CampaignId });
            }

            // Gán AccountId nếu người dùng đã đăng nhập
            if (User.Identity?.IsAuthenticated == true)
            {
                var accountIdClaim = User.FindFirst("AccountId")?.Value;
                if (int.TryParse(accountIdClaim, out var accountId))
                {
                    model.AccountId = accountId;
                }
            }

            await _commentService.CreateAsync(model);

            return RedirectToAction("Post", "Home", new { id = model.CampaignId });
        }
    }
}

using Fe.DTOs.Comment;
using Fe.Services.Comment;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuestionsController : Controller
    {
        private readonly ICommentService _commentService;

        public QuestionsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // GET: Admin/Question
        public async Task<IActionResult> Index()
        {
            var comments = await _commentService.GetAllForDashboardAsync();
            return View(comments);
        }

        // GET: Admin/Question/Reply/{id}
        [HttpGet]
        public async Task<IActionResult> Reply(int id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
                return NotFound();

            return View(comment);
        }

        // POST: Admin/Question/Reply
        [HttpPost]
        public async Task<IActionResult> Reply(int id, string replyContent)
        {
            if (string.IsNullOrWhiteSpace(replyContent))
            {
                ModelState.AddModelError("", "Reply cannot be empty.");
                return RedirectToAction("Reply", new { id });
            }

            await _commentService.ReplyAsync(id, replyContent);
            return RedirectToAction("Index");
        }

        // POST: Admin/Question/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _commentService.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}

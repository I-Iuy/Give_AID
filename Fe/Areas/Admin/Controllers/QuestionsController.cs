using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuestionsController : Controller
    {
        public class QuestionModel
        {
            public int Id { get; set; }
            public int ProgramId { get; set; }
            public string UserName { get; set; }
            public string Content { get; set; }
            public string Answer { get; set; }
        }

        private static readonly List<QuestionModel> SampleQuestions = new List<QuestionModel>
        {
            new QuestionModel { Id = 1, ProgramId = 1, UserName = "Nguyen Van A", Content = "How will the funds be distributed?", Answer = "" },
            new QuestionModel { Id = 2, ProgramId = 1, UserName = "Le Thi B", Content = "Can I donate monthly?", Answer = "Yes, monthly donations are accepted." }
        };

        public IActionResult List(int programId)
        {
            ViewBag.ProgramId = programId;
            var questions = SampleQuestions.Where(q => q.ProgramId == programId).ToList();
            return View(questions);
        }

        public IActionResult Reply(int id)
        {
            var q = SampleQuestions.FirstOrDefault(x => x.Id == id);
            return View(q);
        }
    }
}

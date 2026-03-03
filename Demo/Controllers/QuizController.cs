using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo.Controllers;

[Authorize]
public class QuizController : Controller
{
    private readonly DB db;

    public QuizController(DB db)
    {
        this.db = db;
    }

    private int? GetCurrentUserId()
    {
        var name = User.Identity?.Name;
        if (string.IsNullOrEmpty(name)) return null;
        return db.Users.Where(u => u.Username == name).Select(u => u.UserId).FirstOrDefault();
    }

    public IActionResult Index()
    {
        var questions = db.QuizQuestions
            .Where(q => q.IsActive)
            .OrderBy(q => Guid.NewGuid())
            .Take(10)
            .ToList();

        var vm = new QuizTakeVM
        {
            Questions = questions.Select(q => new QuizQuestionAnswerVM
            {
                QuizQuestionId = q.QuizQuestionId,
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    public IActionResult Index(QuizTakeVM vm)
    {
        if (vm.Questions == null || vm.Questions.Count == 0)
        {
            ModelState.AddModelError("", "No questions submitted. Please retry the quiz.");
            return RedirectToAction(nameof(Index));
        }

        var ids = vm.Questions.Select(q => q.QuizQuestionId).ToList();
        var dbQuestions = db.QuizQuestions
            .Where(q => ids.Contains(q.QuizQuestionId))
            .ToDictionary(q => q.QuizQuestionId);

        var gradedQuestions = new List<QuizQuestionAnswerVM>();
        int correct = 0;

        foreach (var q in vm.Questions)
        {
            if (!dbQuestions.TryGetValue(q.QuizQuestionId, out var entity))
                continue;

            var selected = q.SelectedOption?.Trim().ToUpperInvariant();
            var correctOpt = entity.CorrectOption?.Trim().ToUpperInvariant() ?? "";
            bool isCorrect = !string.IsNullOrEmpty(selected) && selected == correctOpt;
            if (isCorrect) correct++;

            gradedQuestions.Add(new QuizQuestionAnswerVM
            {
                QuizQuestionId = entity.QuizQuestionId,
                QuestionText = entity.QuestionText,
                OptionA = entity.OptionA,
                OptionB = entity.OptionB,
                OptionC = entity.OptionC,
                OptionD = entity.OptionD,
                SelectedOption = selected,
                CorrectOption = correctOpt,
                IsCorrect = isCorrect
            });
        }

        var resultVm = new QuizTakeVM
        {
            Questions = gradedQuestions,
            CorrectCount = correct,
            IsSubmitted = true
        };

        var userId = GetCurrentUserId();
        if (userId != null && gradedQuestions.Count > 0)
        {
            var attempt = new QuizAttempt
            {
                UserId = userId.Value,
                TotalQuestions = gradedQuestions.Count,
                CorrectCount = correct
            };
            db.QuizAttempts.Add(attempt);
            db.SaveChanges();

            foreach (var q in gradedQuestions)
            {
                db.QuizAttemptDetails.Add(new QuizAttemptDetail
                {
                    QuizAttemptId = attempt.QuizAttemptId,
                    QuizQuestionId = q.QuizQuestionId,
                    SelectedOption = q.SelectedOption,
                    IsCorrect = q.IsCorrect
                });
            }
            db.SaveChanges();

            ViewBag.SavedAttemptId = attempt.QuizAttemptId;
        }

        return View(resultVm);
    }

    public IActionResult History()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var list = db.QuizAttempts
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.QuizAttemptId)
            .ToList();

        return View(list);
    }

    public IActionResult Detail(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var attempt = db.QuizAttempts
            .Include(a => a.Details)
            .ThenInclude(d => d.Question)
            .FirstOrDefault(a => a.QuizAttemptId == id && a.UserId == userId);

        if (attempt == null) return RedirectToAction(nameof(History));

        return View(attempt);
    }
}


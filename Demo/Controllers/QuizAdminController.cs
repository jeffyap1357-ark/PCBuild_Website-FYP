using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers;

[Authorize(Roles = Roles.Admin)]
public class QuizAdminController : Controller
{
    private readonly DB db;

    public QuizAdminController(DB db)
    {
        this.db = db;
    }

    private static readonly string[] Levels = ["Beginner", "Normal", "Professional"];

    public IActionResult Index(string? level)
    {
        var query = db.QuizQuestions.AsQueryable();
        if (!string.IsNullOrWhiteSpace(level) && Levels.Contains(level, StringComparer.OrdinalIgnoreCase))
        {
            var lv = Levels.First(x => x.Equals(level, StringComparison.OrdinalIgnoreCase));
            if (lv == "Beginner")
                query = query.Where(q => q.Level == null || q.Level == "" || q.Level == "Beginner");
            else
                query = query.Where(q => q.Level != null && q.Level == lv);
        }
        var list = query.OrderBy(q => q.QuizQuestionId).ToList();
        ViewBag.Levels = Levels;
        ViewBag.SelectedLevel = level;
        return View(list);
    }

    public IActionResult Create()
    {
        return View(new QuizQuestion());
    }

    [HttpPost]
    public IActionResult Create(QuizQuestion q)
    {
        if (string.IsNullOrWhiteSpace(q.CorrectOption))
            ModelState.AddModelError("CorrectOption", "Correct option is required (A/B/C/D).");

        q.CorrectOption = (q.CorrectOption ?? "").Trim().ToUpperInvariant();
        if (!new[] { "A", "B", "C", "D" }.Contains(q.CorrectOption))
            ModelState.AddModelError("CorrectOption", "Correct option must be A, B, C, or D.");

        var level = (q.Level ?? "").Trim();
        if (!string.IsNullOrEmpty(level) && !Levels.Contains(level, StringComparer.OrdinalIgnoreCase))
            q.Level = null;
        else if (!string.IsNullOrEmpty(level))
            q.Level = Levels.First(x => x.Equals(level, StringComparison.OrdinalIgnoreCase));

        q.Explanation = string.IsNullOrWhiteSpace(q.Explanation) ? null : q.Explanation.Trim();
        if (ModelState.IsValid)
        {
            q.IsActive = true;
            db.QuizQuestions.Add(q);
            db.SaveChanges();
            TempData["Info"] = "Question created.";
            return RedirectToAction(nameof(Index));
        }

        return View(q);
    }

    public IActionResult Edit(int id)
    {
        var q = db.QuizQuestions.FirstOrDefault(x => x.QuizQuestionId == id);
        if (q == null) return RedirectToAction(nameof(Index));
        return View(q);
    }

    [HttpPost]
    public IActionResult Edit(int id, QuizQuestion form)
    {
        var q = db.QuizQuestions.FirstOrDefault(x => x.QuizQuestionId == id);
        if (q == null) return RedirectToAction(nameof(Index));

        if (string.IsNullOrWhiteSpace(form.CorrectOption))
            ModelState.AddModelError("CorrectOption", "Correct option is required (A/B/C/D).");

        var correct = (form.CorrectOption ?? "").Trim().ToUpperInvariant();
        if (!new[] { "A", "B", "C", "D" }.Contains(correct))
            ModelState.AddModelError("CorrectOption", "Correct option must be A, B, C, or D.");

        var level = (form.Level ?? "").Trim();
        if (!string.IsNullOrEmpty(level) && Levels.Contains(level, StringComparer.OrdinalIgnoreCase))
            q.Level = Levels.First(x => x.Equals(level, StringComparison.OrdinalIgnoreCase));
        else
            q.Level = null;

        if (ModelState.IsValid)
        {
            q.QuestionText = form.QuestionText;
            q.OptionA = form.OptionA;
            q.OptionB = form.OptionB;
            q.OptionC = form.OptionC;
            q.OptionD = form.OptionD;
            q.CorrectOption = correct;
            q.Explanation = string.IsNullOrWhiteSpace(form.Explanation) ? null : form.Explanation.Trim();
            q.IsActive = form.IsActive;
            db.SaveChanges();
            TempData["Info"] = "Question updated.";
            return RedirectToAction(nameof(Index));
        }

        return View(form);
    }

    public IActionResult Delete(int id)
    {
        var q = db.QuizQuestions.FirstOrDefault(x => x.QuizQuestionId == id);
        if (q == null) return RedirectToAction(nameof(Index));
        return View(q);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var q = db.QuizQuestions.FirstOrDefault(x => x.QuizQuestionId == id);
        if (q != null)
        {
            db.QuizQuestions.Remove(q);
            db.SaveChanges();
            TempData["Info"] = "Question deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}


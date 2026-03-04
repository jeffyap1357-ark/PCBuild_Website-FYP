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

    public IActionResult Index()
    {
        var list = db.QuizQuestions
            .OrderBy(q => q.QuizQuestionId)
            .ToList();
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

        if (ModelState.IsValid)
        {
            q.QuestionText = form.QuestionText;
            q.OptionA = form.OptionA;
            q.OptionB = form.OptionB;
            q.OptionC = form.OptionC;
            q.OptionD = form.OptionD;
            q.CorrectOption = correct;
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


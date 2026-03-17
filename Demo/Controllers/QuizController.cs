using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo.Controllers;

[Authorize]
public class QuizController : Controller
{
    private static readonly string[] Levels = ["Beginner", "Normal", "Professional"];

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

    private static string NormalizeLevel(string? level)
    {
        if (string.IsNullOrWhiteSpace(level)) return Levels[0];
        level = level.Trim();
        foreach (var lv in Levels)
        {
            if (string.Equals(lv, level, StringComparison.OrdinalIgnoreCase))
                return lv;
        }
        return Levels[0];
    }

    private static string BuildExplanation(QuizQuestion q, string correctOpt)
    {
        var text = q.QuestionText ?? string.Empty;
        correctOpt = correctOpt.ToUpperInvariant();
        if (text.Contains("brain", StringComparison.OrdinalIgnoreCase))
            return "The CPU executes instructions and controls all other components, so it is often called the \"brain\" of the computer.";
        if (text.Contains("operating system", StringComparison.OrdinalIgnoreCase) || text.Contains("permanently", StringComparison.OrdinalIgnoreCase))
            return "Your operating system and files are stored on persistent storage (SSD/HDD), which keeps data even when the PC is powered off.";
        if (text.Contains("socket type", StringComparison.OrdinalIgnoreCase) && text.Contains("AMD", StringComparison.OrdinalIgnoreCase))
            return "Most modern AMD desktop CPUs use AM4 or AM5 sockets.";
        if (text.Contains("DDR5 motherboard", StringComparison.OrdinalIgnoreCase))
            return "Memory generations are not cross-compatible. A DDR5 motherboard requires DDR5 RAM sticks.";
        if (text.Contains("gaming performance", StringComparison.OrdinalIgnoreCase) && text.Contains("1440p", StringComparison.OrdinalIgnoreCase))
            return "At higher resolutions like 1440p, the graphics card (GPU) is usually the main bottleneck for frames per second.";
        if (text.Contains("minimum RAM size", StringComparison.OrdinalIgnoreCase))
            return "16GB RAM is widely recommended as a comfortable baseline for modern gaming.";
        if (text.Contains("supplies power to all PC parts", StringComparison.OrdinalIgnoreCase))
            return "The power supply unit (PSU) converts wall AC power into stable DC voltages for every component.";
        if (text.Contains("SSD generally provide", StringComparison.OrdinalIgnoreCase) && text.Contains("HDD", StringComparison.OrdinalIgnoreCase))
            return "SSDs use flash storage with much lower latency than HDDs.";
        if (text.Contains("physically fits inside a PC case", StringComparison.OrdinalIgnoreCase))
            return "Form factor (like ATX, micro-ATX, Mini-ITX) defines the size and mounting holes of a motherboard.";
        if (text.Contains("quieter but more complex to install", StringComparison.OrdinalIgnoreCase))
            return "All-in-one liquid coolers move heat to a radiator away from the CPU.";
        return "The correct option best matches standard PC hardware behaviour for this question.";
    }

    private void GetUserLevelProgress(int userId, out HashSet<string> passedLevels, out int maxUnlockedIndex)
    {
        passedLevels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var attempts = db.QuizAttempts
            .Include(a => a.Details)
            .ThenInclude(d => d.Question)
            .Where(a => a.UserId == userId)
            .ToList();

        foreach (var attempt in attempts)
        {
            if (attempt.TotalQuestions <= 0 || attempt.CorrectCount != attempt.TotalQuestions)
                continue;
            var levelNames = attempt.Details
                .Select(d => d.Question?.Level)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(NormalizeLevel)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (levelNames.Count == 1)
                passedLevels.Add(levelNames[0]);
        }

        maxUnlockedIndex = 1;
        for (int i = 0; i < Levels.Length; i++)
        {
            if (passedLevels.Contains(Levels[i]))
            {
                var unlockUpTo = Math.Min(Levels.Length, i + 2);
                if (unlockUpTo > maxUnlockedIndex)
                    maxUnlockedIndex = unlockUpTo;
            }
        }
    }

    public IActionResult LevelsView()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        GetUserLevelProgress(userId.Value, out var passedLevels, out var maxUnlockedIndex);

        var model = Levels.Select((name, index) => new
        {
            Name = name,
            Index = index,
            Unlocked = index < maxUnlockedIndex,
            Completed = passedLevels.Contains(name)
        }).ToList();

        ViewBag.Levels = model;
        return View("Levels");
    }

    public IActionResult Index(string level)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var normalizedLevel = NormalizeLevel(level);

        GetUserLevelProgress(userId.Value, out _, out var unlockedMax);

        int levelIndex = Array.IndexOf(Levels, normalizedLevel);
        if (levelIndex < 0) levelIndex = 0;
        if (levelIndex > unlockedMax - 1)
        {
            TempData["Info"] = "Please complete the previous level with a full score to unlock this level.";
            return RedirectToAction(nameof(LevelsView));
        }

        const int QuestionsPerQuiz = 5;
        IQueryable<QuizQuestion> levelQuery = db.QuizQuestions.Where(q => q.IsActive);
        if (normalizedLevel == "Beginner")
            levelQuery = levelQuery.Where(q => q.Level == null || q.Level == "" || q.Level == "Beginner");
        else
            levelQuery = levelQuery.Where(q => q.Level != null && q.Level != "" && q.Level == normalizedLevel);

        var questions = levelQuery
            .OrderBy(q => Guid.NewGuid())
            .Take(QuestionsPerQuiz)
            .ToList();

        if (questions.Count == 0)
        {
            TempData["Info"] = "No questions configured for this level yet.";
            return RedirectToAction(nameof(LevelsView));
        }

        var vm = new QuizTakeVM
        {
            Level = normalizedLevel,
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
            return RedirectToAction(nameof(LevelsView));
        }

        var ids = vm.Questions.Select(q => q.QuizQuestionId).ToList();
        var dbQuestions = db.QuizQuestions
            .Where(q => ids.Contains(q.QuizQuestionId))
            .ToDictionary(q => q.QuizQuestionId);

        var gradedQuestions = new List<QuizQuestionAnswerVM>();
        int correct = 0;
        string normalizedLevel = NormalizeLevel(vm.Level);

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
                IsCorrect = isCorrect,
                Explanation = BuildExplanation(entity, correctOpt)
            });
        }

        var resultVm = new QuizTakeVM
        {
            Level = normalizedLevel,
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
            TempData["ShowFeedback"] = true;
        }

        return View(resultVm);
    }

    public IActionResult History()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account");

        var list = db.QuizAttempts
            .Include(a => a.Details)
            .ThenInclude(d => d.Question)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.QuizAttemptId)
            .ToList();

        foreach (var attempt in list)
        {
            var levelNames = attempt.Details
                .Select(d => d.Question?.Level)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(NormalizeLevel)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            attempt.Level = levelNames.Count == 1 ? levelNames[0] : "Mixed";
        }

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

        var levelNames = attempt.Details
            .Select(d => d.Question?.Level)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(NormalizeLevel)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        attempt.Level = levelNames.Count == 1 ? levelNames[0] : "Mixed";

        return View(attempt);
    }
}

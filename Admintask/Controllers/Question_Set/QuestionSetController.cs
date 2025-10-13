using AdminPortal.Data;
using AdminPortal.Models;
using AdminPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPortal.Controllers.QuestionSetArea
{
    // /question-set/...
    [Route("question-set")]
    public class QuestionSetController : Controller
    {
        private readonly FloriaDbContext _db;
        public QuestionSetController(FloriaDbContext db) => _db = db;

        // GET: /question-set
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var vm = await _db.QuestionSets
                .OrderBy(x => x.Id)
                .Select(x => new QuestionSetVm
                {
                    Id = x.Id,
                    Name = x.Name,
                    Hint = x.Description,
                    IsActive = x.IsActive,
                    IsLocked = x.IsLocked,
                    IsScore = x.IsScore,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            // The project currently contains the view at Views/Home/Manage_Question_Set/questionset.cshtml
            // Return that specific view path so the controller renders the existing file.
            return View("~/Views/Home/Manage_Question_Set/questionset.cshtml", vm);
        }

        // GET: /question-set/create
        [HttpGet("create")]
        public IActionResult Create() => View(new QuestionSetVm());

        // POST: /question-set/create
        [HttpPost("create"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionSetVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var entity = new QuestionSet
            {
                Name = vm.Name.Trim(),
                Description = vm.Hint,
                IsActive = vm.IsActive, // true = hiện
                IsScore = vm.IsScore,
                IsLocked = vm.IsLocked
            };
            _db.QuestionSets.Add(entity);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /question-set/edit/5
        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var x = await _db.QuestionSets.FindAsync(id);
            if (x == null) return NotFound();

            var vm = new QuestionSetVm
            {
                Id = x.Id,
                Name = x.Name,
                Hint = x.Description,
                IsActive = x.IsActive,
                IsLocked = x.IsLocked,
                IsScore = x.IsScore,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            };
            return View(vm); // View: Views/QuestionSet/Edit.cshtml
        }

        // POST: /question-set/edit/5
        [HttpPost("edit/{id:int}"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuestionSetVm vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var x = await _db.QuestionSets.FindAsync(id);
            if (x == null) return NotFound();

            x.Name = vm.Name.Trim();
            x.Description = vm.Hint;
            x.IsActive = vm.IsActive;
            x.IsScore = vm.IsScore;
            x.IsLocked = vm.IsLocked;
            // UpdatedAt set trong SaveChanges

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: /question-set/toggle-lock
        [HttpPost("toggle-lock"), ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var x = await _db.QuestionSets.FindAsync(id);
            if (x == null) return NotFound();

            x.IsLocked = !x.IsLocked;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: /question-set/toggle-visibility
        // (is_active true/false ↔ nút Hidden/Disable trên UI)
        [HttpPost("toggle-visibility"), ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var x = await _db.QuestionSets.FindAsync(id);
            if (x == null) return NotFound();

            x.IsActive = !x.IsActive; // false = hidden
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // // POST: /question-set/delete/5
        // [HttpPost("delete/{id:int}"), ValidateAntiForgeryToken]
        // public async Task<IActionResult> Delete(int id)
        // {
        //     var x = await _db.QuestionSets.FindAsync(id);
        //     if (x == null) return NotFound();

        //     _db.QuestionSets.Remove(x);
        //     await _db.SaveChangesAsync();
        //     return RedirectToAction(nameof(Index));
        // }
        
                [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id)
{
    var set = await _db.QuestionSets
        .FirstOrDefaultAsync(s => s.Id == id);

    if (set == null) return NotFound();
    if (set.IsLocked)
    {
        TempData["msg"] = "Bộ đang bị khóa, không thể xóa.";
        return RedirectToAction(nameof(Index)); // trang danh sách
    }

    _db.QuestionSets.Remove(set); // cascade sẽ lo phần còn lại
    await _db.SaveChangesAsync();

    TempData["msg"] = $"Đã xóa bộ #{id} và toàn bộ câu hỏi/đáp án liên quan.";
    return RedirectToAction(nameof(Index));
}
    }
}

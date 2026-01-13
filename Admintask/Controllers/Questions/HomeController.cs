using AdminPortal.Data;
using AdminPortal.Models;
using AdminPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly FloriaDbContext _db;
        public HomeController(FloriaDbContext db) => _db = db;

        // -----------------------------
        // Helpers chung
        // -----------------------------
        private IQueryable<Question> BaseQueryWithSet(bool? score = null)
        {
            IQueryable<Question> q = _db.Questions.AsNoTracking();
            q = q.Include(x => x.Answers);
            q = q.Include(x => x.QuestionSet);

            if (score.HasValue)
                q = q.Where(x => x.QuestionSet != null && x.QuestionSet.IsScore == score.Value);

            return q;
        }

        private static IQueryable<QuestionWithAnswersVm> ProjectToVm(IQueryable<Question> q)
        {
            return q.Select(x => new QuestionWithAnswersVm
            {
                QuestionId    = x.Id,
                QuestionSetId = x.QuestionSetId,
                Text          = x.Text,
                Type          = x.Type,
                IsActive      = x.IsActive,
                IsLocked      = x.IsLocked || (x.QuestionSet != null && x.QuestionSet.IsLocked),
                IsSetScored   = x.QuestionSet != null && x.QuestionSet.IsScore,
                OrderInSet    = x.OrderInSet,
                MaxPoints     = x.MaxPoints,
                Skipped       = x.Skipped,
                Answers = x.Answers
                    .OrderBy(a => a.OrderInQuestion ?? int.MaxValue)
                    .Select(a => new AnswerVm
                    {
                        Id = a.Id,
                        Label = a.Label,
                        Text  = a.Text,
                        Hint  = a.Hint,
                        OrderInQuestion = a.OrderInQuestion,
                        Points = a.Points,
                        IsExclusive = a.IsExclusive,
                        IsActive = a.IsActive
                    }).ToList()
            });
        }

        /// <summary>
        /// Tính điểm hợp lệ cho 1 câu trong bộ có tính điểm, clamp theo MaxScore còn lại.
        /// Nếu qId != null → trừ điểm của chính câu đó ra khỏi tổng đang dùng.
        /// </summary>
        private async Task<decimal?> ComputeValidPointsAsync(int questionSetId, int? qId, decimal? requested)
        {
            var set = await _db.QuestionSets.FirstOrDefaultAsync(s => s.Id == questionSetId);
            if (set == null || !set.IsScore) return null;

            var want = Math.Max(0m, requested ?? 0m);

            // tổng điểm đã dùng (ngoại trừ câu đang update)
            var used = await _db.Questions
                .Where(q => q.QuestionSetId == questionSetId)
                .SumAsync(q => (qId.HasValue && q.Id == qId.Value) ? 0m : (q.MaxPoints ?? 0m));

            var remain = Math.Max(0m, set.MaxScore - used);
            if (want > remain) want = remain;

            return want > 0 ? want : (decimal?)null;
        }

        private IActionResult RedirAdd(bool score, int? questionSetId = null)
            => RedirectToAction(nameof(AddQuestion), new { score, questionSetId });

        private IActionResult RedirManage(int? questionSetId, bool? score = null)
            => RedirectToAction(nameof(ManageQuestions), new { questionSetId, score });

        // -----------------------------
        // LIST / MANAGE
        // -----------------------------
        // GET /Home/ManageQuestions?questionSetId=1&score=true
        public async Task<IActionResult> ManageQuestions(int? questionSetId, bool? score = null, int page = 1, int pageSize = 50)
        {
            var q = BaseQueryWithSet(score);
            if (questionSetId.HasValue) q = q.Where(x => x.QuestionSetId == questionSetId.Value);

            var data = await ProjectToVm(q)
                .OrderBy(x => x.QuestionSetId)
                .ThenBy(x => x.OrderInSet)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.QuerySetId = questionSetId;
            ViewBag.Score = score ?? false;
            return View("Manage_Questions/questions", data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuestionOrder(int questionId, int questionSetId, int newOrder, bool? score = null)
        {
            var list = await _db.Questions
                .Where(x => x.QuestionSetId == questionSetId)
                .OrderBy(x => x.OrderInSet)
                .ToListAsync();

            var target = list.FirstOrDefault(x => x.Id == questionId);
            if (target == null) return NotFound();

            if (newOrder < 1) newOrder = 1;
            if (newOrder > list.Count) newOrder = list.Count;

            list.Remove(target);
            list.Insert(newOrder - 1, target);

            for (int i = 0; i < list.Count; i++)
            {
                var desired = i + 1;
                if (list[i].OrderInSet != desired)
                {
                    list[i].OrderInSet = desired;
                    list[i].UpdatedAt = DateTime.UtcNow;
                    _db.Entry(list[i]).Property(x => x.OrderInSet).IsModified = true;
                    _db.Entry(list[i]).Property(x => x.UpdatedAt).IsModified = true;
                }
            }

            await _db.SaveChangesAsync();
            TempData["msg"] = $"Đã cập nhật thứ tự câu hỏi #{questionId} → {newOrder}.";
            return RedirManage(questionSetId, score);
        }
        public async Task<IActionResult> AddQuestion(bool score = false, int? questionSetId = null)
        {
            // Query câu hỏi như bạn đang có
            var q = _db.Questions
                .AsNoTracking()
                .Include(x => x.Answers)
                .Include(x => x.QuestionSet)
                .Where(x => x.QuestionSet != null && x.QuestionSet.IsScore == score);

            if (questionSetId.HasValue)
                q = q.Where(x => x.QuestionSetId == questionSetId.Value);

            var data = await q
                .OrderBy(x => x.QuestionSetId).ThenBy(x => x.OrderInSet)
                .Select(x => new QuestionWithAnswersVm {
                    QuestionId    = x.Id,
                    QuestionSetId = x.QuestionSetId,
                    Text          = x.Text,
                    Type          = x.Type,
                    IsActive      = x.IsActive,
                    IsLocked      = x.IsLocked || (x.QuestionSet != null && x.QuestionSet.IsLocked),
                    OrderInSet    = x.OrderInSet,
                    MaxPoints     = x.MaxPoints,
                    Skipped       = x.Skipped
                })
                .ToListAsync();

            // NEW: bơm tất cả QuestionSet của tab hiện tại (kể cả set chưa có câu)
            var allSets = await _db.QuestionSets
                .AsNoTracking()
                .Where(s => s.IsScore == score /*&& s.IsActive*/)  // tùy bạn có lọc IsActive không
                .OrderBy(s => s.Id)
                .Select(s => new { s.Id, s.Name, s.IsLocked, s.MaxScore })
                .ToListAsync();

            ViewBag.Score = score;
            ViewBag.QuerySetId = questionSetId;

            // Đưa xuống view:
            ViewBag.AllSets = allSets;
            ViewBag.MaxScoresBySet = allSets.ToDictionary(x => x.Id, x => x.MaxScore);

            return View("Manage_Add_Questions/AddQuestion", data);
        }

        // -----------------------------
        // CRUD QUESTION (gộp toggle)
        // -----------------------------
        /// <summary>
        /// lock | active | delete
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleQuestionState(
            int id,
            string op,
            bool score,
            int? questionSetId = null,
            string? returnUrl = null,
            string? from = null,
            int? page = null,
            string? anchor = null
        )
        {
            var x = await _db.Questions.FindAsync(id);
            if (x == null) return NotFound();

            switch ((op ?? "").ToLowerInvariant())
            {
                case "lock":  x.IsLocked = !x.IsLocked; break;
                case "active": x.IsActive = !x.IsActive; break;
                case "skip":   x.Skipped = !x.Skipped; break;
                case "delete":
                    _db.Questions.Remove(x);
                    await _db.SaveChangesAsync();
                    TempData["msg"] = $"Đã xóa câu hỏi #{id}.";
                    return RedirSmart(score, questionSetId ?? x.QuestionSetId, returnUrl, from, page, anchor ?? $"q-{id}");
                default:
                    return BadRequest("op phải là lock|active|skip|delete");
            }

            await _db.SaveChangesAsync();
            TempData["msg"] = $"Đã cập nhật trạng thái ({op}) cho câu hỏi #{id}.";
            return RedirSmart(score, questionSetId ?? x.QuestionSetId, returnUrl, from, page, anchor ?? $"q-{id}");
        }

        private IActionResult RedirSmart(
            bool score,
            int? questionSetId,
            string? returnUrl,
            string? from,
            int? page,
            string? anchor
        )
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl + (string.IsNullOrEmpty(anchor) ? "" : $"#{anchor}"));

            var route = new { questionSetId, score, page };
            string fragment = string.IsNullOrEmpty(anchor) ? null : anchor;

            string? url = from?.ToLowerInvariant() switch
            {
                "manage" => Url.Action("ManageQuestions", "Home", route, null, null, fragment),
                "add"    => Url.Action("AddQuestion",     "Home", route, null, null, fragment),
                _        => Url.Action("ManageQuestions", "Home", route, null, null, fragment)
            };

            return Redirect(url ?? "/");
        }


        // tạo câu hỏi
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuestion(int questionSetId, string text, string type = "single", bool score = false, decimal? maxPoints = null)
        {
            var set = await _db.QuestionSets.FirstOrDefaultAsync(s => s.Id == questionSetId);
            if (set == null) { TempData["msg"] = "Bộ câu hỏi không tồn tại."; return RedirAdd(score, questionSetId); }
            if (set.IsLocked) { TempData["msg"] = "Bộ câu hỏi đang bị khóa."; return RedirAdd(score, questionSetId); }

            var points = score ? await ComputeValidPointsAsync(questionSetId, null, maxPoints) : null;

            var maxOrder = await _db.Questions.Where(q => q.QuestionSetId == questionSetId).MaxAsync(q => (int?)q.OrderInSet) ?? 0;

            var q = new Question
            {
                QuestionSetId = questionSetId,
                Text = (text ?? "").Trim(),
                Type = string.IsNullOrWhiteSpace(type) ? "single" : type,
                OrderInSet = maxOrder + 1,
                IsActive = true,
                IsLocked = false,
                MaxPoints = points,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Questions.Add(q);
            await _db.SaveChangesAsync();
            TempData["msg"] = $"Đã tạo câu hỏi #{q.Id}.";
            return RedirAdd(score, questionSetId);
        }

        // popup edit kèm answers (giữ nguyên core, chỉ đổi redirect cho đồng nhất)
        [HttpPost("/Home/EditQuestionWithAnswers")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestionWithAnswers(Question dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["msg"] = "Dữ liệu không hợp lệ.";
                return RedirManage(dto.QuestionSetId, null);
            }

            var setState = await _db.QuestionSets
                .Where(s => s.Id == dto.QuestionSetId)
                .Select(s => new { s.IsLocked, s.IsActive })
                .FirstOrDefaultAsync();
            if (setState == null) return NotFound();
            if (setState.IsLocked) return Forbid();

            var q = await _db.Questions
                .Include(x => x.Answers)
                .FirstOrDefaultAsync(x => x.Id == dto.Id && x.QuestionSetId == dto.QuestionSetId);
            if (q == null) return NotFound();

            q.Text = (dto.Text ?? "").Trim();
            q.Type = dto.Type ?? q.Type;
            q.UpdatedAt = DateTime.UtcNow;

            if (string.Equals(q.Type, "text", StringComparison.OrdinalIgnoreCase))
            {
                var removeAll = q.Answers.ToList();
                if (removeAll.Count > 0) _db.Answers.RemoveRange(removeAll);
            }
            else
            {
                // Attempt to read MaxPoints from the posted form collection if present
                try
                {
                    var form = Request.Form;
                    string? rawMax = form["MaxPoints"].FirstOrDefault();
                    decimal? requested = null;
                    if (!string.IsNullOrWhiteSpace(rawMax))
                    {
                        rawMax = rawMax.Trim().Replace(',', '.');
                        if (decimal.TryParse(rawMax, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var val))
                            requested = val;
                    }

                    var set = await _db.QuestionSets.FirstOrDefaultAsync(s => s.Id == dto.QuestionSetId);
                    if (set != null && set.IsScore)
                    {
                        q.MaxPoints = await ComputeValidPointsAsync(dto.QuestionSetId, q.Id, requested);
                    }
                    else
                    {
                        q.MaxPoints = null;
                    }
                }
                catch
                {
                    // ignore parsing issues; do not block the save for non-critical max points parsing
                }

                var incoming = dto.Answers ?? new List<Answer>();
                var keepIds = incoming.Where(a => a.Id > 0).Select(a => a.Id).ToHashSet();

                var toRemove = q.Answers.Where(a => !keepIds.Contains(a.Id)).ToList();
                if (toRemove.Count > 0) _db.Answers.RemoveRange(toRemove);

                // Get form data to properly handle checkbox states
                var postedForm = Request.Form;
                
                // Get question set info to determine if scoring is enabled
                var questionSet = await _db.QuestionSets.FirstOrDefaultAsync(s => s.Id == dto.QuestionSetId);
                var isSetScored = questionSet?.IsScore ?? false;
                
                for (int i = 0; i < incoming.Count; i++)
                {
                    var a = incoming[i];
                    var label = a.Label?.Trim();
                    var text = (a.Text ?? "").Trim();
                    var hint = a.Hint?.Trim();
                    
                    bool isExclusive = false;
                    decimal pts = 0m;
                    
                    // Only process scoring if the question set is scored
                    if (isSetScored)
                    {
                        // Check if this answer's checkbox is checked by looking at form data
                        var isExclusiveKey = $"Answers[{i}].IsExclusive";
                        var isExclusiveValues = postedForm[isExclusiveKey];
                        isExclusive = isExclusiveValues.Any() && isExclusiveValues.Contains("true");
                        
                        // assign points from question maxpoints when marked
                        pts = (isExclusive && q.MaxPoints.HasValue) ? q.MaxPoints.Value : 0m;
                        
                        // Debug logging (can be removed in production)
                        System.Diagnostics.Debug.WriteLine($"Answer {i}: IsExclusive={isExclusive}, Points={pts}, MaxPoints={q.MaxPoints}");
                    }

                    if (a.Id > 0)
                    {
                        var ex = q.Answers.FirstOrDefault(x => x.Id == a.Id);
                        if (ex != null)
                        {
                            ex.Label = label ?? "";
                            ex.Text = text;
                            ex.Hint = hint ?? "";
                            // persist scoring fields: checked -> award question's max points
                            ex.Points = pts;
                            ex.IsExclusive = isExclusive;
                            ex.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        q.Answers.Add(new Answer
                        {
                            QuestionId = q.Id,
                            Label = label ?? "",
                            Text = text,
                            Hint = hint ?? "",
                            // scoring fields: checked -> award question's max points
                            Points = pts,
                            IsExclusive = isExclusive,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _db.SaveChangesAsync();
            TempData["msg"] = $"Đã lưu câu hỏi #{q.Id}.";
            return RedirManage(dto.QuestionSetId, null);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuestionPoints(
            int id, int questionSetId,
            string? maxPoints,  // <-- nhận string
            bool score,
            int? redirectSetId = null)
        {
            // Parse tolerant: chấm hoặc phẩy đều ok
            decimal? requested = null;
            if (!string.IsNullOrWhiteSpace(maxPoints))
            {
                var raw = maxPoints.Trim();
                // normalize: đổi dấu phẩy thành chấm
                raw = raw.Replace(',', '.');
                if (decimal.TryParse(raw, System.Globalization.NumberStyles.Number,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out var val))
                {
                    requested = val;
                }
            }

            var q = await _db.Questions.FirstOrDefaultAsync(x => x.Id == id && x.QuestionSetId == questionSetId);
            if (q == null) { TempData["msg"] = "Câu hỏi không tồn tại."; return RedirectToAction(nameof(AddQuestion), new { score, questionSetId = redirectSetId ?? questionSetId }); }

            var set = await _db.QuestionSets.FirstOrDefaultAsync(s => s.Id == questionSetId);
            if (set == null || !set.IsScore) { TempData["msg"] = "Bộ không hợp lệ/không bật tính điểm."; return RedirectToAction(nameof(AddQuestion), new { score, questionSetId = redirectSetId ?? questionSetId }); }
            if (set.IsLocked || q.IsLocked) { TempData["msg"] = "Không thể sửa vì đã khóa."; return RedirectToAction(nameof(AddQuestion), new { score, questionSetId = redirectSetId ?? questionSetId }); }

            // clamp hợp lệ (đã trừ điểm của chính câu này trong ComputeValidPointsAsync)
            q.MaxPoints = await ComputeValidPointsAsync(questionSetId, q.Id, requested);
            q.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["msg"] = $"Đã cập nhật điểm cho câu hỏi #{q.Id}.";
            return RedirectToAction(nameof(AddQuestion), new { score, questionSetId = redirectSetId ?? questionSetId });
        }

        [HttpPost("set-points")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPoints(
            int questionId,
            decimal score,
            [FromForm] int[] selectedOptionIds,
            int? redirectSetId,
            bool? scoreTab)
        {
            // Lấy câu hỏi + đáp án
            var question = await _db.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
                return NotFound("Không tìm thấy câu hỏi.");

            if (question.Type == "text")
                return BadRequest("Câu hỏi tự luận (text) không hỗ trợ gán điểm đáp án.");

            // Giới hạn score theo max_points nếu có
            if (question.MaxPoints.HasValue && score > question.MaxPoints.Value)
                score = question.MaxPoints.Value;

            // Chuẩn hoá list chọn
            var selected = (selectedOptionIds ?? System.Array.Empty<int>()).ToHashSet();

            // Gán điểm: chọn = score, không chọn = 0
            var now = DateTime.UtcNow;
            foreach (var ans in question.Answers)
            {
                var award = selected.Contains(ans.Id);
                ans.Points = award ? score : 0m;
                ans.IsExclusive = award;
                ans.UpdatedAt = now;
                // mark properties as modified for clarity
                _db.Entry(ans).Property(a => a.Points).IsModified = true;
                _db.Entry(ans).Property(a => a.IsExclusive).IsModified = true;
                _db.Entry(ans).Property(a => a.UpdatedAt).IsModified = true;
            }

            await _db.SaveChangesAsync();

            // If this was an AJAX request (client-side checkbox change), return JSON with updated answers
            if (Request.Headers.TryGetValue("X-Requested-With", out var hdr) && hdr == "XMLHttpRequest")
            {
                var answersOut = question.Answers.Select(a => new
                {
                    id = a.Id,
                    points = a.Points,
                    isExclusive = a.IsExclusive
                }).ToArray();

                return Json(new { ok = true, answers = answersOut });
            }

            // Otherwise redirect back to the management page preserving filters
            return RedirectToAction(
                actionName: "ManageQuestions",
                controllerName: "Home",
                routeValues: new
                {
                    questionSetId = redirectSetId ?? question.QuestionSetId,
                    score = scoreTab 
                });
        }

        // -----------------------------
        // VIDEO & POST MANAGEMENT
        // -----------------------------
        public IActionResult ManageVideo()
        {
            return View("Manage_Video_Post/video");
        }

        public IActionResult ManagePost()
        {
            return View("Manage_Video_Post/post");
        }
    }
}

namespace AdminPortal.ViewModels
{
    public class QuestionSetVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Hint { get; set; }          // map description

        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }

        // GIỮ NGUYÊN theo DB: isScore
        public bool IsScore { get; set; }

        // NEW: match DB (question_sets.max_score)
        public decimal MaxScore { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // tiện UI
        public bool IsHidden => !IsActive;
    }
}

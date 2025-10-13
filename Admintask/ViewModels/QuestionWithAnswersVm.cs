namespace AdminPortal.ViewModels
{
    public class QuestionWithAnswersVm
    {
        public int? QuestionId { get; set; }
        public int QuestionSetId { get; set; }

        public string Text { get; set; } = "";
        // single | multiple | text
        public string Type { get; set; } = "single";

        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public int OrderInSet { get; set; }
        public bool IsSetScored { get; set; }
        public decimal? MaxPoints { get; set; }
        public bool Skipped{ get; set; }

        public List<AnswerVm> Answers { get; set; } = new();
    }

    public class AnswerVm
    {
        public int? Id { get; set; }

        // DB: NCHAR(1) NOT NULL -> để string length=1
        public string? Label { get; set; } = ""; // validate length=1 ở layer service/controller

        public string Text { get; set; } = "";
        public string? Hint { get; set; }

        public int? OrderInQuestion { get; set; }  // DB: INT NULL

        // NEW: match DB
        public bool IsExclusive { get; set; }      // Answers.is_exclusive
        public bool IsActive { get; set; }         // Answers.is_active
        public decimal Points { get; set; }        // Answers.points
    }
}

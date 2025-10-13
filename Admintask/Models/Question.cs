using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminPortal.Models
{
    [Table("Questions")]
    public class Question
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("question_set_id")]
        public int QuestionSetId { get; set; }

        [Required]
        [Column("text")]
        public string Text { get; set; } = "";

        // single | multiple | text
        [Required]
        [Column("type")]
        public string Type { get; set; } = "single";

        [Column("order_in_set")]
        public int OrderInSet { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("is_locked")]
        public bool IsLocked { get; set; } = false;

        [Required]
        [Column("skipped")]
        public bool Skipped { get; set; } = false;

        [Column("max_points", TypeName = "decimal(5,2)")]
        public decimal? MaxPoints { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public List<Answer> Answers { get; set; } = new();

        public QuestionSet? QuestionSet { get; set; }
    }
}

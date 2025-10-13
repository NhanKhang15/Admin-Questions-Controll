using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminPortal.Models
{
    [Table("Answers")]
    public class Answer
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("question_id")]
        public int QuestionId { get; set; }

        [Required]
        [StringLength(1)]
        [Column("label", TypeName = "nchar(1)")]
        public string Label { get; set; } = "A";

        [Required]
        [Column("text")]
        public string Text { get; set; } = "";

        [Column("hint")]
        public string? Hint { get; set; }

        [Column("order_in_question")]
        public int? OrderInQuestion { get; set; }

        [Required]
        [Column("is_exclusive")]
        public bool IsExclusive { get; set; } = false;

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("points", TypeName = "decimal(5,2)")]
        public decimal Points { get; set; } = 0m;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Nav
        public Question? Question { get; set; }
    }
}

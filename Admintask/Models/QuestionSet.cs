using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminPortal.Models
{
    [Table("question_sets")]
    public class QuestionSet
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required, MaxLength(255)]
        [Column("name")]
        public string Name { get; set; } = "";

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("is_locked")]
        public bool IsLocked { get; set; } = false;

        // GIỮ NGUYÊN đúng DB
        [Required]
        [Column("isScore")]
        public bool IsScore { get; set; } = false;

        // NEW: match DB decimal(5,2)
        [Required]
        [Column("max_score", TypeName = "decimal(5,2)")]
        public decimal MaxScore { get; set; } = 10m;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminPortal.Models
{
    [Table("Posts")]
    public class Post
    {
        [Key]
        [Column("post_id")]
        public int PostId { get; set; }

        [Column("expert_id")]
        public int? ExpertId { get; set; }

        [Required]
        [Column("title")]
        [MaxLength(255)]
        public string Title { get; set; } = "";

        [Column("summary")]
        [MaxLength(500)]
        public string? Summary { get; set; }

        [Required]
        [Column("content")]
        public string Content { get; set; } = "";

        [Column("thumbnail_url")]
        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }

        [Column("is_premium")]
        public bool IsPremium { get; set; } = false;

        [Required]
        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "draft";

        [Column("published_at")]
        public DateTime? PublishedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("ExpertId")]
        public Expert? Expert { get; set; }

        public PostStats? Stats { get; set; }
        public ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}

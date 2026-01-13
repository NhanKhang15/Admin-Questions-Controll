using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminPortal.Models
{
    [Table("Videos")]
    public class Video
    {
        [Key]
        [Column("video_id")]
        public int VideoId { get; set; }

        [Column("expert_id")]
        public int? ExpertId { get; set; }

        [Required]
        [Column("title")]
        [MaxLength(255)]
        public string Title { get; set; } = "";

        [Column("description")]
        public string? Description { get; set; }

        [Column("thumbnail_url")]
        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }

        [Required]
        [Column("video_url")]
        [MaxLength(500)]
        public string VideoUrl { get; set; } = "";

        [Column("duration_seconds")]
        public int DurationSeconds { get; set; } = 0;

        [Column("is_short")]
        public bool IsShort { get; set; } = false;

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

        public VideoStats? Stats { get; set; }
        public ICollection<VideoCategory> VideoCategories { get; set; } = new List<VideoCategory>();
        public ICollection<VideoTag> VideoTags { get; set; } = new List<VideoTag>();
    }
}

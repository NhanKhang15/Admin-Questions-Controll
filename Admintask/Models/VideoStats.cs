using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminPortal.Models
{
    [Table("VideoStats")]
    public class VideoStats
    {
        [Key]
        [Column("video_id")]
        public int VideoId { get; set; }

        [Column("view_count")]
        public long ViewCount { get; set; } = 0;

        [Column("like_count")]
        public long LikeCount { get; set; } = 0;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("VideoId")]
        public Video Video { get; set; } = null!;
    }
}

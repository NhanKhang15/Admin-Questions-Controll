using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminPortal.Models
{
    [Table("PostStats")]
    public class PostStats
    {
        [Key]
        [Column("post_id")]
        public int PostId { get; set; }

        [Column("view_count")]
        public long ViewCount { get; set; } = 0;

        [Column("like_count")]
        public long LikeCount { get; set; } = 0;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("PostId")]
        public Post Post { get; set; } = null!;
    }
}

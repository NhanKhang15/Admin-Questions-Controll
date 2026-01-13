using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminPortal.Models
{
    [Table("Tags")]
    public class Tag
    {
        [Key]
        [Column("tag_id")]
        public int TagId { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(80)]
        public string Name { get; set; } = "";

        [Required]
        [Column("slug")]
        [MaxLength(120)]
        public string Slug { get; set; } = "";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<VideoTag> VideoTags { get; set; } = new List<VideoTag>();
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}

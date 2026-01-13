using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminPortal.Models
{
    [Table("ContentCategories")]
    public class ContentCategory
    {
        [Key]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(120)]
        public string Name { get; set; } = "";

        [Required]
        [Column("slug")]
        [MaxLength(160)]
        public string Slug { get; set; } = "";

        [Column("description")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<VideoCategory> VideoCategories { get; set; } = new List<VideoCategory>();
        public ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminPortal.Models
{
    [Table("Experts")]
    public class Expert
    {
        [Key]
        [Column("expert_id")]
        public int ExpertId { get; set; }

        [Required]
        [Column("full_name")]
        [MaxLength(100)]
        public string FullName { get; set; } = "";

        [Column("specialization")]
        [MaxLength(100)]
        public string? Specialization { get; set; }

        [Column("contact_info")]
        [MaxLength(255)]
        public string? ContactInfo { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<Video> Videos { get; set; } = new List<Video>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}

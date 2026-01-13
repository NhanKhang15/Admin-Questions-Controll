using System.ComponentModel.DataAnnotations.Schema;

namespace AdminPortal.Models
{
    [Table("PostCategories")]
    public class PostCategory
    {
        [Column("post_id")]
        public int PostId { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }

        // Navigation
        [ForeignKey("PostId")]
        public Post Post { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public ContentCategory Category { get; set; } = null!;
    }
}

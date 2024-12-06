using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Blog.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        
        // Arabic content
        [Required]
        public string TitleAr { get; set; } = string.Empty;
        [Required]
        public string ShortDescriptionAr { get; set; } = string.Empty;
        [Required]
        public string DescriptionAr { get; set; } = string.Empty;
        
        // English content
        [Required]
        public string TitleEn { get; set; } = string.Empty;
        [Required]
        public string ShortDescriptionEn { get; set; } = string.Empty;
        [Required]
        public string DescriptionEn { get; set; } = string.Empty;
        
        // Language-independent properties
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;
        
        [ForeignKey(nameof(ApplicationUserId))]
        public virtual ApplicationUser? ApplicationUser { get; set; }
        
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime? UpdateDate { get; set; }
        public string? Slug { get; set; }
        public string? ThumbnailUrl { get; set; }

        // Helper method to get thumbnail URL with fallback
        public string GetThumbnailUrl()
        {
            if (string.IsNullOrEmpty(ThumbnailUrl))
            {
                return "/images/blog/default.jpg";
            }
            return ThumbnailUrl;
        }

        // Category relationship
        [Required]
        public int CategoryId { get; set; }
        
        [ForeignKey(nameof(CategoryId))]
        public virtual Category? Category { get; set; }

        // Collections
        public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Status flags
        public bool IsPublished { get; set; }
        public int ViewCount { get; set; }
    }
}

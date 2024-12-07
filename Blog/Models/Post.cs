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
        [Required]
        public string ContentAr { get; set; } = string.Empty;
        
        // English content
        [Required]
        public string TitleEn { get; set; } = string.Empty;
        [Required]
        public string ShortDescriptionEn { get; set; } = string.Empty;
        [Required]
        public string DescriptionEn { get; set; } = string.Empty;
        [Required]
        public string ContentEn { get; set; } = string.Empty;
        
        // Language-independent properties
        public string? ThumbnailUrl { get; set; }
        
        [Required]
        public string Slug { get; set; } = string.Empty;
        
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime LastModifiedDate { get; set; }
        public int ViewCount { get; set; }
        public bool IsPublished { get; set; }
        
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;
        
        [ForeignKey(nameof(ApplicationUserId))]
        public virtual ApplicationUser? User { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        [ForeignKey(nameof(CategoryId))]
        public virtual Category? Category { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public string GetThumbnailUrl()
        {
            return ThumbnailUrl ?? "/images/default-thumbnail.jpg";
        }
    }
}

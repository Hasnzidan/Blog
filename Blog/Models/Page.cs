using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class Page
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public string? ShortDescription { get; set; }
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public string Slug { get; set; } = string.Empty;
        
        public string? ThumbnailUrl { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public bool Published { get; set; }
    }
}

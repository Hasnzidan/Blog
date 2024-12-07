using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class Page
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "English title is required")]
        public string TitleEn { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Arabic title is required")]
        public string TitleAr { get; set; } = string.Empty;
        
        public string? ShortDescriptionEn { get; set; }
        
        public string? ShortDescriptionAr { get; set; }
        
        [Required(ErrorMessage = "English content is required")]
        public string ContentEn { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Arabic content is required")]
        public string ContentAr { get; set; } = string.Empty;
        
        [Required]
        public string Slug { get; set; } = string.Empty;
        
        public string? ThumbnailUrl { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public bool Published { get; set; }

        // Helper properties for the view
        public string Title => System.Threading.Thread.CurrentThread.CurrentUICulture.Name.StartsWith("ar") ? TitleAr : TitleEn;
        public string Content => System.Threading.Thread.CurrentThread.CurrentUICulture.Name.StartsWith("ar") ? ContentAr : ContentEn;
        public string? ShortDescription => System.Threading.Thread.CurrentThread.CurrentUICulture.Name.StartsWith("ar") ? ShortDescriptionAr : ShortDescriptionEn;
        public string? Thumbnail => ThumbnailUrl;
    }
}

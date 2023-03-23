using Microsoft.Build.Framework;

namespace Blog.ViewModels
{
    public class PageVM
    {
        public int Id { get; set; }
        [Required]
        public string? Title { get; set; }
        public string? shortDescription { get; set; }
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public IFormFile? Thumbnail { get; set; }
    }
}

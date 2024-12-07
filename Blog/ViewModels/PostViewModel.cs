using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels
{
    public class PostViewModel
    {
        public int Id { get; set; }
        [Required]
        public string TitleEn { get; set; } = string.Empty;
        [Required]
        public string TitleAr { get; set; } = string.Empty;
        [Required]
        public string ShortDescriptionEn { get; set; } = string.Empty;
        [Required]
        public string ShortDescriptionAr { get; set; } = string.Empty;
        [Required]
        public string Slug { get; set; } = string.Empty;
        [Required]
        public string ThumbnailUrl { get; set; } = string.Empty;
        [Required]
        public string CategoryNameEn { get; set; } = string.Empty;
        [Required]
        public string CategoryNameAr { get; set; } = string.Empty;
        [Required]
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsPublished { get; set; }
        public int ViewCount { get; set; }
        public int CommentsCount { get; set; }
        public int CategoryId { get; set; }
    }
}

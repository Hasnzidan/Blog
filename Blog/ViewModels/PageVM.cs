using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Blog.ViewModels
{
    public class PageVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English title is required")]
        [Display(Name = "English Title")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic title is required")]
        [Display(Name = "Arabic Title")]
        public string TitleAr { get; set; } = string.Empty;

        [Display(Name = "English Short Description")]
        public string? ShortDescriptionEn { get; set; }

        [Display(Name = "Arabic Short Description")]
        public string? ShortDescriptionAr { get; set; }

        [Required(ErrorMessage = "English content is required")]
        [Display(Name = "English Content")]
        public string ContentEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic content is required")]
        [Display(Name = "Arabic Content")]
        public string ContentAr { get; set; } = string.Empty;

        public string? ThumbnailUrl { get; set; }

        [Display(Name = "Thumbnail")]
        public IFormFile? Thumbnail { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool Published { get; set; }

        public string Slug { get; set; } = string.Empty;

        // Helper properties for the view
        public string Title => System.Threading.Thread.CurrentThread.CurrentUICulture.Name.StartsWith("ar") ? TitleAr : TitleEn;
        public string Content => System.Threading.Thread.CurrentThread.CurrentUICulture.Name.StartsWith("ar") ? ContentAr : ContentEn;
        public string? ShortDescription => System.Threading.Thread.CurrentThread.CurrentUICulture.Name.StartsWith("ar") ? ShortDescriptionAr : ShortDescriptionEn;
    }
}

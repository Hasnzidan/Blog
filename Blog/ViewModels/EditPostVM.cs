using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Blog.Models;

namespace Blog.ViewModels
{
    public class EditPostVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Arabic title is required")]
        [Display(Name = "Arabic Title")]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English title is required")]
        [Display(Name = "English Title")]
        public string TitleEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic short description is required")]
        [Display(Name = "Arabic Short Description")]
        public string ShortDescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English short description is required")]
        [Display(Name = "English Short Description")]
        public string ShortDescriptionEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic description is required")]
        [Display(Name = "Arabic Description")]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English description is required")]
        [Display(Name = "English Description")]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public IFormFile? Thumbnail { get; set; }
        public string? ThumbnailUrl { get; set; }

        public List<Category>? Categories { get; set; }
        public List<Tag>? Tags { get; set; }
        public List<int> SelectedTags { get; set; } = new();
    }
}

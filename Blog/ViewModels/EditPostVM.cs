using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Blog.Models;
using System.Collections.Generic;

namespace Blog.ViewModels
{
    public class EditPostVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Arabic title is required")]
        [Display(Name = "Arabic Title")]
        public string? TitleAr { get; set; }

        [Required(ErrorMessage = "English title is required")]
        [Display(Name = "English Title")]
        public string? TitleEn { get; set; }

        [Required(ErrorMessage = "Arabic short description is required")]
        [Display(Name = "Arabic Short Description")]
        public string? ShortDescriptionAr { get; set; }

        [Required(ErrorMessage = "English short description is required")]
        [Display(Name = "English Short Description")]
        public string? ShortDescriptionEn { get; set; }

        [Required(ErrorMessage = "Arabic description is required")]
        [Display(Name = "Arabic Description")]
        public string? DescriptionAr { get; set; }

        [Required(ErrorMessage = "English description is required")]
        [Display(Name = "English Description")]
        public string? DescriptionEn { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public IFormFile? Thumbnail { get; set; }
        public string? ThumbnailUrl { get; set; }

        public List<Category>? Categories { get; set; }
    }
}

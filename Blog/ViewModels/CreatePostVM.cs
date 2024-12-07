using Blog.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Blog.ViewModels
{
    public class CreatePostVM
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

        [Required(ErrorMessage = "Arabic content is required")]
        [Display(Name = "Arabic Content")]
        public string ContentAr { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "English content is required")]
        [Display(Name = "English Content")]
        public string ContentEn { get; set; } = string.Empty;
        
        public string? ApplicationUserId { get; set; }
        
        public string? ThumbnailUrl { get; set; }
        
        public IFormFile? Thumbnail { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public string Slug { get; set; } = string.Empty;

        public List<Category>? Categories { get; set; }
    }
}

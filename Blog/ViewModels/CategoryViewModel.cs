using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        [Required]
        public string NameEn { get; set; } = string.Empty;
        [Required]
        public string NameAr { get; set; } = string.Empty;
        public int PostCount { get; set; }
    }
}

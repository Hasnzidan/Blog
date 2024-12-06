using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        
        // Arabic content
        [Required]
        public string NameAr { get; set; } = string.Empty;
        public string? DescriptionAr { get; set; }
        
        // English content
        [Required]
        public string NameEn { get; set; } = string.Empty;
        public string? DescriptionEn { get; set; }
        
        // Language-independent properties
        [Required]
        public string Slug { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

        // Helper properties
        public string Name 
        { 
            get => System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "ar-SA" ? NameAr : NameEn;
            set
            {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "ar-SA")
                    NameAr = value;
                else
                    NameEn = value;
            }
        }

        public string? Description 
        { 
            get => System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "ar-SA" ? DescriptionAr : DescriptionEn;
            set
            {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "ar-SA")
                    DescriptionAr = value;
                else
                    DescriptionEn = value;
            }
        }
    }
}

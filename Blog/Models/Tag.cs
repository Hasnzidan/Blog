using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        public string NameAr { get; set; } = string.Empty;

        [Required]
        public string NameEn { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}

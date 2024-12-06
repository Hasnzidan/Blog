using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Models
{
    public class PostTag
    {
        [Required]
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public Post? Post { get; set; }

        [Required]
        public int TagId { get; set; }

        [ForeignKey("TagId")]
        public Tag? Tag { get; set; }
    }
}

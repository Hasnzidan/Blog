using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic; // Added this line

namespace Blog.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Navigation property for Posts
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}

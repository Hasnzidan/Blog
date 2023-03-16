using Microsoft.Build.Framework;

namespace Blog.ViewModels
{
    public class LoginVM
    {
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? Password { get; set; }
        public bool RememberM { get; set; }
    }
}

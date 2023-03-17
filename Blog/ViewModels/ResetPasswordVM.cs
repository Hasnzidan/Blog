using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels
{
    public class ResetPasswordVM
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        [Required]
        public string? NewPassword { get; set; }
        [Compare(nameof(NewPassword))]
        [Required]
        public string? confirmPassword { get; set; }
    }
}

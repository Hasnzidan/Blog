using System.Collections.Generic;

namespace Blog.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<PostViewModel> Posts { get; set; } = new List<PostViewModel>();
        public IEnumerable<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public int? SelectedCategoryId { get; set; }
    }
}

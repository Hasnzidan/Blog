using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels
{
    public class SearchViewModel
    {
        public string Query { get; set; } = string.Empty;
        public List<PostViewModel> Posts { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
    }
}

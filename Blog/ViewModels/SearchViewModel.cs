using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels
{
    public class SearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Author { get; set; }
        
        [Display(Name = "From Date")]
        public DateTime? FromDate { get; set; }
        
        [Display(Name = "To Date")]
        public DateTime? ToDate { get; set; }
        
        // Advanced filters
        public int? CategoryId { get; set; }
        public List<int> SelectedTags { get; set; } = new();
        public string? SortBy { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        // For dropdowns and suggestions
        public List<CategoryViewModel> Categories { get; set; } = new();
        public List<TagViewModel> Tags { get; set; } = new();
        public List<string> SearchSuggestions { get; set; } = new();
        
        public List<PostViewModel> Posts { get; set; } = new();
        public int TotalPosts { get; set; }
        
        // User preferences
        public bool SavePreferences { get; set; }
        public string? PreferredCategory { get; set; }
        public List<string> PreferredTags { get; set; } = new();
    }

    public class PostViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? Author { get; set; }
        public DateTime CreateDate { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Slug { get; set; }
        public string? Category { get; set; }
        public List<string> Tags { get; set; } = new();
        public string? HighlightedContent { get; set; }
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

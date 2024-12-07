namespace Blog.ViewModels
{
    public class DashboardViewModel
    {
        public int UsersCount { get; set; }
        public int PostsCount { get; set; }
        public int PublishedPostsCount { get; set; }
        public int CategoriesCount { get; set; }
        public int CommentsCount { get; set; }
        public int PendingCommentsCount { get; set; }
        public int TotalViews { get; set; }
        public List<DashboardPostViewModel> LatestPosts { get; set; } = new();
        public List<DashboardPostViewModel> TopPosts { get; set; } = new();
        public List<CommentViewModel> LatestComments { get; set; } = new();
    }

    public class DashboardPostViewModel
    {
        public int Id { get; set; }
        public string TitleAr { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string CategoryNameAr { get; set; } = string.Empty;
        public string CategoryNameEn { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsPublished { get; set; }
        public int ViewCount { get; set; }
        public int CommentsCount { get; set; }
    }

    public class CommentViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string PostTitleAr { get; set; } = string.Empty;
        public string PostTitleEn { get; set; } = string.Empty;
        public string PostSlug { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
    }
}

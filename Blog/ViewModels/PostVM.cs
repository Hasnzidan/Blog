namespace Blog.ViewModels
{
    public class PostVM
    {
        public int Id { get; set; }
        public string TitleAr { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}

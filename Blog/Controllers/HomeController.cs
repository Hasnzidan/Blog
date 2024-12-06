using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Blog.Models;
using Blog.Data;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IActionResult> Index(string? categorySlug = null)
        {
            try
            {
                var postsQuery = _context.Posts
                    .Include(p => p.ApplicationUser)
                    .Include(p => p.Category)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(categorySlug))
                {
                    postsQuery = postsQuery.Where(p => p.Category != null && p.Category.Slug == categorySlug);
                }

                var posts = await postsQuery
                    .OrderByDescending(p => p.CreateDate)
                    .Select(p => new Post
                    {
                        Id = p.Id,
                        TitleAr = p.TitleAr ?? string.Empty,
                        TitleEn = p.TitleEn ?? string.Empty,
                        Slug = p.Slug ?? string.Empty,
                        ShortDescriptionAr = p.ShortDescriptionAr ?? string.Empty,
                        ShortDescriptionEn = p.ShortDescriptionEn ?? string.Empty,
                        DescriptionAr = p.DescriptionAr ?? string.Empty,
                        DescriptionEn = p.DescriptionEn ?? string.Empty,
                        ThumbnailUrl = !string.IsNullOrEmpty(p.ThumbnailUrl) ? p.ThumbnailUrl : "/images/categories/default.jpg",
                        CreateDate = p.CreateDate,
                        Category = p.Category,
                        ApplicationUser = p.ApplicationUser,
                        PostTags = p.PostTags ?? new List<PostTag>()
                    })
                    .ToListAsync();

                // Get all categories for the filter
                ViewBag.Categories = await _context.Categories
                    .Where(c => !string.IsNullOrEmpty(c.Name) && !string.IsNullOrEmpty(c.Slug))
                    .ToListAsync();

                // Set default thumbnail based on category
                foreach (var post in posts)
                {
                    if (string.IsNullOrEmpty(post.ThumbnailUrl))
                    {
                        var postCategorySlug = post.Category?.Slug?.ToLower() ?? string.Empty;
                        post.ThumbnailUrl = postCategorySlug switch
                        {
                            "technology" => "/images/categories/technology.jpg",
                            "programming" => "/images/categories/programming.jpg",
                            "self-development" => "/images/categories/self-development.jpg",
                            "entrepreneurship" => "/images/categories/entrepreneurship.jpg",
                            _ => "/images/categories/default.jpg"
                        };
                    }
                }

                return View(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching posts");
                return View(new List<Post>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
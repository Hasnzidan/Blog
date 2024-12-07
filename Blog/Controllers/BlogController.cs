using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BlogController> _logger;

        public BlogController(ApplicationDbContext context, ILogger<BlogController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            try
            {
                var posts = await _context.Posts
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .Where(p => p.IsPublished)
                    .OrderByDescending(p => p.CreateDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var totalPosts = await _context.Posts.CountAsync(p => p.IsPublished);
                ViewBag.TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);
                ViewBag.CurrentPage = page;

                return View(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching blog posts");
                return View(new List<Post>());
            }
        }

        public async Task<IActionResult> Details(string slug)
        {
            try
            {
                if (string.IsNullOrEmpty(slug))
                {
                    return NotFound();
                }

                var post = await _context.Posts
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);

                if (post == null)
                {
                    return NotFound();
                }

                // Increment view count
                post.ViewCount++;
                await _context.SaveChangesAsync();

                return View(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching blog post details");
                return NotFound();
            }
        }
    }
}

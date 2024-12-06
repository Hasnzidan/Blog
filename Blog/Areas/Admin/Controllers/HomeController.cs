using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Blog.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Blog.Models;
using Blog.ViewModels;
using Microsoft.Extensions.Logging;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<HomeController> _logger;
        private readonly INotyfService _notification;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<HomeController> logger,
            INotyfService notification)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notification = notification ?? throw new ArgumentNullException(nameof(notification));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var isRtl = System.Threading.Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft;
                var dashboardViewModel = new DashboardViewModel
                {
                    UsersCount = await _userManager.Users.CountAsync(),
                    PostsCount = await _context.Posts.CountAsync(),
                    CategoriesCount = await _context.Categories.CountAsync(),
                    TagsCount = await _context.Tags.CountAsync(),
                    CommentsCount = await _context.Comments.CountAsync(),
                    PublishedPostsCount = await _context.Posts.CountAsync(p => p.IsPublished),
                    PendingCommentsCount = await _context.Comments.CountAsync(c => !c.IsApproved),
                    TotalViews = await _context.Posts.SumAsync(p => p.ViewCount),

                    LatestPosts = await _context.Posts
                        .Include(p => p.Category)
                        .Include(p => p.Comments)
                        .OrderByDescending(p => p.CreateDate)
                        .Take(5)
                        .Select(p => new DashboardPostViewModel
                        {
                            Id = p.Id,
                            TitleAr = p.TitleAr,
                            TitleEn = p.TitleEn,
                            CategoryNameAr = p.Category.NameAr,
                            CategoryNameEn = p.Category.NameEn,
                            Slug = p.Slug,
                            CreatedDate = p.CreateDate,
                            IsPublished = p.IsPublished,
                            ViewCount = p.ViewCount,
                            CommentsCount = p.Comments.Count
                        })
                        .ToListAsync(),

                    LatestComments = await _context.Comments
                        .Include(c => c.User)
                        .Include(c => c.Post)
                        .OrderByDescending(c => c.CreatedDate)
                        .Take(5)
                        .Select(c => new CommentViewModel
                        {
                            Id = c.Id,
                            UserName = c.User.UserName ?? string.Empty,
                            Content = c.Content,
                            CreatedDate = c.CreatedDate,
                            PostTitleAr = c.Post.TitleAr,
                            PostTitleEn = c.Post.TitleEn,
                            PostSlug = c.Post.Slug,
                            IsApproved = c.IsApproved
                        })
                        .ToListAsync(),

                    PostsByCategory = await _context.Posts
                        .Where(p => p.IsPublished)
                        .GroupBy(p => isRtl ? p.Category.NameAr : p.Category.NameEn)
                        .Select(g => new { Category = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.Category, x => x.Count),

                    PostsByTag = await _context.PostTags
                        .GroupBy(pt => isRtl ? pt.Tag.NameAr : pt.Tag.NameEn)
                        .Select(g => new { Tag = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.Tag, x => x.Count),
                        
                    TopPosts = await _context.Posts
                        .Where(p => p.IsPublished)
                        .OrderByDescending(p => p.ViewCount)
                        .Take(5)
                        .Select(p => new DashboardPostViewModel
                        {
                            Id = p.Id,
                            TitleAr = p.TitleAr,
                            TitleEn = p.TitleEn,
                            ViewCount = p.ViewCount,
                            CommentsCount = p.Comments.Count
                        })
                        .ToListAsync()
                };

                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading admin dashboard");
                _notification.Error("Failed to load dashboard data");
                return View(new DashboardViewModel());
            }
        }
    }
}

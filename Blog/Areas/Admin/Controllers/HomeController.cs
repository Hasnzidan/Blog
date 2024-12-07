using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<HomeController> logger, INotyfService notification)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _notification = notification;
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
                            TitleAr = p.TitleAr == null ? string.Empty : p.TitleAr,
                            TitleEn = p.TitleEn == null ? string.Empty : p.TitleEn,
                            CategoryNameAr = p.Category == null ? string.Empty : (p.Category.NameAr == null ? string.Empty : p.Category.NameAr),
                            CategoryNameEn = p.Category == null ? string.Empty : (p.Category.NameEn == null ? string.Empty : p.Category.NameEn),
                            Slug = p.Slug == null ? string.Empty : p.Slug,
                            CreatedDate = p.CreateDate,
                            IsPublished = p.IsPublished,
                            ViewCount = p.ViewCount,
                            CommentsCount = p.Comments == null ? 0 : p.Comments.Count
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
                            UserName = c.User == null ? string.Empty : (c.User.UserName == null ? string.Empty : c.User.UserName),
                            Content = c.Content == null ? string.Empty : c.Content,
                            CreatedDate = c.CreatedDate,
                            PostTitleAr = c.Post == null ? string.Empty : (c.Post.TitleAr == null ? string.Empty : c.Post.TitleAr),
                            PostTitleEn = c.Post == null ? string.Empty : (c.Post.TitleEn == null ? string.Empty : c.Post.TitleEn),
                            PostSlug = c.Post == null ? string.Empty : (c.Post.Slug == null ? string.Empty : c.Post.Slug),
                            IsApproved = c.IsApproved
                        })
                        .ToListAsync(),

                    TopPosts = await _context.Posts
                        .Where(p => p.IsPublished)
                        .OrderByDescending(p => p.ViewCount)
                        .Take(5)
                        .Select(p => new DashboardPostViewModel
                        {
                            Id = p.Id,
                            TitleAr = p.TitleAr == null ? string.Empty : p.TitleAr,
                            TitleEn = p.TitleEn == null ? string.Empty : p.TitleEn,
                            ViewCount = p.ViewCount,
                            CommentsCount = p.Comments == null ? 0 : p.Comments.Count
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

using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;

namespace Blog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId = null)
        {
            var currentCulture = CultureInfo.CurrentUICulture.Name;
            var isArabic = currentCulture == "ar-SA";

            var query = _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .Where(p => p.IsPublished);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var posts = await query
                .OrderByDescending(p => p.CreateDate)
                .Select(p => new PostViewModel
                {
                    Id = p.Id,
                    TitleEn = p.TitleEn ?? string.Empty,
                    TitleAr = p.TitleAr ?? string.Empty,
                    ShortDescriptionEn = p.ShortDescriptionEn ?? string.Empty,
                    ShortDescriptionAr = p.ShortDescriptionAr ?? string.Empty,
                    CategoryNameEn = p.Category == null ? string.Empty : p.Category.NameEn,
                    CategoryNameAr = p.Category == null ? string.Empty : p.Category.NameAr,
                    AuthorName = p.User == null ? string.Empty : $"{p.User.FirstName} {p.User.LastName}",
                    ThumbnailUrl = string.IsNullOrEmpty(p.ThumbnailUrl) ? "/uploads/thumbnails/default.jpg" : p.ThumbnailUrl,
                    Slug = p.Slug ?? string.Empty,
                    CreatedDate = p.CreateDate,
                    IsPublished = p.IsPublished,
                    ViewCount = p.ViewCount,
                    CategoryId = p.CategoryId
                })
                .ToListAsync();

            var categories = await _context.Categories
                .OrderBy(c => isArabic ? c.NameAr : c.NameEn)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    NameEn = c.NameEn ?? string.Empty,
                    NameAr = c.NameAr ?? string.Empty
                })
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                Posts = posts,
                Categories = categories,
                SelectedCategoryId = categoryId
            };

            return View(viewModel);
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
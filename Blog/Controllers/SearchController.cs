using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Blog.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SearchController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Json(new { success = false, message = "Please enter a search term" });
                }

                var currentCulture = CultureInfo.CurrentUICulture.Name;
                var isArabic = currentCulture == "ar-SA";

                var posts = await _context.Posts
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .Where(p => (p.IsPublished && (
                           (p.TitleEn ?? string.Empty).Contains(query) || 
                           (p.TitleAr ?? string.Empty).Contains(query) ||
                           (p.ShortDescriptionEn ?? string.Empty).Contains(query) ||
                           (p.ShortDescriptionAr ?? string.Empty).Contains(query) ||
                           (p.DescriptionEn ?? string.Empty).Contains(query) ||
                           (p.DescriptionAr ?? string.Empty).Contains(query))))
                    .OrderByDescending(p => p.CreateDate)
                    .Take(10)
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
                        ViewCount = p.ViewCount
                    })
                    .ToListAsync();

                return Json(new { success = true, data = posts });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

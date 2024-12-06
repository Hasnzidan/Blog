using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.Extensions.Localization;

namespace Blog.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly INotyfService _notification;
        private readonly IStringLocalizer<PostController> _localizer;

        public PostController(
            ApplicationDbContext context, 
            IWebHostEnvironment environment,
            INotyfService notification,
            IStringLocalizer<PostController> localizer)
        {
            _context = context;
            _environment = environment;
            _notification = notification;
            _localizer = localizer;
        }

        // GET: Post/Create
        public IActionResult Create()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                _notification.Warning(_localizer["PleaseLoginToCreatePost"]);
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Create", "Post") });
            }

            var categories = _context.Categories.ToList();
            var tags = _context.Tags.ToList();

            if (!categories.Any())
            {
                _notification.Warning(_localizer["PleaseCreateCategories"]);
                return RedirectToAction("Index", "Category", new { area = "Admin" });
            }

            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.Tags = new SelectList(tags, "Id", "Name");
            return View();
        }

        // POST: Post/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TitleAr,TitleEn,ShortDescriptionAr,ShortDescriptionEn,DescriptionAr,DescriptionEn,CategoryId")] Post post, 
            IFormFile? thumbnail, int[]? selectedTags)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                _notification.Warning(_localizer["PleaseLoginToCreatePost"]);
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Create", "Post") });
            }

            if (ModelState.IsValid && post.TitleEn != null && post.TitleAr != null)
            {
                // Set creation date and author
                post.CreateDate = DateTime.Now;
                post.Slug = GenerateSlug(post.TitleEn); // Using English title for slug
                post.ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (thumbnail != null)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "blog");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + thumbnail.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await thumbnail.CopyToAsync(fileStream);
                    }

                    post.ThumbnailUrl = "/images/blog/" + uniqueFileName;
                }

                // Add selected tags
                if (selectedTags != null)
                {
                    foreach (var tagId in selectedTags)
                    {
                        var tag = await _context.Tags.FindAsync(tagId);
                        if (tag != null)
                        {
                            post.PostTags.Add(new PostTag { Post = post, Tag = tag });
                        }
                    }
                }

                _context.Add(post);
                await _context.SaveChangesAsync();

                _notification.Success(_localizer["PostCreatedSuccessfully"]);
                return RedirectToAction("Detail", new { slug = post.Slug });
            }

            var categories = _context.Categories.ToList();
            var tags = _context.Tags.ToList();

            ViewBag.Categories = new SelectList(categories, "Id", "Name", post.CategoryId);
            ViewBag.Tags = new SelectList(tags, "Id", "Name");
            return View(post);
        }

        // GET: Post/Detail/slug
        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                _notification.Error(_localizer["PostNotFound"]);
                return RedirectToAction("Index", "Home");
            }

            var post = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.ApplicationUser)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (post == null)
            {
                _notification.Error(_localizer["PostNotFound"]);
                return RedirectToAction("Index", "Home");
            }

            return View(post);
        }

        private string GenerateSlug(string? title)
        {
            if (string.IsNullOrEmpty(title))
                return string.Empty;

            string slug = title.ToLower()
                .Replace(" ", "-")
                .Replace("&", "and")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("?", "")
                .Replace("!", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("/", "-")
                .Replace("\\", "-");

            // Remove any double dashes
            while (slug.Contains("--"))
            {
                slug = slug.Replace("--", "-");
            }

            // Remove trailing dash
            if (slug.EndsWith("-"))
                slug = slug.Substring(0, slug.Length - 1);

            return slug;
        }

        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                   + "_" + Guid.NewGuid().ToString().Substring(0, 4)
                   + Path.GetExtension(fileName);
        }
    }
}

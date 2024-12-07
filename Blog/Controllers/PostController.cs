using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
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

            if (!categories.Any())
            {
                _notification.Warning(_localizer["PleaseCreateCategories"]);
                return RedirectToAction("Index", "Category", new { area = "Admin" });
            }

            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: Post/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TitleAr,TitleEn,ShortDescriptionAr,ShortDescriptionEn,DescriptionAr,DescriptionEn,CategoryId")] Post post, IFormFile? thumbnail)
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

                _context.Add(post);
                await _context.SaveChangesAsync();

                _notification.Success(_localizer["PostCreatedSuccessfully"]);
                return RedirectToAction("Detail", new { slug = post.Slug });
            }

            var categories = _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", post.CategoryId);
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
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (post == null)
            {
                _notification.Error(_localizer["PostNotFound"]);
                return RedirectToAction("Index", "Home");
            }

            return View(post);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null || post.ApplicationUserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }

            var editVM = new EditPostVM
            {
                Id = post.Id,
                TitleAr = post.TitleAr,
                TitleEn = post.TitleEn,
                ShortDescriptionAr = post.ShortDescriptionAr,
                ShortDescriptionEn = post.ShortDescriptionEn,
                DescriptionAr = post.DescriptionAr,
                DescriptionEn = post.DescriptionEn,
                CategoryId = post.CategoryId,
                ThumbnailUrl = post.ThumbnailUrl
            };

            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", post.CategoryId);

            return View(editVM);
        }

        // POST: Post/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPostVM model, IFormFile? thumbnail)
        {
            if (ModelState.IsValid)
            {
                var post = await _context.Posts.FindAsync(model.Id);
                if (post == null || post.ApplicationUserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                {
                    return NotFound();
                }

                post.TitleAr = model.TitleAr ?? string.Empty;
                post.TitleEn = model.TitleEn ?? string.Empty;
                post.ShortDescriptionAr = model.ShortDescriptionAr ?? string.Empty;
                post.ShortDescriptionEn = model.ShortDescriptionEn ?? string.Empty;
                post.DescriptionAr = model.DescriptionAr ?? string.Empty;
                post.DescriptionEn = model.DescriptionEn ?? string.Empty;
                post.CategoryId = model.CategoryId;
                post.Slug = GenerateSlug(model.TitleEn ?? string.Empty);

                if (thumbnail != null)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "blog");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + thumbnail.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await thumbnail.CopyToAsync(fileStream);
                    }

                    // Delete old thumbnail if exists
                    if (!string.IsNullOrEmpty(post.ThumbnailUrl))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, post.ThumbnailUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    post.ThumbnailUrl = "/images/blog/" + uniqueFileName;
                }

                await _context.SaveChangesAsync();
                _notification.Success(_localizer["PostUpdatedSuccessfully"]);
                return RedirectToAction("Detail", new { slug = post.Slug });
            }

            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", model.CategoryId);
            return View(model);
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

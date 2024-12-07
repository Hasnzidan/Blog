using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreateDate)
                .Select(p => new PostViewModel
                {
                    Id = p.Id,
                    TitleEn = p.TitleEn ?? string.Empty,
                    TitleAr = p.TitleAr ?? string.Empty,
                    CategoryNameEn = p.Category == null ? string.Empty : p.Category.NameEn,
                    CategoryNameAr = p.Category == null ? string.Empty : p.Category.NameAr,
                    AuthorName = p.User == null ? string.Empty : $"{p.User.FirstName} {p.User.LastName}",
                    CreatedDate = p.CreateDate,
                    IsPublished = p.IsPublished,
                    ViewCount = p.ViewCount,
                    CommentsCount = p.Comments == null ? 0 : p.Comments.Count
                })
                .ToListAsync();

            return View(posts);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "NameEn");
            return View(new CreatePostVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostVM model)
        {
            if (ModelState.IsValid)
            {
                // Generate slug from English title
                var slug = model.TitleEn.ToLower().Replace(" ", "-");
                
                var post = new Post
                {
                    TitleEn = model.TitleEn,
                    TitleAr = model.TitleAr,
                    ShortDescriptionEn = model.ShortDescriptionEn,
                    ShortDescriptionAr = model.ShortDescriptionAr,
                    DescriptionEn = model.DescriptionEn,
                    DescriptionAr = model.DescriptionAr,
                    ContentEn = model.ContentEn,
                    ContentAr = model.ContentAr,
                    CategoryId = model.CategoryId,
                    ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                    CreateDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    IsPublished = true,
                    Slug = slug
                };

                if (model.Thumbnail != null)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "thumbnails");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Thumbnail.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Thumbnail.CopyToAsync(fileStream);
                    }

                    post.ThumbnailUrl = "/uploads/thumbnails/" + uniqueFileName;
                }

                try
                {
                    _context.Add(post);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving post: " + ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "NameEn", model.CategoryId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            var model = new CreatePostVM
            {
                Id = post.Id,
                TitleEn = post.TitleEn,
                TitleAr = post.TitleAr,
                ShortDescriptionEn = post.ShortDescriptionEn,
                ShortDescriptionAr = post.ShortDescriptionAr,
                DescriptionEn = post.DescriptionEn,
                DescriptionAr = post.DescriptionAr,
                ContentEn = post.ContentEn,
                ContentAr = post.ContentAr,
                CategoryId = post.CategoryId,
                ThumbnailUrl = post.ThumbnailUrl
            };

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "NameEn", model.CategoryId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreatePostVM model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var post = await _context.Posts.FindAsync(id);
                    if (post == null)
                    {
                        return NotFound();
                    }

                    // Safely assign values with null checks
                    post.TitleEn = model?.TitleEn ?? string.Empty;
                    post.TitleAr = model?.TitleAr ?? string.Empty;
                    post.ShortDescriptionEn = model?.ShortDescriptionEn ?? string.Empty;
                    post.ShortDescriptionAr = model?.ShortDescriptionAr ?? string.Empty;
                    post.DescriptionEn = model?.DescriptionEn ?? string.Empty;
                    post.DescriptionAr = model?.DescriptionAr ?? string.Empty;
                    post.ContentEn = model?.ContentEn ?? string.Empty;
                    post.ContentAr = model?.ContentAr ?? string.Empty;
                    post.CategoryId = model?.CategoryId ?? post.CategoryId; // Keep existing CategoryId if new one is null
                    post.LastModifiedDate = DateTime.UtcNow;

                    if (model?.Thumbnail != null)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "thumbnails");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Thumbnail.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Thumbnail.CopyToAsync(fileStream);
                        }

                        // Delete old thumbnail if exists
                        if (!string.IsNullOrEmpty(post.ThumbnailUrl))
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, 
                                post.ThumbnailUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        post.ThumbnailUrl = "/uploads/thumbnails/" + uniqueFileName;
                    }

                    try
                    {
                        _context.Update(post);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error updating post: " + ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error finding post: " + ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "NameEn", model.CategoryId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return Json(new { success = false, message = "Post not found." });
            }

            try
            {
                if (!string.IsNullOrEmpty(post.ThumbnailUrl))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, 
                        post.ThumbnailUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error occurred while deleting post: " + ex.Message });
            }
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> TogglePublish(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return Json(new { success = false, message = "Post not found." });
            }

            try
            {
                post.IsPublished = !post.IsPublished;
                post.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Json(new { success = true, isPublished = post.IsPublished });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error occurred while updating post status: " + ex.Message });
            }
        }
    }
}

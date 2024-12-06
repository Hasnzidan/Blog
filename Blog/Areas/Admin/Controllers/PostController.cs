using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PostController> _logger;

        public PostController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager,
            ILogger<PostController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var posts = await _context.Posts!
                    .Include(p => p.Category)
                    .Include(p => p.ApplicationUser)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .OrderByDescending(p => p.CreateDate)
                    .ToListAsync();

                var listofPostsVM = posts.Select(x => new PostVM
                {
                    Id = x.Id,
                    TitleAr = x.TitleAr ?? string.Empty,
                    TitleEn = x.TitleEn ?? string.Empty,
                    ThumbnailUrl = x.ThumbnailUrl != null ? x.ThumbnailUrl : "/images/blog/default.jpg",
                    CreatedDate = x.CreateDate,
                    AuthorName = x.ApplicationUser != null 
                        ? $"{x.ApplicationUser.FirstName} {x.ApplicationUser.LastName}".Trim()
                        : "Unknown Author",
                    Category = x.Category != null ? x.Category.Name : "Uncategorized",
                    Tags = x.PostTags != null 
                        ? x.PostTags.Where(pt => pt.Tag != null).Select(pt => pt.Tag.NameEn).Where(n => !string.IsNullOrEmpty(n)).ToList() 
                        : new List<string>()
                }).ToList();

                return View(listofPostsVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching posts");
                TempData["Error"] = "An error occurred while loading posts.";
                return View(new List<PostVM>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var categories = await _context.Categories!
                    .Where(c => c.Name != null)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                    
                var tags = await _context.Tags!
                    .Where(t => t.NameEn != null)
                    .OrderBy(t => t.NameEn)
                    .ToListAsync();

                var model = new CreatePostVM
                {
                    Categories = categories,
                    Tags = tags
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing create post form");
                TempData["Error"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories!
                    .Where(c => c.Name != null)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                    
                model.Tags = await _context.Tags!
                    .Where(t => t.NameEn != null)
                    .OrderBy(t => t.NameEn)
                    .ToListAsync();
                return View(model);
            }

            try
            {
                var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
                if (loggedInUser == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                if (await _context.Posts?.AnyAsync(x => x.TitleAr == model.TitleAr || x.TitleEn == model.TitleEn) == true)
                {
                    TempData["Error"] = "Post with the same title already exists";
                    return RedirectToAction(nameof(Index));
                }

                var post = new Post
                {
                    TitleAr = model.TitleAr?.Trim() ?? string.Empty,
                    TitleEn = model.TitleEn?.Trim() ?? string.Empty,
                    ShortDescriptionAr = model.ShortDescriptionAr?.Trim() ?? string.Empty,
                    ShortDescriptionEn = model.ShortDescriptionEn?.Trim() ?? string.Empty,
                    DescriptionAr = model.DescriptionAr?.Trim() ?? string.Empty,
                    DescriptionEn = model.DescriptionEn?.Trim() ?? string.Empty,
                    ApplicationUserId = loggedInUser.Id,
                    CategoryId = model.CategoryId,
                    CreateDate = DateTime.Now
                };

                // Generate slug
                if (!string.IsNullOrEmpty(model.TitleAr))
                {
                    string slug = model.TitleAr.Trim().ToLower();
                    slug = slug.Replace(" ", "-");
                    post.Slug = $"{slug}-{Guid.NewGuid()}";
                }
                else if (!string.IsNullOrEmpty(model.TitleEn))
                {
                    string slug = model.TitleEn.Trim().ToLower();
                    slug = slug.Replace(" ", "-");
                    post.Slug = $"{slug}-{Guid.NewGuid()}";
                }

                // Handle thumbnail
                if (model.Thumbnail != null)
                {
                    post.ThumbnailUrl = UploadImage(model.Thumbnail);
                }

                // Handle tags
                if (model.SelectedTags != null && model.SelectedTags.Length > 0)
                {
                    foreach (var tagId in model.SelectedTags)
                    {
                        var postTag = new PostTag
                        {
                            PostId = post.Id,
                            TagId = tagId
                        };
                        await _context.PostTags!.AddAsync(postTag);
                    }
                }

                await _context.Posts!.AddAsync(post);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Post created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating post");
                ModelState.AddModelError("", "An error occurred while saving the post.");
                model.Categories = await _context.Categories!
                    .Where(c => c.Name != null)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                    
                model.Tags = await _context.Tags!
                    .Where(t => t.NameEn != null)
                    .OrderBy(t => t.NameEn)
                    .ToListAsync();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var post = await _context.Posts
                    .Include(p => p.Category)
                    .Include(p => p.ApplicationUser)
                    .Include(p => p.PostTags)
                    .ThenInclude(p => p.Tag)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (post == null)
                {
                    return NotFound();
                }

                var categories = await _context.Categories!
                    .Where(c => c.Name != null)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                    
                var tags = await _context.Tags!
                    .Where(t => t.NameEn != null)
                    .OrderBy(t => t.NameEn)
                    .ToListAsync();

                var model = new EditPostVM
                {
                    Id = post.Id,
                    TitleAr = post.TitleAr,
                    TitleEn = post.TitleEn,
                    ShortDescriptionAr = post.ShortDescriptionAr,
                    ShortDescriptionEn = post.ShortDescriptionEn,
                    DescriptionAr = post.DescriptionAr,
                    DescriptionEn = post.DescriptionEn,
                    ThumbnailUrl = post.ThumbnailUrl != null ? post.ThumbnailUrl : "/images/blog/default.jpg",
                    CategoryId = post.CategoryId,
                    Categories = categories,
                    Tags = tags,
                    SelectedTags = post.PostTags != null 
                        ? post.PostTags.Where(pt => pt.Tag != null).Select(pt => pt.TagId).ToList() 
                        : new List<int>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching post for edit");
                TempData["Error"] = "An error occurred while loading the post.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPostVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories!
                    .Where(c => c.Name != null)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                    
                model.Tags = await _context.Tags!
                    .Where(t => t.NameEn != null)
                    .OrderBy(t => t.NameEn)
                    .ToListAsync();
                return View(model);
            }

            try
            {
                var post = await _context.Posts
                    .Include(p => p.Category)
                    .Include(p => p.ApplicationUser)
                    .Include(p => p.PostTags)
                    .ThenInclude(p => p.Tag)
                    .FirstOrDefaultAsync(p => p.Id == model.Id);

                if (post == null)
                {
                    return NotFound();
                }

                if (await _context.Posts?.AnyAsync(x => (x.TitleAr == model.TitleAr || x.TitleEn == model.TitleEn) && x.Id != model.Id) == true)
                {
                    TempData["Error"] = "Post with the same title already exists";
                    return RedirectToAction(nameof(Index));
                }

                post.TitleAr = model.TitleAr?.Trim() ?? string.Empty;
                post.TitleEn = model.TitleEn?.Trim() ?? string.Empty;
                post.ShortDescriptionAr = model.ShortDescriptionAr?.Trim() ?? string.Empty;
                post.ShortDescriptionEn = model.ShortDescriptionEn?.Trim() ?? string.Empty;
                post.DescriptionAr = model.DescriptionAr?.Trim() ?? string.Empty;
                post.DescriptionEn = model.DescriptionEn?.Trim() ?? string.Empty;
                post.CategoryId = model.CategoryId;
                post.UpdateDate = DateTime.Now;

                // Update slug if title changed
                if (!string.IsNullOrEmpty(model.TitleAr) && model.TitleAr != post.TitleAr)
                {
                    string slug = model.TitleAr.Trim().ToLower();
                    slug = slug.Replace(" ", "-");
                    post.Slug = $"{slug}-{Guid.NewGuid()}";
                }
                else if (!string.IsNullOrEmpty(model.TitleEn) && model.TitleEn != post.TitleEn)
                {
                    string slug = model.TitleEn.Trim().ToLower();
                    slug = slug.Replace(" ", "-");
                    post.Slug = $"{slug}-{Guid.NewGuid()}";
                }

                // Update thumbnail
                if (model.Thumbnail != null)
                {
                    // Delete old thumbnail if it exists
                    if (!string.IsNullOrEmpty(post.ThumbnailUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, post.ThumbnailUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    post.ThumbnailUrl = UploadImage(model.Thumbnail);
                }

                // Update tags
                post.PostTags?.Clear();
                if (model.SelectedTags != null && model.SelectedTags.Count > 0)
                {
                    foreach (var tagId in model.SelectedTags)
                    {
                        var postTag = new PostTag
                        {
                            PostId = post.Id,
                            TagId = tagId
                        };
                        await _context.PostTags!.AddAsync(postTag);
                    }
                }

                _context.Update(post);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Post updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating post");
                ModelState.AddModelError("", "An error occurred while updating the post.");
                model.Categories = await _context.Categories!
                    .Where(c => c.Name != null)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                    
                model.Tags = await _context.Tags!
                    .Where(t => t.NameEn != null)
                    .OrderBy(t => t.NameEn)
                    .ToListAsync();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.PostTags)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (post == null)
                {
                    return NotFound();
                }

                // Delete the thumbnail file if it exists
                if (!string.IsNullOrEmpty(post.ThumbnailUrl))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, post.ThumbnailUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Post deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting post");
                TempData["Error"] = "An error occurred while deleting the post.";
                return RedirectToAction(nameof(Index));
            }
        }

        private string UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("No file was uploaded");
                }

                var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "blog");
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(folderPath, uniqueFileName);

                // Create directory if it doesn't exist
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                return $"/images/blog/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading image: {ex.Message}");
            }
        }
    }
}

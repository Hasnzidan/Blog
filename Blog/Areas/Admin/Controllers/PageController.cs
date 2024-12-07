using AspNetCoreHero.ToastNotification.Abstractions;
using Blog.Data;
using Blog.ViewModels;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotyfService _notification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<PageController> _logger;

        public PageController(ApplicationDbContext context,
                            INotyfService notification,
                            IWebHostEnvironment webHostEnvironment,
                            ILogger<PageController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _notification = notification ?? throw new ArgumentNullException(nameof(notification));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> About()
        {
            try
            {
                var page = await _context.Pages!.FirstOrDefaultAsync(x => x.Slug == "about");
                if (page == null)
                {
                    _logger.LogWarning("About page not found");
                    _notification.Error("About page not found");
                    return RedirectToAction("Index", "Home");
                }

                var vm = new PageVM()
                {
                    Id = page.Id,
                    TitleEn = page.TitleEn,
                    TitleAr = page.TitleAr,
                    ShortDescriptionEn = page.ShortDescriptionEn,
                    ShortDescriptionAr = page.ShortDescriptionAr,
                    ContentEn = page.ContentEn,
                    ContentAr = page.ContentAr,
                    ThumbnailUrl = page.ThumbnailUrl,
                    Published = page.Published,
                    CreatedDate = page.CreatedDate,
                    Slug = page.Slug
                };
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching about page");
                _notification.Error("An error occurred while fetching the about page");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> About(PageVM vm)
        {
            try
            {
                if (!ModelState.IsValid) 
                { 
                    _notification.Error("Please fix the validation errors");
                    return View(vm); 
                }

                if (vm == null)
                {
                    _logger.LogWarning("Invalid form submission for about page");
                    _notification.Error("Invalid form submission");
                    return RedirectToAction("About");
                }

                var page = await _context.Pages!.FirstOrDefaultAsync(x => x.Slug == "about");
                if (page == null)
                {
                    _logger.LogWarning("About page not found for update");
                    _notification.Error("Page not found");
                    return View(vm);
                }

                page.TitleEn = vm.TitleEn;
                page.TitleAr = vm.TitleAr;
                page.ShortDescriptionEn = vm.ShortDescriptionEn;
                page.ShortDescriptionAr = vm.ShortDescriptionAr;
                page.ContentEn = vm.ContentEn;
                page.ContentAr = vm.ContentAr;
                page.Published = vm.Published;

                if (vm.Thumbnail != null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(vm.Thumbnail.FileName);
                    string extension = Path.GetExtension(vm.Thumbnail.FileName);
                    
                    if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(extension))
                    {
                        fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath, "thumbnails", fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await vm.Thumbnail.CopyToAsync(fileStream);
                        }

                        // Delete old thumbnail if exists
                        if (!string.IsNullOrEmpty(page.ThumbnailUrl))
                        {
                            string oldImagePath = Path.Combine(wwwRootPath, page.ThumbnailUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        page.ThumbnailUrl = "/thumbnails/" + fileName;
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("About page updated successfully");
                _notification.Success("Page updated successfully");
                return RedirectToAction("About", "Page", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating about page");
                _notification.Error("An error occurred while updating the about page");
                return RedirectToAction("About");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Contact()
        {
            try
            {
                var page = await _context.Pages!.FirstOrDefaultAsync(x => x.Slug == "contact");
                if (page == null)
                {
                    _logger.LogWarning("Contact page not found");
                    _notification.Error("Contact page not found");
                    return RedirectToAction("Index", "Home");
                }

                var vm = new PageVM()
                {
                    Id = page.Id,
                    TitleEn = page.TitleEn,
                    TitleAr = page.TitleAr,
                    ShortDescriptionEn = page.ShortDescriptionEn,
                    ShortDescriptionAr = page.ShortDescriptionAr,
                    ContentEn = page.ContentEn,
                    ContentAr = page.ContentAr,
                    ThumbnailUrl = page.ThumbnailUrl,
                    Published = page.Published,
                    CreatedDate = page.CreatedDate,
                    Slug = page.Slug
                };
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching contact page");
                _notification.Error("An error occurred while fetching the contact page");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Contact(PageVM vm)
        {
            try
            {
                if (!ModelState.IsValid) 
                { 
                    _notification.Error("Please fix the validation errors");
                    return View(vm); 
                }

                if (vm == null)
                {
                    _logger.LogWarning("Invalid form submission for contact page");
                    _notification.Error("Invalid form submission");
                    return RedirectToAction("Contact");
                }

                var page = await _context.Pages!.FirstOrDefaultAsync(x => x.Slug == "contact");
                if (page == null)
                {
                    _logger.LogWarning("Contact page not found for update");
                    _notification.Error("Page not found");
                    return View(vm);
                }

                page.TitleEn = vm.TitleEn;
                page.TitleAr = vm.TitleAr;
                page.ShortDescriptionEn = vm.ShortDescriptionEn;
                page.ShortDescriptionAr = vm.ShortDescriptionAr;
                page.ContentEn = vm.ContentEn;
                page.ContentAr = vm.ContentAr;
                page.Published = vm.Published;

                if (vm.Thumbnail != null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(vm.Thumbnail.FileName);
                    string extension = Path.GetExtension(vm.Thumbnail.FileName);
                    
                    if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(extension))
                    {
                        fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath, "thumbnails", fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await vm.Thumbnail.CopyToAsync(fileStream);
                        }

                        // Delete old thumbnail if exists
                        if (!string.IsNullOrEmpty(page.ThumbnailUrl))
                        {
                            string oldImagePath = Path.Combine(wwwRootPath, page.ThumbnailUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        page.ThumbnailUrl = "/thumbnails/" + fileName;
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Contact page updated successfully");
                _notification.Success("Page updated successfully");
                return RedirectToAction("Contact", "Page", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating contact page");
                _notification.Error("An error occurred while updating the contact page");
                return RedirectToAction("Contact");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Privacy()
        {
            try
            {
                var page = await _context.Pages!.FirstOrDefaultAsync(x => x.Slug == "privacy-policy");
                if (page == null)
                {
                    _logger.LogWarning("Privacy policy page not found");
                    _notification.Error("Privacy policy page not found");
                    return RedirectToAction("Index", "Home");
                }

                var vm = new PageVM()
                {
                    Id = page.Id,
                    TitleEn = page.TitleEn,
                    TitleAr = page.TitleAr,
                    ShortDescriptionEn = page.ShortDescriptionEn,
                    ShortDescriptionAr = page.ShortDescriptionAr,
                    ContentEn = page.ContentEn,
                    ContentAr = page.ContentAr,
                    ThumbnailUrl = page.ThumbnailUrl,
                    Published = page.Published,
                    CreatedDate = page.CreatedDate,
                    Slug = page.Slug
                };
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching privacy policy page");
                _notification.Error("An error occurred while fetching the privacy policy page");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Privacy(PageVM vm)
        {
            try
            {
                if (!ModelState.IsValid) 
                { 
                    _notification.Error("Please fix the validation errors");
                    return View(vm); 
                }

                if (vm == null)
                {
                    _logger.LogWarning("Invalid form submission for privacy policy page");
                    _notification.Error("Invalid form submission");
                    return RedirectToAction("Privacy");
                }

                var page = await _context.Pages!.FirstOrDefaultAsync(x => x.Slug == "privacy-policy");
                if (page == null)
                {
                    _logger.LogWarning("Privacy policy page not found for update");
                    _notification.Error("Page not found");
                    return View(vm);
                }

                page.TitleEn = vm.TitleEn;
                page.TitleAr = vm.TitleAr;
                page.ShortDescriptionEn = vm.ShortDescriptionEn;
                page.ShortDescriptionAr = vm.ShortDescriptionAr;
                page.ContentEn = vm.ContentEn;
                page.ContentAr = vm.ContentAr;
                page.Published = vm.Published;

                if (vm.Thumbnail != null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(vm.Thumbnail.FileName);
                    string extension = Path.GetExtension(vm.Thumbnail.FileName);
                    
                    if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(extension))
                    {
                        fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath, "thumbnails", fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await vm.Thumbnail.CopyToAsync(fileStream);
                        }

                        // Delete old thumbnail if exists
                        if (!string.IsNullOrEmpty(page.ThumbnailUrl))
                        {
                            string oldImagePath = Path.Combine(wwwRootPath, page.ThumbnailUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        page.ThumbnailUrl = "/thumbnails/" + fileName;
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Privacy policy page updated successfully");
                _notification.Success("Page updated successfully");
                return RedirectToAction("Privacy", "Page", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating privacy policy page");
                _notification.Error("An error occurred while updating the privacy policy page");
                return RedirectToAction("Privacy");
            }
        }
    }
}
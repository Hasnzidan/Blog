using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blog.Models;
using Blog.Data;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.Extensions.Logging;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [AutoValidateAntiforgeryToken]
    public class TagController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotyfService _notification;
        private readonly ILogger<TagController> _logger;

        public TagController(
            ApplicationDbContext context,
            INotyfService notification,
            ILogger<TagController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _notification = notification ?? throw new ArgumentNullException(nameof(notification));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var tags = await _context.Tags
                    .Include(t => t.PostTags)
                    .OrderBy(t => t.NameEn)
                    .ToListAsync();
                return View(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching tags");
                _notification.Error("Failed to load tags");
                return View(new List<Tag>());
            }
        }

        public IActionResult Create()
        {
            return View(new Tag());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Tag tag)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check for duplicate names
                    if (await _context.Tags.AnyAsync(t =>
                        t.NameEn.Equals(tag.NameEn, StringComparison.OrdinalIgnoreCase) ||
                        t.NameAr.Equals(tag.NameAr, StringComparison.OrdinalIgnoreCase)))
                    {
                        ModelState.AddModelError("", "Tag with this name already exists");
                        return View(tag);
                    }

                    tag.Slug = GenerateSlug(tag.NameEn);
                    await _context.Tags.AddAsync(tag);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Tag created: {TagName}", tag.NameEn);
                    _notification.Success("Tag created successfully!");
                    return RedirectToAction(nameof(Index));
                }
                return View(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating tag: {TagName}", tag.NameEn);
                _notification.Error("Failed to create tag");
                return View(tag);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    _notification.Error("Tag ID is required");
                    return RedirectToAction(nameof(Index));
                }

                var tag = await _context.Tags.FindAsync(id);
                if (tag == null)
                {
                    _logger.LogWarning("Tag not found: {TagId}", id);
                    _notification.Error("Tag not found!");
                    return RedirectToAction(nameof(Index));
                }

                return View(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching tag for edit: {TagId}", id);
                _notification.Error("Failed to load tag");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Tag tag)
        {
            try
            {
                if (id != tag.Id)
                {
                    _notification.Error("Invalid tag ID");
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    // Check for duplicate names
                    if (await _context.Tags.AnyAsync(t =>
                        t.Id != tag.Id && (
                        t.NameEn.Equals(tag.NameEn, StringComparison.OrdinalIgnoreCase) ||
                        t.NameAr.Equals(tag.NameAr, StringComparison.OrdinalIgnoreCase))))
                    {
                        ModelState.AddModelError("", "Tag with this name already exists");
                        return View(tag);
                    }

                    tag.Slug = GenerateSlug(tag.NameEn);
                    _context.Update(tag);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Tag updated: {TagName}", tag.NameEn);
                    _notification.Success("Tag updated successfully!");
                    return RedirectToAction(nameof(Index));
                }
                return View(tag);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await TagExists(tag.Id))
                {
                    _logger.LogWarning(ex, "Tag not found during update: {TagId}", id);
                    _notification.Error("Tag not found!");
                    return RedirectToAction(nameof(Index));
                }
                
                _logger.LogError(ex, "Concurrency error while updating tag: {TagId}", id);
                _notification.Error("The tag was modified by another user. Please try again.");
                return View(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating tag: {TagId}", id);
                _notification.Error("Failed to update tag");
                return View(tag);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tag = await _context.Tags
                    .Include(t => t.PostTags)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tag == null)
                {
                    _logger.LogWarning("Tag not found for deletion: {TagId}", id);
                    _notification.Error("Tag not found!");
                    return RedirectToAction(nameof(Index));
                }

                if (tag.PostTags?.Any() == true)
                {
                    _notification.Warning("Cannot delete tag that is used in posts. Please remove the tag from all posts first.");
                    return RedirectToAction(nameof(Index));
                }

                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Tag deleted: {TagName}", tag.NameEn);
                _notification.Success("Tag deleted successfully!");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting tag: {TagId}", id);
                _notification.Error("Failed to delete tag");
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<bool> TagExists(int id)
        {
            return await _context.Tags.AnyAsync(e => e.Id == id);
        }

        private static string GenerateSlug(string name)
        {
            // Convert to lowercase and trim
            var slug = name.ToLowerInvariant().Trim();
            
            // Replace spaces with hyphens
            slug = Regex.Replace(slug, @"\s+", "-");
            
            // Remove invalid characters
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
            
            // Remove multiple hyphens
            slug = Regex.Replace(slug, @"-+", "-");
            
            // Trim hyphens from start and end
            return slug.Trim('-');
        }
    }
}

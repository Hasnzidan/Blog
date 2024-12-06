using AspNetCoreHero.ToastNotification.Abstractions;
using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [AutoValidateAntiforgeryToken]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotyfService _notification;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(
            ApplicationDbContext context,
            INotyfService notification,
            ILogger<CategoryController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _notification = notification ?? throw new ArgumentNullException(nameof(notification));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.Posts)
                    .OrderBy(c => c.NameEn)
                    .ToListAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching categories");
                _notification.Error("Failed to load categories");
                return View(new List<Category>());
            }
        }

        public IActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check for duplicate names
                    if (await _context.Categories.AnyAsync(c => 
                        c.NameEn.Equals(category.NameEn, StringComparison.OrdinalIgnoreCase) ||
                        c.NameAr.Equals(category.NameAr, StringComparison.OrdinalIgnoreCase)))
                    {
                        ModelState.AddModelError("", "Category with this name already exists");
                        return View(category);
                    }

                    await _context.Categories.AddAsync(category);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Category created: {CategoryName}", category.NameEn);
                    _notification.Success("Category created successfully!");
                    return RedirectToAction(nameof(Index));
                }
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating category: {CategoryName}", category.NameEn);
                _notification.Error("Failed to create category");
                return View(category);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    _logger.LogWarning("Category not found: {CategoryId}", id);
                    _notification.Error("Category not found!");
                    return RedirectToAction(nameof(Index));
                }
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching category for edit: {CategoryId}", id);
                _notification.Error("Failed to load category");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check for duplicate names
                    if (await _context.Categories.AnyAsync(c => 
                        c.Id != category.Id && (
                        c.NameEn.Equals(category.NameEn, StringComparison.OrdinalIgnoreCase) ||
                        c.NameAr.Equals(category.NameAr, StringComparison.OrdinalIgnoreCase))))
                    {
                        ModelState.AddModelError("", "Category with this name already exists");
                        return View(category);
                    }

                    _context.Categories.Update(category);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Category updated: {CategoryName}", category.NameEn);
                    _notification.Success("Category updated successfully!");
                    return RedirectToAction(nameof(Index));
                }
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating category: {CategoryId}", category.Id);
                _notification.Error("Failed to update category");
                return View(category);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Posts)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    _logger.LogWarning("Category not found for deletion: {CategoryId}", id);
                    _notification.Error("Category not found!");
                    return RedirectToAction(nameof(Index));
                }

                if (category.Posts?.Any() == true)
                {
                    _notification.Warning("Cannot delete category with associated posts. Please remove or reassign the posts first.");
                    return RedirectToAction(nameof(Index));
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Category deleted: {CategoryName}", category.NameEn);
                _notification.Success("Category deleted successfully!");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting category: {CategoryId}", id);
                _notification.Error("Failed to delete category");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Blog.Models;
using Blog.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blog.Controllers
{
    public class PageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PageController> _logger;

        public PageController(ApplicationDbContext context, ILogger<PageController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> About()
        {
            try
            {
                var page = await _context.Pages!
                    .FirstOrDefaultAsync(p => p.Slug == "about" && p.Published);

                if (page == null)
                {
                    _logger.LogWarning("About page not found");
                    return NotFound();
                }

                return View(page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving About page");
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> Contact()
        {
            try
            {
                var page = await _context.Pages!
                    .FirstOrDefaultAsync(p => p.Slug == "contact" && p.Published);

                if (page == null)
                {
                    _logger.LogWarning("Contact page not found");
                    return NotFound();
                }

                return View(page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Contact page");
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> PrivacyPolicy()
        {
            try
            {
                var page = await _context.Pages!
                    .FirstOrDefaultAsync(p => p.Slug == "privacy-policy" && p.Published);

                if (page == null)
                {
                    _logger.LogWarning("Privacy Policy page not found");
                    return NotFound();
                }

                return View(page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Privacy Policy page");
                return StatusCode(500);
            }
        }
    }
}

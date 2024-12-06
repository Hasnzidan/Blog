using Blog.Data;
using Blog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    public class PageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PageController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> About()
        {
            var page = await _context.Pages!.FirstOrDefaultAsync(x => x.Slug == "about");
            
            if (page == null)
            {
                return NotFound();
            }

            var vm = new PageVM()
            {
                Title = page.Title,
                Content = page.Content,
                ShortDescription = page.ShortDescription,
                ThumbnailUrl = page.ThumbnailUrl,
                CreatedDate = page.CreatedDate,
                Published = page.Published,
                Slug = page.Slug
            };
            return View(vm);
        }

        public async Task<IActionResult> Contact()
        {
            var page = await _context.Pages!.FirstOrDefaultAsync(x => x.Slug == "contact");
            
            if (page == null)
            {
                return NotFound();
            }

            var vm = new PageVM()
            {
                Title = page.Title,
                Content = page.Content,
                ShortDescription = page.ShortDescription,
                ThumbnailUrl = page.ThumbnailUrl,
                CreatedDate = page.CreatedDate,
                Published = page.Published,
                Slug = page.Slug
            };
            return View(vm);
        }

        public async Task<IActionResult> PrivacyPolicy()
        {
            var page = await _context.Pages!.FirstOrDefaultAsync(x => x.Slug == "privacy-policy");
            
            if (page == null)
            {
                return NotFound();
            }

            var vm = new PageVM()
            {
                Title = page.Title,
                Content = page.Content,
                ShortDescription = page.ShortDescription,
                ThumbnailUrl = page.ThumbnailUrl,
                CreatedDate = page.CreatedDate,
                Published = page.Published,
                Slug = page.Slug
            };
            return View(vm);
        }
    }
}

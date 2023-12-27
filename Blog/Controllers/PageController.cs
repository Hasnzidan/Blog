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

        public async Task <IActionResult> About()
        {
            var page = await _context.Pages!.FirstOrDefaultAsync(x => x.Slug== "About-Us");
            var vm = new PageVM()
            {
                Title = page!.Title,
                Description = page.Description,
                shortDescription = page.ShortDescription,
                ThumbnailUrl = page.ThumbnailUrl,

            };
            return View(vm);
        }
        public async Task<IActionResult> Contact()
        {
            var page = await _context.Pages!.FirstOrDefaultAsync(x => x.Slug == "Contact-Us");
            var vm = new PageVM()
            {
                Title = page!.Title,
                Description = page.Description,
                shortDescription = page.ShortDescription,
                ThumbnailUrl = page.ThumbnailUrl,

            };
            return View(vm);
        } public async Task<IActionResult> PrivacyPolicy()
        {
            var page = await _context.Pages!.FirstOrDefaultAsync(x => x.Slug == "Privacy");
            var vm = new PageVM()
            {
                Title = page!.Title,
                Description = page.Description,
                shortDescription = page.ShortDescription,
                ThumbnailUrl = page.ThumbnailUrl,

            };
            return View(vm);
        }
    }
}

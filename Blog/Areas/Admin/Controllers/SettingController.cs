
using AspNetCoreHero.ToastNotification.Abstractions;
using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class SettingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public INotyfService _notification { get; }
        public SettingController(
            ApplicationDbContext context,
            INotyfService notyfService,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _notification = notyfService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings = _context.Settings!.ToList();
            if (settings.Count > 0)
            {
                var vm = new SettingVM()
                {
                    Id = settings[0].Id,
                    SiteName = settings[0].SiteName,
                    ShortDescription = settings[0].ShortDescription,
                    ThumbnailUrl = settings[0].ThumbnailUrl,
                    FacebookUrl = settings[0].FacebookUrl,
                    GithubUrl = settings[0].GithubUrl,
                    Title = settings[0].Title,
                    TwitterUrl = settings[0].TwitterUrl,
                };
                return View(vm);
            }
            var setting = new Setting()
            {
                SiteName = "Demo Name"
            };

            await _context.Settings!.AddAsync(setting);
            await _context.SaveChangesAsync();

            var Createdsettings = _context.Settings!.ToList();
            var createdVm = new SettingVM()
            {
                Id = Createdsettings[0].Id,
                SiteName = Createdsettings[0].SiteName,
                ShortDescription = Createdsettings[0].ShortDescription,
                ThumbnailUrl = Createdsettings[0].ThumbnailUrl,
                FacebookUrl = Createdsettings[0].FacebookUrl,
                GithubUrl = Createdsettings[0].GithubUrl,
                Title = Createdsettings[0].Title,
                TwitterUrl = Createdsettings[0].TwitterUrl,
            };

            return View(createdVm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(SettingVM vm)
        {
            if (!ModelState.IsValid) { return View(vm); }
            var setting = await _context.Settings!.FirstOrDefaultAsync(x => x.Id == vm.Id);
            if (setting == null)
            {
                _notification.Error("Something went Wrong ");
                return View(vm);
            }
            setting.SiteName = vm.SiteName;
            setting.Title = vm.Title;
            setting.ShortDescription = vm.ShortDescription;
            setting.FacebookUrl = vm.FacebookUrl;
            setting.TwitterUrl = vm.TwitterUrl;
            setting.GithubUrl = vm.GithubUrl;
            if (vm.Thumbnail != null)
            {
                setting.ThumbnailUrl = UploadImage(vm.Thumbnail);
            }
            await _context.SaveChangesAsync();
            _notification.Success("Setting Updated Successfully");
            return RedirectToAction("Index", "Setting", new { area = "Admin" });

        }


        private string UploadImage(IFormFile file)
        {
            string uniqueFileName = "";
            var folderpath = Path.Combine(_webHostEnvironment.WebRootPath, "thumbnails");
            uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filepath = Path.Combine(folderpath, uniqueFileName);
            using (FileStream fileStream = System.IO.File.Create(filepath))
            {
                file.CopyTo(fileStream);
            }
            return uniqueFileName;
        }
    }

}
﻿using AspNetCoreHero.ToastNotification.Abstractions;
using Blog.Data;
using Blog.Models;
using Blog.Utilites;
using Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Areas.Admin.Controllers
{
        [Area("Admin")]
        [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        public INotyfService _notification {  get; }
        private readonly UserManager<ApplicationUser> _userManager;   
        private readonly IWebHostEnvironment _webHostEnvironment;
        public PostController(ApplicationDbContext context,
                                  INotyfService notyfService,
                                  IWebHostEnvironment webHostEnvironment,
                                  UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _notification = notyfService;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var listofPosts = new List<Post>();
            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            var loggedInUserRole = await _userManager.GetRolesAsync(loggedInUser!);
            if (loggedInUserRole[0] == WebsiteRoles.WebsiteAdmin)
            {
                listofPosts = await _context.Posts!.Include(x => x.ApplicationUser).ToListAsync();
            }
            else
            {
                listofPosts = await _context.Posts!.Include(x => x.ApplicationUser).Where(x=>x.ApplicationUser!.Id==loggedInUser!.Id ).ToListAsync();

            }

            var listofPostsVM = listofPosts.Select(x => new PostVM()
            {
                Id = x.Id,
                Title = x.Title,
                ThumnailUrl = x.ThumnmailUrl,
                CreatedDate = x.CreateDate,
                AuthorName = x.ApplicationUser!.FirstName + " " + x.ApplicationUser.LastName

            }).ToList();
            return View(listofPostsVM);

        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreatePostVM());  
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePostVM vm)
        {
            if (!ModelState.IsValid) { return View(vm); }


            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);


            var post = new Post();
            
            post.Title = vm.Title;
            post.Description = vm.Description;
            post.ShortDescription = vm.ShortDescription;
            post.ApplicationUserId = loggedInUser!.Id;

            if(post.Title != null)
            {
                string slug = vm.Title!.Trim();
                slug = slug.Replace(" ","-");
                post.Slug = slug + "-" + Guid.NewGuid();
            }

            if(vm.Thumbnail != null)
            {
                post.ThumnmailUrl = UploadImage(vm.Thumbnail);
            }

            await _context.Posts!.AddAsync(post);
            await _context.SaveChangesAsync();
            _notification.Success("post craeted successfully");
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts!.FirstOrDefaultAsync(x => id == x.Id);

            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            var loggedInUserRole = await _userManager.GetRolesAsync(loggedInUser!);
            if (loggedInUserRole[0] == WebsiteRoles.WebsiteAdmin  || loggedInUser?.Id == post?.ApplicationUserId )
            {
                 _context.Posts!.Remove(post!); 
                await _context.SaveChangesAsync();
                _notification.Success(" Post Deleted Successfully");
                return RedirectToAction("Index", "Post", new { area = "Admin" });

            }
            return View();
        }
        private string UploadImage(IFormFile file)
        {
            string uniqueFileName = "";
            var folderpath = Path.Combine(_webHostEnvironment.WebRootPath, "thumbnails");
            uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filepath = Path.Combine(folderpath, uniqueFileName);
            using(FileStream fileStream = System.IO.File.Create(filepath))
            {
                file.CopyTo(fileStream);
            }
            return uniqueFileName;
        }
        
    }
}

using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Utilites
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task Initialize()
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Any())
                {
                    await _context.Database.MigrateAsync();
                }

                // Add default roles if they don't exist
                var defaultRoles = new[] { "Admin", "User" };
                foreach (var roleName in defaultRoles)
                {
                    if (!string.IsNullOrEmpty(roleName) && !await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Add default admin user if no users exist
                if (!_userManager.Users.Any())
                {
                    var admin = new ApplicationUser
                    {
                        UserName = "admin@gmail.com",
                        Email = "admin@gmail.com",
                        FirstName = "Admin",
                        LastName = "User",
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(admin, "Admin123*");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(admin, "Admin");
                    }
                }

                // Add test categories if none exist
                if (!_context.Categories.Any())
                {
                    var categories = new List<Category>
                    {
                        new Category
                        {
                            NameEn = "Technology",
                            NameAr = "التكنولوجيا",
                            DescriptionEn = "Latest in tech",
                            DescriptionAr = "أحدث التقنيات",
                            Slug = "technology"
                        },
                        new Category
                        {
                            NameEn = "Travel",
                            NameAr = "السفر",
                            DescriptionEn = "Travel experiences",
                            DescriptionAr = "تجارب السفر",
                            Slug = "travel"
                        }
                    };

                    await _context.Categories.AddRangeAsync(categories);
                    await _context.SaveChangesAsync();
                }

                // Add test posts if none exist
                if (!_context.Posts.Any())
                {
                    var category = await _context.Categories.FirstOrDefaultAsync();
                    var admin = await _userManager.FindByEmailAsync("admin@gmail.com");

                    if (category != null && admin != null)
                    {
                        var posts = new List<Post>
                        {
                            new Post
                            {
                                TitleEn = "Welcome to Our Blog",
                                TitleAr = "مرحباً بكم في مدونتنا",
                                ShortDescriptionEn = "This is our first multilingual blog post",
                                ShortDescriptionAr = "هذه أول تدوينة متعددة اللغات",
                                ContentEn = "Welcome to our multilingual blog! We're excited to share content in both English and Arabic.",
                                ContentAr = "مرحباً بكم في مدونتنا متعددة اللغات! نحن متحمسون لمشاركة المحتوى باللغتين الإنجليزية والعربية.",
                                DescriptionEn = "Welcome to our multilingual blog! We're excited to share content in both English and Arabic.",
                                DescriptionAr = "مرحباً بكم في مدونتنا متعددة اللغات! نحن متحمسون لمشاركة المحتوى باللغتين الإنجليزية والعربية.",
                                Slug = "welcome-post",
                                CategoryId = category.Id,
                                ApplicationUserId = admin.Id,
                                CreateDate = DateTime.Now,
                                LastModifiedDate = DateTime.Now,
                                IsPublished = true
                            },
                            new Post
                            {
                                TitleEn = "Getting Started with ASP.NET Core",
                                TitleAr = "البدء مع ASP.NET Core",
                                ShortDescriptionEn = "Learn the basics of ASP.NET Core",
                                ShortDescriptionAr = "تعلم أساسيات ASP.NET Core",
                                ContentEn = "ASP.NET Core is a cross-platform framework for building modern web applications.",
                                ContentAr = "ASP.NET Core هو إطار عمل متعدد المنصات لبناء تطبيقات الويب الحديثة.",
                                DescriptionEn = "ASP.NET Core is a cross-platform framework for building modern web applications.",
                                DescriptionAr = "ASP.NET Core هو إطار عمل متعدد المنصات لبناء تطبيقات الويب الحديثة.",
                                Slug = "aspnet-core-basics",
                                CategoryId = category.Id,
                                ApplicationUserId = admin.Id,
                                CreateDate = DateTime.Now,
                                LastModifiedDate = DateTime.Now,
                                IsPublished = true
                            }
                        };

                        await _context.Posts.AddRangeAsync(posts);
                        await _context.SaveChangesAsync();
                    }
                }

                if (!_context.Pages!.Any())
                {
                    var page = new Page
                    {
                        TitleEn = "About Us",
                        TitleAr = "من نحن",
                        Slug = "about",
                        ContentEn = "This is the about us page content.",
                        ContentAr = "هذا هو محتوى صفحة من نحن",
                        Published = true,
                        CreatedDate = DateTime.Now
                    };

                    await _context.Pages!.AddAsync(page);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while seeding the database.", ex);
            }
        }
    }
}

using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data
{
    public static class DbSeeder
    {
        public static async Task SeedDefaultData(IServiceProvider service)
        {
            var userMgr = service.GetRequiredService<UserManager<IdentityUser>>();
            var roleMgr = service.GetRequiredService<RoleManager<IdentityRole>>();
            var context = service.GetRequiredService<ApplicationDbContext>();

            // Seed Roles
            await SeedRoles(roleMgr);

            // Seed Admin User
            await SeedAdminUser(userMgr);

            // Seed Categories
            await SeedCategories(context);

            // Seed Settings
            await SeedSettings(context);

            // Seed Sample Posts
            await SeedSamplePosts(context, userMgr);
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleMgr)
        {
            if (!await roleMgr.RoleExistsAsync("Admin"))
            {
                var role = new IdentityRole("Admin");
                await roleMgr.CreateAsync(role);
            }
        }

        private static async Task SeedAdminUser(UserManager<IdentityUser> userMgr)
        {
            if (await userMgr.FindByEmailAsync("admin@blog.com") == null)
            {
                var user = new IdentityUser
                {
                    UserName = "admin@blog.com",
                    Email = "admin@blog.com",
                    EmailConfirmed = true
                };

                var result = await userMgr.CreateAsync(user, "Admin@123");
                if (result.Succeeded)
                {
                    await userMgr.AddToRoleAsync(user, "Admin");
                }
            }
        }

        private static async Task SeedCategories(ApplicationDbContext context)
        {
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { NameEn = "Technology", NameAr = "التكنولوجيا" },
                    new Category { NameEn = "Health", NameAr = "الصحة" },
                    new Category { NameEn = "Sports", NameAr = "الرياضة" },
                    new Category { NameEn = "Science", NameAr = "العلوم" }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedSettings(ApplicationDbContext context)
        {
            if (!await context.Settings.AnyAsync())
            {
                var settings = new Setting
                {
                    SiteName = "My Blog",
                    Title = "Welcome to My Blog",
                    ShortDescription = "A multilingual blog about various topics",
                    ThumbnailUrl = "/images/logo.png",
                    FacebookUrl = "https://facebook.com/myblog",
                    TwitterUrl = "https://twitter.com/myblog",
                    GithubUrl = "https://github.com/myblog"
                };

                await context.Settings.AddAsync(settings);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedSamplePosts(ApplicationDbContext context, UserManager<IdentityUser> userMgr)
        {
            if (!await context.Posts.AnyAsync())
            {
                var admin = await userMgr.FindByEmailAsync("admin@blog.com");
                if (admin == null) return;

                var category = await context.Categories.FirstOrDefaultAsync();
                if (category == null) return;

                var post = new Post
                {
                    TitleEn = "Sample Post",
                    TitleAr = "مقال تجريبي",
                    ShortDescriptionEn = "This is a sample post",
                    ShortDescriptionAr = "هذا مقال تجريبي",
                    DescriptionEn = "This is a sample post content",
                    DescriptionAr = "هذا محتوى المقال التجريبي",
                    Slug = "sample-post",
                    ThumbnailUrl = "/images/blog/default.jpg",
                    CategoryId = category.Id,
                    ApplicationUserId = admin.Id,
                    IsPublished = true,
                    CreateDate = DateTime.UtcNow
                };

                await context.Posts.AddAsync(post);
                await context.SaveChangesAsync();
            }
        }
    }
}
